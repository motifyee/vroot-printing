
using System.Security.Cryptography;
using System.Text;

namespace PrintingLibrary.EncryptUtils;

public static class EncryptUtil {

  public static string GenerateMetaHash(string filePath) {
    byte[] fileBytes = File.ReadAllBytes(filePath);
    var md5Hash = MD5.HashData(fileBytes);
    var sha1Hash = SHA1.HashData(fileBytes);
    var sha256Hash = SHA256.HashData(fileBytes);

    // Concatenate all hashes
    var combined = md5Hash.Concat(sha1Hash).Concat(sha256Hash).ToArray();

    // Hash the combined hashes for fixed output size
    var result = SHA256.HashData(combined);

    return Convert.ToHexString(result);
  }


  public static string GetFileHash(string filePath, string secret) {
    var keyBytes = Encoding.UTF8.GetBytes(secret);
    using var hmac = new HMACSHA256(keyBytes);
    using var stream = System.IO.File.OpenRead(filePath);
    var hashBytes = hmac.ComputeHash(stream);
    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
  }
}