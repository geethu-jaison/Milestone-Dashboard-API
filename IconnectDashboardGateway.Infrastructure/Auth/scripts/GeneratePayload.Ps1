#
.SYNOPSIS
  Build an AES-256-CBC encrypted auth payload using the same registry "K" logic as RegistryConnectionStringProvider.

.PARAMETER SiteId
  Unique site / child id (GUID string recommended). Not editable in the same way as display name.

.PARAMETER Secret
  Shared secret string included in plaintext (your protocol field).

.PARAMETER TimestampUnix
  Unix time in seconds (UTC). Default: now.

.PARAMETER RegistrySubKey
  Must match C# RegRoot (relative to HKCU/HKLM).

.PARAMETER Salt
  Must match C# Salt constant.

.PARAMETER XorKey
  Must match C# XorKey constant (0x5A).

.PARAMETER OverrideRegistryKBase64
  If set, use this string instead of reading registry "K" (for dev machines without registry values).
#>

param(
    [Parameter(Mandatory = $true)][string]$SiteId,
    [Parameter(Mandatory = $true)][string]$Secret,
    [Parameter(Mandatory = $false)][long]$TimestampUnix = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds(),

    [string]$RegistrySubKey = 'SOFTWARE\7E3F1A9C-B2D4-4E6F-8A0C-5B3D7E9F1A2C',
    [string]$Salt = 'MonkeyOn1A#Car',
    [byte]$XorKey = 0x5A,

    [string]$OverrideRegistryKBase64
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-RegistryKRawString {
    param([string]$SubKey)

    foreach ($rootName in @('CurrentUser', 'LocalMachine')) {
        $baseKey = [Microsoft.Win32.Registry]::$rootName
        $key = $baseKey.OpenSubKey($SubKey, $false)
        if ($null -ne $key) {
            try {
                $val = $key.GetValue('K')
                if ($val -is [string] -and -not [string]::IsNullOrWhiteSpace($val)) {
                    return $val
                }
            }
            finally {
                $key.Dispose()
            }
        }
    }
    return $null
}

function Remove-SaltFromKey {
    param(
        [Parameter(Mandatory = $true)][string]$SaltedKeyBase64,
        [Parameter(Mandatory = $true)][string]$Salt
    )

    $saltedBytes = [Convert]::FromBase64String($SaltedKeyBase64)
    $saltBytes = [Text.Encoding]::UTF8.GetBytes($Salt)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $saltHash = $sha.ComputeHash($saltBytes)
        $n = [Math]::Min($saltedBytes.Length, $saltHash.Length)
        for ($i = 0; $i -lt $n; $i++) {
            $saltedBytes[$i] = $saltedBytes[$i] -bxor $saltHash[$i]
        }
    }
    finally {
        $sha.Dispose()
    }

    $combined = [Text.Encoding]::UTF8.GetString($saltedBytes)
    $idx = $combined.IndexOf('|')
    if ($idx -ge 0) { return $combined.Substring(0, $idx) }
    return $combined
}

function Deobfuscate-KeyBytes {
    param(
        [Parameter(Mandatory = $true)][string]$ObfuscatedKeyBase64,
        [Parameter(Mandatory = $true)][byte]$XorKey
    )

    $bytes = [Convert]::FromBase64String($ObfuscatedKeyBase64)
    for ($i = 0; $i -lt $bytes.Length; $i++) {
        $bytes[$i] = $bytes[$i] -bxor $XorKey
    }
    return ,$bytes
}

function Get-Aes256KeyFromRegistryK {
    param(
        [string]$RegistrySubKey,
        [string]$Salt,
        [byte]$XorKey,
        [string]$OverrideRegistryKBase64
    )

    $rawK = if ($OverrideRegistryKBase64) { $OverrideRegistryKBase64 } else { Get-RegistryKRawString -SubKey $RegistrySubKey }
    if ([string]::IsNullOrWhiteSpace($rawK)) {
        throw "Registry value K not found under HKCU or HKLM at '$RegistrySubKey'. Use -OverrideRegistryKBase64 for testing."
    }

    $obfB64 = Remove-SaltFromKey -SaltedKeyBase64 $rawK -Salt $Salt
    if ([string]::IsNullOrWhiteSpace($obfB64)) {
        throw "RemoveSaltFromKey returned empty string."
    }

    $keyBytes = Deobfuscate-KeyBytes -ObfuscatedKeyBase64 $obfB64 -XorKey $XorKey
    if ($keyBytes.Length -ne 32) {
        throw "Derived AES key length is $($keyBytes.Length) bytes; expected 32 for AES-256. Check registry K format."
    }

    return ,$keyBytes
}

function Protect-AuthPayloadAesCbc {
    param(
        [Parameter(Mandatory = $true)][byte[]]$AesKey32,
        [Parameter(Mandatory = $true)][string]$PlaintextUtf8
    )

    $aes = [System.Security.Cryptography.Aes]::Create()
    try {
        $aes.KeySize = 256
        $aes.Mode = [System.Security.Cryptography.CipherMode]::CBC
        $aes.Padding = [System.Security.Cryptography.PaddingMode]::PKCS7
        $aes.Key = $AesKey32
        $aes.GenerateIV()

        $enc = $aes.CreateEncryptor()
        $plainBytes = [Text.Encoding]::UTF8.GetBytes($PlaintextUtf8)
        $cipher = $enc.TransformFinalBlock($plainBytes, 0, $plainBytes.Length)

        $out = New-Object byte[] (16 + $cipher.Length)
        [Array]::Copy($aes.IV, 0, $out, 0, 16)
        [Array]::Copy($cipher, 0, $out, 16, $cipher.Length)
        return ,$out
    }
    finally {
        $aes.Dispose()
    }
}

# --- main ---

$key = Get-Aes256KeyFromRegistryK -RegistrySubKey $RegistrySubKey -Salt $Salt -XorKey $XorKey -OverrideRegistryKBase64 $OverrideRegistryKBase64

# Plaintext: unique SiteId + secret + timestamp (no display name — can change on child)
$plain = "$SiteId|$Secret|$TimestampUnix"

$encryptedBytes = Protect-AuthPayloadAesCbc -AesKey32 $key -PlaintextUtf8 $plain
$encryptedB64 = [Convert]::ToBase64String($encryptedBytes)

[pscustomobject]@{
    SiteId            = $SiteId
    TimestampUnix     = $TimestampUnix
    PlaintextPreview  = "$SiteId|<secret>|$TimestampUnix"
    EncryptedPayload  = $encryptedB64
} | Format-List