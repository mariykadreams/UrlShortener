using System.Security.Cryptography;
using System.Text;

namespace UrlShortener.API.Services
{
    public class UrlShorteningService : IUrlShorteningService
    {
        private const int ShortCodeLength = 7;

        private const int MaxRetries = 5;

        public string GenerateShortCode(string originalUrl)
        {
            for (int retryCount = 0; retryCount < MaxRetries; retryCount++)
            {
                string inputToHash = originalUrl + (retryCount > 0 ? Guid.NewGuid().ToString() : string.Empty);

                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(inputToHash));

                    long hashValue = BitConverter.ToInt64(hashBytes, 0);

                    hashValue = Math.Abs(hashValue);

                    string shortCode = Base62Converter.Encode(hashValue);

                    if (shortCode.Length > ShortCodeLength)
                    {
                        shortCode = shortCode.Substring(0, ShortCodeLength);
                    }
                    else if (shortCode.Length < ShortCodeLength)
                    {
                        shortCode = shortCode.PadRight(ShortCodeLength, Base62Converter.Alphabet[0]);
                    }
                    return shortCode;
                }
            }

            throw new InvalidOperationException("Failed to generate a unique short code after several attempts.");
        }
    }
    public static class Base62Converter
    {
        public const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private static readonly int Base = Alphabet.Length;
        public static string Encode(long number)
        {
            if (number == 0)
            {
                return Alphabet[0].ToString();
            }

            StringBuilder sb = new StringBuilder();
            while (number > 0)
            {
                sb.Insert(0, Alphabet[(int)(number % Base)]);
                number /= Base;
            }
            return sb.ToString();
        }

        public static long Decode(string base62String)
        {
            long number = 0;
            for (int i = 0; i < base62String.Length; i++)
            {
                number = number * Base + Alphabet.IndexOf(base62String[i]);
            }
            return number;
        }
    }
}
