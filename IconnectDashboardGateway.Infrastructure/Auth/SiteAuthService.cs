using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using IconnectDashboardGateway.Application.Interfaces.Auth;
using IconnectDashboardGateway.Application.Interfaces.Connection;
using IconnectDashboardGateway.Application.Interfaces.Logger;
using IconnectDashboardGateway.Contracts.Dtos.Auth;
using IconnectDashboardGateway.Contracts.Dtos.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;

namespace IconnectDashboardGateway.Infrastructure.Auth
{
    [Obfuscation(Exclude = false, ApplyToMembers = true)]
    public class SiteAuthService : ISiteAuthService
    {
        private readonly IRegistryConnectionStringProvider _connectionStringProvider;
        private readonly IAppLogger _appLogger;

        private const string FallbackSecret = "Rdt#21.04.26";
        // in memory token store 
        private static readonly ConcurrentDictionary<string, TokenEntry> _tokens=new();


        private const int TokenTtlMinutes = 30;
        private const int MaxClockSkewSeconds = 300;

        private const string RegRoot = @"SOFTWARE\7E3F1A9C-B2D4-4E6F-8A0C-5B3D7E9F1A2C";
        private const string ValueKey = "K";
        private const byte XorKey = 0x5A;
        private const string Salt = "MonkeyOn1A#Car";
        public SiteAuthService(IAppLogger appLogger, IRegistryConnectionStringProvider registryConnectionStringProvider)
        {
            _appLogger = appLogger;
            _connectionStringProvider = registryConnectionStringProvider;
        }

        public async Task<JsonResponseModel<ParentHandshakeResponseDto>> ValidateHandshakeAndIssueTokenAsync(string payload, CancellationToken cancellationToken = default)
        {
            try
            {
                var aesKey = GetRealKey();
                if (aesKey is null || aesKey.Length != 32)
                    return JsonResponseModel<ParentHandshakeResponseDto>.Fail("Auth key not available.");
                var plain = DecryptAes(Convert.FromBase64String(payload), aesKey);
                if (string.IsNullOrWhiteSpace(plain))
                    return JsonResponseModel<ParentHandshakeResponseDto>.Fail("Unable to decrypt payload.");
                // expected: siteId|secret|timestampUnix
                var parts = plain.Split('|');
                if (parts.Length != 3)
                    return JsonResponseModel<ParentHandshakeResponseDto>.Fail("Invalid payload format.");
                var siteId = parts[0];
                var secret = parts[1];
                var timestampStr = parts[2];
                if (string.IsNullOrEmpty(siteId))
                    return JsonResponseModel<ParentHandshakeResponseDto>.Fail("siteId is not there in the payload.");
                if (!long.TryParse(timestampStr, out var unixTs))
                    return JsonResponseModel<ParentHandshakeResponseDto>.Fail("Invalid timestamp.");
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (Math.Abs(now - unixTs) > MaxClockSkewSeconds)
                    return JsonResponseModel<ParentHandshakeResponseDto>.Fail("Timestamp expired.");
                var siteOk = await ValidateSiteSecretAsync(siteId, secret, cancellationToken);
                if (!siteOk)
                    return JsonResponseModel<ParentHandshakeResponseDto>.Fail("Site authentication failed.");
                var token = GenerateToken();
                var tokenHash = HashToken(token);
                var expiresUtc = DateTime.UtcNow.AddMinutes(TokenTtlMinutes);
                _tokens[tokenHash] = new TokenEntry
                {
                    SiteId = siteId,
                    ExpiresUtc = expiresUtc
                };
                var data = new ParentHandshakeResponseDto
                {
                    AccessToken = token,
                    ExpiresUtc = expiresUtc
                };
                return JsonResponseModel<ParentHandshakeResponseDto>.Ok(data, "Handshake successful.");

            }
            catch (Exception ex)
            {
                _appLogger.LogError("Error validating handshake and issuing token", ex);
                return JsonResponseModel<ParentHandshakeResponseDto>.Fail("failed to get token");
            }
        }

        public Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default)
        {
            try
            {
                CleanupExpiredTokens();
                var tokenHash = HashToken(token);
                if (!_tokens.TryGetValue(tokenHash, out var entry))
                    return Task.FromResult(false);
                if (entry.ExpiresUtc <= DateTime.UtcNow)
                {
                    _tokens.TryRemove(tokenHash, out _);
                    return Task.FromResult(false);
                }
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
        private async Task<bool> ValidateSiteSecretAsync(string siteId, string secret, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(siteId))
                return false;

            const string sql = @"
                                IF EXISTS (SELECT 1 FROM dbo.SiteInfo WHERE SiteId = @SiteId)
                                    SELECT CAST(1 AS INT);
                                ELSE
                                    SELECT CAST(0 AS INT);";

            await using var con = new SqlConnection(_connectionStringProvider.GetConnectionString());
            await con.OpenAsync(ct);

            var exists = await con.QuerySingleAsync<int>(
                new CommandDefinition(sql, new { SiteId = siteId }, cancellationToken: ct));

            return exists == 1;
        }
        private static string GenerateToken()
        {
            Span<byte> bytes = stackalloc byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }
        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }
        private static void CleanupExpiredTokens()
        {
            var now = DateTime.UtcNow;
            foreach (var kv in _tokens)
            {
                if (kv.Value.ExpiresUtc <= now)
                    _tokens.TryRemove(kv.Key, out _);
            }
        }
        // ---- Registry key extraction (same pattern as your provider) ----
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
                    if (key == null) return null;
                    var raw = key.GetValue(ValueKey) as string;
                    if (string.IsNullOrEmpty(raw)) return null;
                    return RemoveSaltFromKey(raw, Salt);
                }
            }
            catch
            {
                return null;
            }
        }
        private static byte[]? GetRealKey()
        {
            var obf = GetObfuscatedKey();
            if (string.IsNullOrEmpty(obf)) return null;
            var bytes = Convert.FromBase64String(obf);
            for (var i = 0; i < bytes.Length; i++)
                bytes[i] ^= XorKey;
            return bytes;
        }
        private static string RemoveSaltFromKey(string saltedKey, string salt)
        {
            var saltedBytes = Convert.FromBase64String(saltedKey);
            var saltBytes = Encoding.UTF8.GetBytes(salt);
            using var sha = SHA256.Create();
            var saltHash = sha.ComputeHash(saltBytes);
            for (var i = 0; i < Math.Min(saltedBytes.Length, saltHash.Length); i++)
                saltedBytes[i] ^= saltHash[i];
            var combined = Encoding.UTF8.GetString(saltedBytes);
            var idx = combined.IndexOf('|');
            return idx >= 0 ? combined[..idx] : combined;
        }
        private static string? DecryptAes(byte[] encrypted, byte[] key)
        {
            if (encrypted.Length < 17) return null;
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            var iv = new byte[16];
            Buffer.BlockCopy(encrypted, 0, iv, 0, 16);
            aes.IV = iv;
            var cipher = new byte[encrypted.Length - 16];
            Buffer.BlockCopy(encrypted, 16, cipher, 0, cipher.Length);
            using var dec = aes.CreateDecryptor();
            var plain = dec.TransformFinalBlock(cipher, 0, cipher.Length);
            return Encoding.UTF8.GetString(plain);
        }


        private sealed class TokenEntry
        {
            public string SiteId { get; set; } = string.Empty;
            public DateTime ExpiresUtc { get; set; }
        }


    }
}
