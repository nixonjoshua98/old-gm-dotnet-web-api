using System;
using System.Security.Cryptography;
using System.Text;

namespace SRC.Common
{
    public static class Utility
    {
        static readonly DateTime GameDayEpoch = new(2021, 3, 29, 22, 0, 0);

        public static int GetGameDayNumber() => GetGameDayNumber(DateTime.UtcNow);
        public static int GetGameDayNumber(DateTime dt) => (int)(dt - GameDayEpoch).TotalDays;


        public static Random SeededRandom(string seed)
        {
            using var algo = SHA1.Create();
            var hash = BitConverter.ToInt32(algo.ComputeHash(Encoding.UTF8.GetBytes(seed)));
            return new Random(hash);
        }
    }
}
