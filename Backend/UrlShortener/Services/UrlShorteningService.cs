using System.Security.Cryptography;
using System.Text;

namespace UrlShortener.API.Services
{
    /// <summary>
    /// Служба, отвечающая за генерацию уникальных коротких URL-адресов с использованием хеширования и кодирования Base62.
    /// Эта реализация включает концептуальную стратегию разрешения коллизий.
    /// </summary>
    public class UrlShorteningService : IUrlShorteningService
    {
        // Определяем желаемую длину короткого кода.
        // 7-символьная строка Base62 может представлять 62^7 уникальных URL (приблизительно 3,5 миллиарда).
        private const int ShortCodeLength = 7;
        // Максимальное количество повторных попыток для разрешения коллизий.
        private const int MaxRetries = 5;

        /// <summary>
        /// Генерирует уникальный короткий код для данного исходного URL с использованием хеширования SHA256
        /// и кодирования Base62, с базовым механизмом разрешения коллизий.
        /// </summary>
        /// <param name="originalUrl">Исходный URL для сокращения.</param>
        /// <returns>Уникальный короткий код, закодированный в Base62.</returns>
        public string GenerateShortCode(string originalUrl)
        {
            for (int retryCount = 0; retryCount < MaxRetries; retryCount++)
            {
                // Создаем уникальный вход для хеширования, добавляя счетчик повторных попыток (seed).
                // В реальном сценарии вы будете хешировать originalUrl
                // и затем проверять наличие коллизий в вашей базе данных. Если коллизия найдена,
                // вы повторите попытку с измененным входом (например, добавив соль или счетчик повторных попыток).
                string inputToHash = originalUrl + (retryCount > 0 ? Guid.NewGuid().ToString() : string.Empty);

                using (SHA256 sha256 = SHA256.Create())
                {
                    // Вычисляем хеш входной строки.
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(inputToHash));

                    // Берем часть хеша. Использование 6 байтов (48 бит) обычно
                    // приводит к строке Base62 длиной около 7 символов.
                    // Нам нужно преобразовать эти байты в длинное целое число для использования Base62Converter.
                    // Убедитесь, что мы не выходим за пределы, если hashBytes короче 6 байтов,
                    // хотя SHA256 производит 32 байта.
                    long hashValue = BitConverter.ToInt64(hashBytes, 0);

                    // Убедитесь, что значение хеша положительно для кодирования Base62.
                    hashValue = Math.Abs(hashValue);

                    // Кодируем значение хеша в строку Base62.
                    string shortCode = Base62Converter.Encode(hashValue);

                    // Обрезаем или дополняем короткий код до желаемой длины.
                    // Если хеш производит более короткую строку, она может быть концептуально дополнена
                    // или могут быть использованы дополнительные байты хеша. В этом примере мы обрезаем до длины.
                    if (shortCode.Length > ShortCodeLength)
                    {
                        shortCode = shortCode.Substring(0, ShortCodeLength);
                    }
                    else if (shortCode.Length < ShortCodeLength)
                    {
                        // Дополняем символами из алфавита, если слишком короткий.
                        // Это упрощенное дополнение. Надежное решение может перегенерировать или использовать больше хеша.
                        shortCode = shortCode.PadRight(ShortCodeLength, Base62Converter.Alphabet[0]);
                    }

                    // В реальном приложении здесь вы бы проверяли, существует ли 'shortCode'
                    // уже в вашей базе данных.
                    // В этом примере мы предполагаем, что он уникален после нескольких попыток.
                    // if (!IsShortCodeTaken(shortCode)) // Заполнитель для проверки базы данных
                    // {
                    //     return shortCode;
                    // }

                    // Для демонстрации мы просто вернем первый сгенерированный код,
                    // который соответствует длине, но цикл показывает логику повторных попыток.
                    // В реальной системе проверка базы данных определила бы, когда прервать.
                    return shortCode;
                }
            }

            // Если после MaxRetries не удалось сгенерировать уникальный код (крайне маловероятно при хорошем хешировании),
            // генерируем исключение или обрабатываем ошибку соответствующим образом.
            throw new InvalidOperationException("Не удалось сгенерировать уникальный короткий код после нескольких попыток.");
        }
    }
    public static class Base62Converter
    {
        public const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private static readonly int Base = Alphabet.Length;

        /// <summary>
        /// Encodes a long integer into a Base62 string.
        /// </summary>
        /// <param name="number">The long integer to encode.</param>
        /// <returns>The Base62 encoded string.</returns>
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

        /// <summary>
        /// Decodes a Base62 string back into a long integer.
        /// </summary>
        /// <param name="base62String">The Base62 string to decode.</param>
        /// <returns>The decoded long integer.</returns>
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