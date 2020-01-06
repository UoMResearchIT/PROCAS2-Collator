using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Security.Cryptography;

namespace PROCAS2.Services.Utility
{
    public class HashingService:IHashingService
    {
        // The password methods are copied from here : http://crackstation.net/hashing-security.htm

        // The following constants may be changed without breaking existing hashes.
        private const int SALT_BYTES = 128;
        private const int HASH_BYTES = 128;
        private const int PBKDF2_ITERATIONS = 10000;

        private const int ITERATION_INDEX = 0;
        private const int SALT_INDEX = 1;
        private const int PBKDF2_INDEX = 2;

        private IConfigService _configService;

        public HashingService(IConfigService configService)
        {
            _configService = configService;
        }


        /// <summary>
        /// Creates a salted PBKDF2 hash of the password.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>The hash of the password.</returns>
        public string CreateHash(string password)
        {
            // Generate a random salt
            RNGCryptoServiceProvider csprng = new RNGCryptoServiceProvider();
            byte[] salt = new byte[SALT_BYTES];
            csprng.GetBytes(salt);

            // Hash the password and encode the parameters
            byte[] hash = PBKDF2(password, salt, PBKDF2_ITERATIONS, HASH_BYTES);
            return PBKDF2_ITERATIONS + ":" +
                Convert.ToBase64String(salt) + ":" +
                Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Creates a salted PBKDF2 hash of the NHS number using the secret salt
        /// </summary>
        /// <param name="NHSNumber">NHS number</param>
        /// <returns>The hash (not including the salt)</returns>
        public string CreateNHSHash(string NHSNumber)
        {
            int iterations = Convert.ToInt32(_configService.GetAppSetting("NHSHashingIterations"));
            string saltString = _configService.GetAppSetting("NHSHashingSalt");
            byte[] salt = Convert.FromBase64String(saltString);
            byte[] hash = PBKDF2(NHSNumber, salt, iterations, HASH_BYTES);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Creates a salted PBKDF2 hash of the Screening number using the secret salt
        /// </summary>
        /// <param name="screenNumber">Screening number</param>
        /// <returns>The hash (not including the salt)</returns>
        public string CreateScreenHash(string screeningNumber)
        {
            int iterations = Convert.ToInt32(_configService.GetAppSetting("NHSHashingIterations"));
            string saltString = _configService.GetAppSetting("NHSHashingSalt");
            byte[] salt = Convert.FromBase64String(saltString);
            byte[] hash = PBKDF2(screeningNumber, salt, iterations, HASH_BYTES);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Validates an NHS number given a hash of the correct one.
        /// </summary>
        /// <param name="NHSNumber">The NHS number to check</param>
        /// <param name="goodHash">A hash of the correct NHS number</param>
        /// <returns>True if NHS number correct, else false</returns>
        public bool ValidateNHSNumber(string NHSNumber, string goodHash)
        {
            if (String.IsNullOrEmpty(goodHash))
                return false;

            
            int iterations = Convert.ToInt32(_configService.GetAppSetting("NHSHashingIterations"));
            string saltString = _configService.GetAppSetting("NHSHashingSalt");
            byte[] salt = Convert.FromBase64String(saltString);
            byte[] hash = Convert.FromBase64String(goodHash);

            byte[] testHash = PBKDF2(NHSNumber, salt, iterations, hash.Length);
            return SlowEquals(hash, testHash);
        }

        /// <summary>
        /// Validates a password given a hash of the correct one.
        /// </summary>
        /// <param name="password">The password to check.</param>
        /// <param name="goodHash">A hash of the correct password.</param>
        /// <returns>True if the password is correct. False otherwise.</returns>
        public bool ValidatePassword(string password, string goodHash)
        {
            if (String.IsNullOrEmpty(goodHash))
                return false;

            // Extract the parameters from the hash
            char[] delimiter = { ':' };
            string[] split = goodHash.Split(delimiter);
            int iterations = Int32.Parse(split[ITERATION_INDEX]);
            byte[] salt = Convert.FromBase64String(split[SALT_INDEX]);
            byte[] hash = Convert.FromBase64String(split[PBKDF2_INDEX]);

            byte[] testHash = PBKDF2(password, salt, iterations, hash.Length);
            return SlowEquals(hash, testHash);
        }

        /// <summary>
        /// Compares two byte arrays in length-constant time. This comparison
        /// method is used so that password hashes cannot be extracted from
        /// on-line systems using a timing attack and then attacked off-line.
        /// </summary>
        /// <param name="a">The first byte array.</param>
        /// <param name="b">The second byte array.</param>
        /// <returns>True if both byte arrays are equal. False otherwise.</returns>
        private bool SlowEquals(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
                diff |= (uint)(a[i] ^ b[i]);
            return diff == 0;
        }

        /// <summary>
        /// Computes the PBKDF2-SHA1 hash of a password.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="iterations">The PBKDF2 iteration count.</param>
        /// <param name="outputBytes">The length of the hash to generate, in bytes.</param>
        /// <returns>A hash of the password.</returns>
        private byte[] PBKDF2(string password, byte[] salt, int iterations, int outputBytes)
        {
            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt);
            pbkdf2.IterationCount = iterations;
            return pbkdf2.GetBytes(outputBytes);
        }
    }
}
