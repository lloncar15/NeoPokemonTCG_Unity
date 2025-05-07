using System;
using System.Security.Cryptography;
using System.Text;

namespace GimGim.Utility {
    public static class HashUtility {
        /// <summary>
        /// Generates a SHA1 hash for the given input string.
        /// </summary>
        /// <param name="input">The input string to hash.</param>
        /// <returns>The SHA1 hash as an integer.</returns>
        public static int GenerateSha1Hash(string input) {
            using (SHA1 sha1 = SHA1.Create()) {
                byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToInt32(hashBytes, 0);
            }
        }
    }
}