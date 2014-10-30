using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Pirate.Util
{
  /// <summary>
  /// Handles cryptographic operations.
  /// </summary>
  public static class Crypto
  {
    private static RandomNumberGenerator rng;

    /// <summary>
    /// Gets the bytes of randomness.
    /// </summary>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static byte[] GetRandom(int length)
    {
      lock (typeof(Crypto))
      {
        if (rng == null)
        {
          rng = RandomNumberGenerator.Create();
        }

        var buffer = new byte[length];
        rng.GetBytes(buffer);
        return buffer;
      }
    }

    /// <summary>
    /// Hashes the specified data.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns></returns>
    public static byte[] Hash(byte[] data)
    {
      using (var sha = new SHA512Managed())
      {
        return sha.ComputeHash(data);
      }
    }

    public static byte[] AppendHashMac(byte[] data, byte[] key)
    {
      using (var hmac = new HMACSHA256())
      {
        hmac.Key = key;
        var hash = hmac.ComputeHash(data);
        return data.Concat(hash);
      }
    }

    public static byte[] VerifyAndRemoveHashMac(byte[] data, byte[] key)
    {
      using (var hmac = new HMACSHA256())
      {
        hmac.Key = key;
        var hashLength = hmac.HashSize / 8;

        if (data.Length >= hashLength)
        {
          var dataData = data.Part(0, data.Length - hashLength);
          var hashData = data.Part(data.Length - hashLength);
          var hash = hmac.ComputeHash(dataData);

          if (hash.Equal(hashData))
          {
            return dataData;
          }
          else
          {
            return null;
          }
        }
        else
        {
          return null;
        }
      }
    }

    /// <summary>
    /// Encrypts the plaintext with the key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="plaintext">The plaintext.</param>
    /// <returns></returns>
    public static byte[] Encrypt(byte[] key, byte[] plaintext)
    {
      var iv = GetRandom(16);
      var ciphertext = Encrypt(key, iv, plaintext);
      return iv.Concat(ciphertext);
    }

    /// <summary>
    /// Encrypts the plaintext with the key using the iv.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="iv">The iv.</param>
    /// <param name="plaintext">The plaintext.</param>
    /// <returns></returns>
    public static byte[] Encrypt(byte[] key, byte[] iv, byte[] plaintext)
    {
      using (var aes = new RijndaelManaged())
      {
        aes.Key = key;
        aes.IV = iv;
        aes.Padding = PaddingMode.ISO10126;
        
        MemoryStream memory = null;
        CryptoStream crypto = null;

        try
        {
          memory = new MemoryStream();
          crypto = new CryptoStream(memory, aes.CreateEncryptor(), CryptoStreamMode.Write);
          crypto.Write(plaintext, 0, plaintext.Length);
          crypto.FlushFinalBlock();
          return memory.ToArray();
        }
        finally
        {
          if (crypto != null)
          {
            crypto.Close();
            crypto = null;
            memory = null;
          }
          else if (memory != null)
          {
            memory.Close();
            memory = null;
          }
        }
      }
    }

    /// <summary>
    /// Decrypts the plaintext with the key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="ivAndCiphertext">The iv and ciphertext.</param>
    /// <returns></returns>
    public static byte[] Decrypt(byte[] key, byte[] ivAndCiphertext)
    {
      var iv = ivAndCiphertext.Part(0, 16);
      var ciphertext = ivAndCiphertext.Part(16);
      return Decrypt(key, iv, ciphertext);
    }

    /// <summary>
    /// Decrypts the plaintext with the key using the iv.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="iv">The iv.</param>
    /// <param name="ciphertext">The ciphertext.</param>
    /// <returns></returns>
    public static byte[] Decrypt(byte[] key, byte[] iv, byte[] ciphertext)
    {
      using (var aes = new RijndaelManaged())
      {
        aes.Key = key;
        aes.IV = iv;
        aes.Padding = PaddingMode.ISO10126;

        MemoryStream memory = null;
        CryptoStream crypto = null;

        try
        {
          memory = new MemoryStream();
          crypto = new CryptoStream(memory, aes.CreateDecryptor(), CryptoStreamMode.Write);
          crypto.Write(ciphertext, 0, ciphertext.Length);
          crypto.FlushFinalBlock();
          return memory.ToArray();
        }
        finally
        {
          if (crypto != null)
          {
            crypto.Close();
            crypto = null;
            memory = null;
          }
          else if (memory != null)
          {
            memory.Close();
            memory = null;
          }
        }
      }
    }
  }
}
