using BauphysikToolWPF.Models.Domain;
using System;
using System.Security.Cryptography;
using System.Text;

namespace BauphysikToolWPF.Services.Application
{
    public static class Integrity
    {
        public static byte[] ComputeHmac(byte[] data, byte[] key)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(data);
        }

        public static string ComputeHmacHex(string payload, byte[] key)
        {
            var bytes = Encoding.UTF8.GetBytes(payload);
            return BitConverter.ToString(ComputeHmac(bytes, key)).Replace("-", "").ToLowerInvariant();
        }

        public static bool VerifyHmac(string payload, string expectedHex, byte[] key)
        {
            var expectedBytes = ConvertHexStringToBytes(expectedHex);
            var actual = ComputeHmac(Encoding.UTF8.GetBytes(payload), key);
            return CryptographicOperations.FixedTimeEquals(actual, expectedBytes);
        }

        // TODO: improve
        public static bool ValidateProject(Project p)
        {
            // Example checks — adapt to your domain
            if (p.Name.Length > 256) return false;
            if (p.Elements != null && p.Elements.Count > 10000) return false; // limit sizes
            // validate IDs, ranges, etc.
            return true;
        }

        private static byte[] ConvertHexStringToBytes(string hex)
        {
            int len = hex.Length / 2;
            var result = new byte[len];
            for (int i = 0; i < len; i++)
                result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return result;
        }
    }
}
