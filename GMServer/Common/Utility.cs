using System;
using System.Security.Cryptography;
using System.Text;

namespace GMServer.Common
{
    public static class Utility
    {
        public static Random SeededRandom(string seed)
        {
            using var algo = SHA1.Create();
            var hash = BitConverter.ToInt32(algo.ComputeHash(Encoding.UTF8.GetBytes(seed)));
            return new Random(hash);
        }
    }
}
