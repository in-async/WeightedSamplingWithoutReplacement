using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace InAsync.Linq.OrderByWeight.Tests {

    [TestClass]
    public class EnumerableExtensionsTests {

        [TestMethod]
        public void OrderByRandom() {
            foreach (var item in TestCases()) {
                var message = $"No.{item.testNumber}";
                if (!AssertException.TryExecute(() => EnumerableExtensions.OrderByRandom(item.source, item.weightSelector, item.rand), item.expectedExceptionType, out var actual, message)) {
                    continue;
                }

                actual.Is(item.expected, message);
            }

            // テストケース定義。
            IEnumerable<(int testNumber, int[] source, Func<int, double> weightSelector, Func<double> rand, int[] expected, Type expectedExceptionType)> TestCases() => new(int testNumber, int[] source, Func<int, double> weightSelector, Func<double> rand, int[] expected, Type expectedExceptionType)[]{
                ( 0, null            , x => x, () => 0.0, null            , typeof(ArgumentNullException)),
                ( 1, new[]{0,1,2,3,4}, null  , () => 0.0, null            , typeof(ArgumentNullException)),
                (10, new[]{0,1,2,3,4}, x => x, () => 0.0, new[]{4,3,2,1,0}, null),
                (11, new[]{0,1,2,3,4}, x => x, () => 0.1, new[]{4,3,2,1,0}, null),
                (12, new[]{0,1,2,3,4}, x => x, () => 0.2, new[]{4,3,2,1,0}, null),
                (13, new[]{0,1,2,3,4}, x => x, () => 0.3, new[]{4,3,2,1,0}, null),
                (14, new[]{0,1,2,3,4}, x => x, () => 0.4, new[]{4,3,2,1,0}, null),
                (15, new[]{0,1,2,3,4}, x => x, () => 0.5, new[]{3,4,2,1,0}, null),
                (16, new[]{0,1,2,3,4}, x => x, () => 0.6, new[]{3,2,4,1,0}, null),
                (17, new[]{0,1,2,3,4}, x => x, () => 0.7, new[]{3,2,4,1,0}, null),
                (18, new[]{0,1,2,3,4}, x => x, () => 0.8, new[]{2,3,4,1,0}, null),
                (19, new[]{0,1,2,3,4}, x => x, () => 0.9, new[]{2,1,3,4,0}, null),
                (20, new[]{0,1,2,3,4}, x => x, () => 1.0, new[]{1,2,3,4,0}, null),
            };
        }

        [TestMethod]
        public void OrderByRandom_Statistics() {
            var source = new[] { 0, 1, 2, 3, 4 };
            var rnd = new Random();
            Func<double> rand = () => rnd.NextDouble();
            var trials = 10000;

            var stats = (
                from _ in Enumerable.Range(0, trials)
                select source.OrderByRandom(x => x, rand).First() into item
                group item by item into g
                orderby g.Count() descending
                select new {
                    g.Key,
                    Count = g.Count(),
                    Percentage = (double)g.Count() / trials,
                }
            ).ToArray();

            Trace.WriteLine(JsonConvert.SerializeObject(stats, Formatting.Indented));
            stats.Select(x => new {
                x.Key,
                Percentage = Math.Round(x.Percentage, 1),
            }).Is(new[] {
                new{Key = 4, Percentage = .4},
                new{Key = 3, Percentage = .3},
                new{Key = 2, Percentage = .2},
                new{Key = 1, Percentage = .1},
            });
        }
    }
}