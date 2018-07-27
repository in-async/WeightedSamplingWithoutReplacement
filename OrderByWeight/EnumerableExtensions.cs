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

            return InternalOrderByRandom();

            IEnumerable<T> InternalOrderByRandom() {
                var totalWeight = 0d;
                var weightedItems = source
                    .Select(item => {
                        var weight = weightSelector(item);
                        if (weight < 0) { throw new InvalidOperationException(); }
                        totalWeight += weight;

                        return new {
                            item,
                            weight,
                        };
                    })
                    .ToArray();

                var startIndex = 0;
                while (startIndex < weightedItems.Length) {
                    var target = rand() * totalWeight;
                    var cumulative = 0d;

                    for (var i = startIndex; i < weightedItems.Length; i++) {
                        var weightedItem = weightedItems[i];
                        cumulative += weightedItem.weight;

                        if (cumulative >= target) {
                            yield return weightedItem.item;
                            weightedItems[i] = weightedItems[startIndex++];
                            totalWeight -= weightedItem.weight;
                            break;
                        }
                    }
                }
            }
        }
    }
}