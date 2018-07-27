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
            var trials = 30000;

            var results = (
                from _ in Enumerable.Range(0, trials)
                select source.OrderByRandom(x => x, rand).ToArray()
            ).ToArray();

            for (var i = 0; i < source.Length; i++) {
                $"ElementAt({i}) の出現率：".Dump();
                (
                    from collection in results
                    group collection[i] by collection[i] into g
                    orderby g.Count() descending
                    select new {
                        Item = g.Key,
                        Rate = (double)g.Count() / results.Length,
                    }
                ).ToJson().Dump();
            }

            //"並び順の出現率：".Dump();
            //(
            //    from colleciton in results
            //    select string.Join(",", colleciton) into line
            //    group line by line into g
            //    orderby g.Count() descending
            //    select new {
            //        g.Key,
            //        Percent = ((double)g.Count() / results.Length).ToString("p1"),
            //    }
            //).ToJson().Dump();

            {
                var stats = (
                    from colleciton in results
                    select colleciton.First() into item
                    group item by item into g
                    orderby g.Count() descending
                    select new {
                        g.Key,
                        Rate = (double)g.Count() / results.Length,
                    }
                );
                stats.Select(x => new {
                    x.Key,
                    Rate = Math.Round(x.Rate, 1),
                }).Is(new[] {
                    new{Key = 4, Rate = .4},
                    new{Key = 3, Rate = .3},
                    new{Key = 2, Rate = .2},
                    new{Key = 1, Rate = .1},
                });
            }
            {
                var subResults = results.Where(x => x[0] == 4).ToArray();
                var stats = (
                    from collection in subResults
                    select collection[1] into item
                    group item by item into g
                    orderby g.Count() descending
                    select new {
                        g.Key,
                        Rate = (double)g.Count() / subResults.Length,
                    }
                );
                "ElementAt(0) = 4 の時の出現率：".Dump();
                $"試行数 = {subResults.Length}".Dump();
                stats.ToJson().Dump();
                stats.Select(x => new {
                    x.Key,
                    Rate = Math.Round(x.Rate, 1),
                }).Is(new[]{
                    new{Key = 3, Rate = .5},
                    new{Key = 2, Rate = .3},
                    new{Key = 1, Rate = .2},
                });
            }
            {
                var subResults = results.Where(x => x[0] == 3).ToArray();
                var stats = (
                    from collection in subResults
                    select collection[1] into item
                    group item by item into g
                    orderby g.Count() descending
                    select new {
                        g.Key,
                        Rate = (double)g.Count() / subResults.Length,
                    }
                );
                "ElementAt(0) = 3 の時の出現率：".Dump();
                $"試行数 = {subResults.Length}".Dump();
                stats.ToJson().Dump();
                stats.Select(x => new {
                    x.Key,
                    Rate = Math.Round(x.Rate, 1),
                }).Is(new[]{
                    new{Key = 4, Rate = .6},
                    new{Key = 2, Rate = .3},
                    new{Key = 1, Rate = .1},
                });
            }
        }
    }
}