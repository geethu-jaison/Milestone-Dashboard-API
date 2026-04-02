using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using IconnectDashboardGateway.Application.Interfaces.Connection;
using Microsoft.Win32;

namespace IconnectDashboardGateway.Infrastructure.Connection
{
    /// <summary>
    /// Read-only: same registry path and crypto as SecureRegistryStore for K + D only.
    /// HKCU first, then HKLM. Does not write registry.
    /// </summary>
    public class RegistryConnectionStringProvider:IRegistryConnectionStringProvider
    {
        private const string RegRoot = @"SOFTWARE\7E3F1A9C-B2D4-4E6F-8A0C-5B3D7E9F1A2C";
        private const string ValueKey = "K";
        private const string ValueData = "D";
        private const byte XorKey = 0x5A;
        private const string Salt = "MonkeyOn1A#Car";
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);
        private readonly object _lock = new();
        private string? _cached;
        private DateTimeOffset _expiresUtc = DateTimeOffset.MinValue;
        public string GetConnectionString()
        {
            lock (_lock)
            {
                var now = DateTimeOffset.UtcNow;
                if (_cached is not null && now < _expiresUtc)
                    return _cached;
                var aesKey = GetRealKey();
                if (aesKey is null || aesKey.Length == 0)
                    throw new InvalidOperationException("Registry key K is missing or invalid.");
                var cs = GetConnectionString(aesKey);
                if (string.IsNullOrEmpty(cs))
                    throw new InvalidOperationException("Registry value D is missing or could not be decrypted.");
                _cached = cs;
                _expiresUtc = now.Add(CacheTtl);
                return _cached;
            }
        }
        public void InvalidateCache()
        {
            lock (_lock)
            {
                _cached = null;
                _expiresUtc = DateTimeOffset.MinValue;
            }
        }
        // --- Same logic as SecureRegistryStore (read path only) ---
        private static string? GetObfuscatedKey()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegRoot, false))
                {
                    if (key != null)
                    {
                        var raw = key.GetValue(ValueKey) as string;
                        if (!string.IsNullOrEmpty(raw))
                            return RemoveSaltFromKey(raw, Salt);
                    }
                }
                using (var key = Registry.LocalMachine.OpenSubKey(RegRoot, false))
                {
                    if (key == null)
                        return null;
                    var raw = key.GetValue(ValueKey) as string;
                    if (string.IsNullOrEmpty(raw))
                        return null;
                    return RemoveSaltFromKey(raw, Salt);
                }
            }
            catch
            {
                return null;
            }
        }
        private static byte[]? DeobfuscateKey(string obfuscatedKeyBase64)
        {
            if (string.IsNullOrEmpty(obfuscatedKeyBase64))
                return null;
            var bytes = Convert.FromBase64String(obfuscatedKeyBase64);
            for (var i = 0; i < bytes.Length; i++)
                bytes[i] ^= XorKey;
            return bytes;
        }
        private static byte[]? GetRealKey()
        {
            var obf = GetObfuscatedKey();
            if (string.IsNullOrEmpty(obf))
                return null;
            return DeobfuscateKey(obf);
        }
        private static string? GetConnectionString(byte[] aesKey)
        {
            if (aesKey == null || aesKey.Length == 0)
                return null;
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegRoot, false))
                {
                    if (key != null)
                    {
                        var raw = key.GetValue(ValueData) as byte[];
                        if (raw != null && raw.Length >= 17)
                            return DecryptAes(raw, aesKey);
                    }
                }
                using (var key = Registry.LocalMachine.OpenSubKey(RegRoot, false))
                {
                    if (key == null)
                        return null;
                    var raw = key.GetValue(ValueData) as byte[];
                    if (raw == null || raw.Length < 17)
                        return null;
                    return DecryptAes(raw, aesKey);
                }
            }
            catch
            {
                return null;
            }
        }
        private static string RemoveSaltFromKey(string saltedKey, string salt)
        {
            var saltedBytes = Convert.FromBase64String(saltedKey);
            var saltBytes = Encoding.UTF8.GetBytes(salt);
            using (var sha = SHA256.Create())
            {
                var saltHash = sha.ComputeHash(saltBytes);
                for (var i = 0; i < Math.Min(saltedBytes.Length, saltHash.Length); i++)
                    saltedBytes[i] ^= saltHash[i];
            }
            var combined = Encoding.UTF8.GetString(saltedBytes);
            var idx = combined.IndexOf('|');
            return idx >= 0 ? combined[..idx] : combined;
        }
        private static string? DecryptAes(byte[] encrypted, byte[] key)
        {
            if (encrypted == null || encrypted.Length < 17)
                return null;
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = key;
            var iv = new byte[16];
            Buffer.BlockCopy(encrypted, 0, iv, 0, 16);
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            using var dec = aes.CreateDecryptor();
            var cipher = new byte[encrypted.Length - 16];
            Buffer.BlockCopy(encrypted, 16, cipher, 0, cipher.Length);
            var plain = dec.TransformFinalBlock(cipher, 0, cipher.Length);
            return Encoding.UTF8.GetString(plain);
        }
    }
}
