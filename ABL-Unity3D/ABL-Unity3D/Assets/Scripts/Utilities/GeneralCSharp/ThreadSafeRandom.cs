using System;

namespace Utilities.GeneralCSharp
{
    // https://devblogs.microsoft.com/pfxteam/getting-random-numbers-in-a-thread-safe-way/
    public static class ThreadSafeRandom
    {
        private static readonly Random _seeder = new Random();

        [ThreadStatic] private static Random _perThread;

        public static long Next()
        {
            var local = _perThread;
            if (null == local)
            {
                int seed;
                lock (_seeder)
                {
                    seed = _seeder.Next();
                }

                _perThread = local = new Random(seed);
            }

            long high = local.Next();
            high <<= 32;
            long low = local.Next();

            long combined = high | low;
            return combined;
        }
    }
}