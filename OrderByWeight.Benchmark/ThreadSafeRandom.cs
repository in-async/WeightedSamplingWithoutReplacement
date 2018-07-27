using System;
using System.Threading;

namespace InAsync.Linq.OrderByWeight.Benchmark {

    public static class ThreadSafeRandom {
        private static int _seed = Environment.TickCount;

        private readonly static ThreadLocal<Random> _randomWrapper = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));

        public static Random Value => _randomWrapper.Value;
    }
}