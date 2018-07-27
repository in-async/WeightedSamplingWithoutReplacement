using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace InAsync.Linq.OrderByWeight.Benchmark {

    internal class Program {

        private static void Main(string[] args) {
            BenchmarkRunner.Run<OrderByWeightBenchmark>();
        }
    }

    [Config(typeof(Config))]
    [RPlotExporter]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class OrderByWeightBenchmark {

        private sealed class Config : ManualConfig {

            public Config() {
                //Add(MarkdownExporter.GitHub);
                Add(MemoryDiagnoser.Default);
                Add(StatisticColumn.Min, StatisticColumn.Max);
                //Add(RankColumn.Arabic);
                Add(CategoriesColumn.Default);
                //Add(Job.Core);
                Add(Job.ShortRun);
            }
        }

        private readonly Random _rnd = new Random();
        private Func<double> _rand;
        private double[] _items;

        [Params(1000, 10000)]
        public int N;

        [GlobalSetup]
        public void Setup() {
            _rand = () => ThreadSafeRandom.Value.NextDouble();
            _items = Enumerable.Range(0, N).Select(_ => _rnd.NextDouble()).ToArray();
        }

        [BenchmarkCategory("TakeAll"), Benchmark(Baseline = true)]
        public void OrderByDescending() => _items.OrderByDescending(x => x).All(_ => true);

        [BenchmarkCategory("TakeAll"), Benchmark]
        public void OrderByWeight() => _items.OrderByRandom(x => x, _rand).All(_ => true);

        [BenchmarkCategory("Take10"), Benchmark(Baseline = true)]
        public void OrderByDescending_Take10() => _items.OrderByDescending(x => x).Take(10).All(_ => true);

        [BenchmarkCategory("Take10"), Benchmark]
        public void OrderByWeight_Take10() => _items.OrderByRandom(x => x, _rand).Take(10).All(_ => true);

        [BenchmarkCategory("Take1"), Benchmark(Baseline = true)]
        public void OrderByDescending_First() => _items.OrderByDescending(x => x).First();

        [BenchmarkCategory("Take1"), Benchmark]
        public void OrderByWeight_First() => _items.OrderByRandom(x => x, _rand).First();
    }
}