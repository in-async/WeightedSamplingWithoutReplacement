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
                if (!AssertException.TryExecute(() => EnumerableExtensions.OrderByRandom(item.source, item.weightSelector).ToArray(), item.expectedExceptionType, out var actual, message)) {
                    continue;
                }

                actual.OrderBy(_ => _).Is(item.source.OrderBy(_ => _), message);
            }

            // テストケース定義。
            IEnumerable<(int testNumber, int[] source, Func<int, double> weightSelector, Type expectedExceptionType)> TestCases() => new(int testNumber, int[] source, Func<int, double> weightSelector, Type expectedExceptionType)[]{
                ( 0, null             , x => x , typeof(ArgumentNullException)),
                ( 1, new[]{0,1,2,3,4} , null   , typeof(ArgumentNullException)),
                ( 2, new[]{0,1,2,3,4} , x => -1, typeof(InvalidOperationException)),
                ( 3, new[]{0,1,2,3,-1}, x => x , typeof(InvalidOperationException)),

                (10, new[]{0,1,2,3,4} , x => x , null),
                (11, new[]{0,1,2,3,4} , x => 0 , null),
                (12, new[]{0}         , x => x , null),
                (13, new[]{1}         , x => x , null),
                (14, new int[0]       , x => x , null),
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