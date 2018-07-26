using System;
using System.Collections.Generic;
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
        private double[] _items;

        [Params(1000, 10000)]
        public int N;

        [GlobalSetup]
        public void Setup() {
            _items = Enumerable.Range(0, N).Select(_ => _rnd.NextDouble()).ToArray();
        }

        [BenchmarkCategory("TakeAll"), Benchmark(Baseline = true)]
        public IEnumerable<double> OrderByDescending() => _items.OrderByDescending(x => x);

        [BenchmarkCategory("TakeAll"), Benchmark]
        public IEnumerable<double> OrderByWeight() => _items.OrderByRandom(x => x, () => _rnd.NextDouble());

        [BenchmarkCategory("Take10"), Benchmark(Baseline = true)]
        public IEnumerable<double> OrderByDescending_Take10() => _items.OrderByDescending(x => x).Take(10);

        [BenchmarkCategory("Take10"), Benchmark]
        public IEnumerable<double> OrderByWeight_Take10() => _items.OrderByRandom(x => x, () => _rnd.NextDouble()).Take(10);

        [BenchmarkCategory("Take1"), Benchmark(Baseline = true)]
        public double OrderByDescending_First() => _items.OrderByDescending(x => x).First();

        [BenchmarkCategory("Take1"), Benchmark]
        public double OrderByWeight_First() => _items.OrderByRandom(x => x, () => _rnd.NextDouble()).First();
    }
}