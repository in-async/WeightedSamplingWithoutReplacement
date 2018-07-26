using System;
using System.Collections.Generic;
using System.Linq;

namespace InAsync.Linq {

    public static class EnumerableExtensions {

        /// <summary>
        /// コレクションを重み付き確率的に並べ替えます（非復元抽出）。
        /// </summary>
        /// <typeparam name="T"><paramref name="source"/> の要素。</typeparam>
        /// <param name="source">並べ替える対象のコレクション。</param>
        /// <param name="weightSelector"><typeparamref name="T"/> の重みを選択するデリゲート。</param>
        /// <param name="rand">0.0 から 1.0 の乱数を生成するデリゲート。</param>
        /// <returns>並べ替えられた <typeparamref name="T"/> のコレクション。</returns>
        public static IEnumerable<T> OrderByRandom<T>(this IEnumerable<T> source, Func<T, double> weightSelector, Func<double> rand = null) {
            if (source == null) { throw new ArgumentNullException(nameof(source)); }
            if (weightSelector == null) { throw new ArgumentNullException(nameof(weightSelector)); }
            if (rand == null) {
                var rnd = new Random();
                rand = () => rnd.NextDouble();
            }

            var orderedWeighted = source
                .Select(item => new ItemWeight<T>(item, weightSelector(item)))
                .OrderByDescending(x => x.Weight)
                .ToArray();

            if (orderedWeighted.Any() == false) { return source; }
            if (orderedWeighted.Last().Weight < 0) { throw new InvalidOperationException(); }

            return InternalOrderByRandom();

            IEnumerable<T> InternalOrderByRandom() {
                var totalWeight = orderedWeighted.Sum(x => x.Weight);
                var remainedWeighted = orderedWeighted.Length;

                while (remainedWeighted > 0) {
                    var thresholdWeight = rand() * totalWeight;

                    var accumulatedWeight = 0d;
                    foreach (var weighted in orderedWeighted) {
                        if (weighted.Weight < 0) continue;
                        accumulatedWeight += weighted.Weight;

                        if (accumulatedWeight >= thresholdWeight) {
                            yield return weighted.Item;

                            totalWeight -= weighted.Weight;
                            remainedWeighted--;
                            weighted.ResetWeight();
                            break;
                        }
                    }
                }
            }
        }

        private class ItemWeight<T> {

            public ItemWeight(T item, double weight) {
                Item = item;
                Weight = weight;
            }

            public T Item { get; }

            public double Weight { get; private set; }

            public void ResetWeight() => Weight = -1;
        }
    }
}