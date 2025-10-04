using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class SaveSecret
{
    private const int KEY_LEN = 32; // 256-bit
    private static string cachedSecretPath;
    public static void Init()
    {
        cachedSecretPath = Path.Combine(Application.persistentDataPath, "SaveGame", "secret.bin");
    }
    public static byte[] GetOrCreateKey()
    {
        try
        {
            if (string.IsNullOrEmpty(cachedSecretPath))
            {
                // fallback: vẫn đảm bảo chạy nếu dev quên Init
                cachedSecretPath = Path.Combine(Application.persistentDataPath, "SaveGame", "secret.bin");
            }

            string dir = Path.GetDirectoryName(cachedSecretPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            if (File.Exists(cachedSecretPath))
            {
                var k = File.ReadAllBytes(cachedSecretPath);
                if (k.Length == KEY_LEN) return k;
            }

            byte[] key = new byte[KEY_LEN];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(key);
            File.WriteAllBytes(cachedSecretPath, key);
            return key;
        }
        catch (Exception ex)
        {
            Debug.LogError("[SaveSecret] Failed to get/create key: " + ex);
            return Encoding.UTF8.GetBytes("fallback_key_32_bytes_________")
                .AsSpan(0, KEY_LEN).ToArray();
        }
    }
}

public static class SaveCrypto
{
    private const string MAGIC = "SV1"; 
    private const int SALT_LEN = 16;
    private const int IV_LEN = 16;
    private const int HMAC_LEN = 32;
    private const int PBKDF2_ITERS = 150_000;

    public static byte[] EncryptString(string plaintext, byte[] masterKey)
    {
        byte[] salt = new byte[SALT_LEN];
        using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);

        byte[] keyMaterial = PBKDF2(masterKey, salt, PBKDF2_ITERS, 64);
        byte[] encKey = new byte[32];
        byte[] macKey = new byte[32];
        Buffer.BlockCopy(keyMaterial, 0, encKey, 0, 32);
        Buffer.BlockCopy(keyMaterial, 32, macKey, 0, 32);

        byte[] iv;
        byte[] cipher;
        using (var aes = Aes.Create())
        {
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Key = encKey;
            aes.GenerateIV();
            iv = aes.IV;

            using var encryptor = aes.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plaintext);
            cipher = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        }

        byte[] magic = Encoding.ASCII.GetBytes(MAGIC);
        byte[] prefix = Concat(magic, salt, iv, cipher);

        byte[] hmac = ComputeHmac(macKey, prefix);
        return Concat(prefix, hmac);
    }

    public static string DecryptToString(byte[] payload, byte[] masterKey)
    {
        if (payload == null || payload.Length < 3 + SALT_LEN + IV_LEN + HMAC_LEN)
            throw new Exception("Payload too short");

        var magic = Encoding.ASCII.GetString(payload, 0, 3);
        if (magic != MAGIC) throw new Exception("Invalid magic header");

        int offset = 3;
        byte[] salt = Sub(payload, offset, SALT_LEN); offset += SALT_LEN;
        byte[] iv = Sub(payload, offset, IV_LEN); offset += IV_LEN;

        int cipherLen = payload.Length - offset - HMAC_LEN;
        if (cipherLen <= 0) throw new Exception("Cipher length invalid");
        byte[] cipher = Sub(payload, offset, cipherLen); offset += cipherLen;

        byte[] hmacGiven = Sub(payload, offset, HMAC_LEN);

        // derive keys
        byte[] keyMaterial = PBKDF2(masterKey, salt, PBKDF2_ITERS, 64);
        byte[] encKey = new byte[32];
        byte[] macKey = new byte[32];
        Buffer.BlockCopy(keyMaterial, 0, encKey, 0, 32);
        Buffer.BlockCopy(keyMaterial, 32, macKey, 0, 32);

        // verify HMAC
        byte[] prefix = Concat(Encoding.ASCII.GetBytes(MAGIC), salt, iv, cipher);
        byte[] hmacExpect = ComputeHmac(macKey, prefix);
        if (!ConstantTimeEquals(hmacGiven, hmacExpect))
            throw new CryptographicException("HMAC mismatch");

        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Key = encKey;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        byte[] plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(plain);
    }

    public static bool LooksEncrypted(byte[] payload)
    {
        if (payload == null || payload.Length < 3) return false;
        return payload[0] == (byte)'S' && payload[1] == (byte)'V' && payload[2] == (byte)'1';
    }

    // ---- helpers ----
    private static byte[] PBKDF2(byte[] password, byte[] salt, int iters, int outLen)
    {
        using var kdf = new Rfc2898DeriveBytes(password, salt, iters, HashAlgorithmName.SHA256);
        return kdf.GetBytes(outLen);
    }
    private static byte[] ComputeHmac(byte[] key, byte[] data)
    {
        using var h = new HMACSHA256(key);
        return h.ComputeHash(data);
    }
    private static bool ConstantTimeEquals(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;
        int diff = 0;
        for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
        return diff == 0;
    }
    private static byte[] Concat(params byte[][] chunks)
    {
        int len = 0; foreach (var c in chunks) len += c.Length;
        byte[] all = new byte[len];
        int o = 0;
        foreach (var c in chunks) { Buffer.BlockCopy(c, 0, all, o, c.Length); o += c.Length; }
        return all;
    }
    private static byte[] Sub(byte[] src, int offset, int count)
    {
        byte[] r = new byte[count];
        Buffer.BlockCopy(src, offset, r, 0, count);
        return r;
    }
}
