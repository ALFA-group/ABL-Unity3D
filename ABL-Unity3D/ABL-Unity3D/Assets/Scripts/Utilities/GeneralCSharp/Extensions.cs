using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#nullable enable

namespace Utilities.GeneralCSharp
{
    public static class Extensions
    {
        public static string ToJson(this object jsonMe)
        {
            string? json = JsonConvert.SerializeObject(
                jsonMe,
                Formatting.Indented,
                new StringEnumConverter());
            return json;
        }

        public static T JsonTo<T>(this string json)
        {
            var result = JsonConvert.DeserializeObject<T>(
                json,
                new StringEnumConverter());

            return result;
        }

        public static float NextFloat(this Random random)
        {
            return (float)random.NextDouble();
        }

        public static float NextFloat(this Random random, float min, float max)
        {
            float zeroToOne = random.NextFloat();
            float result = min + zeroToOne * (max - min);
            return result;
        }
        
        public static IEnumerable<T> GetRandomElements<T>(this IEnumerable<T> list, int elementsCount, Random rand)
        {
            return list.OrderBy(_ => rand.Next()).Take(elementsCount);
        }

        public static T GetRandomEntry<T>(this T[] a, Random rand)
        {
            if (a.Length == 0) throw new Exception("Getting random entry from zero-length array");
            return a[rand.Next(0, a.Length)];
        }

        public static T GetRandomEntryOrValue<T>(this T[] a, Random rand, T t)
        {
            if (a.Length == 0) return t;
            return a[rand.Next(0, a.Length)];
        }


        public static T GetRandomEntry<T>(this List<T> list, Random rand)
        {
            return GetRandomEntry(list.ToArray(), rand);
        }

        public static T GetRandomEntry<T>(this IEnumerable<T> enumerable, Random rand)
        {
            return GetRandomEntry(enumerable.ToArray(), rand);
        }

        public static T GetRandomEntryOrValue<T>(this IEnumerable<T> enumerable, Random rand, T t)
        {
            return GetRandomEntryOrValue(enumerable.ToArray(), rand, t);
        }


        public static bool NextBool(this Random rand)
        {
            return rand.NextDouble() > 0.5;
        }


        //http://www.glennslayden.com/code/linq/handy-extension-methods
        public static int IndexOfMax<TSrc, TArg>(this IEnumerable<TSrc> ie, Converter<TSrc, TArg> fn)
            where TArg : IComparable<TArg>
        {
            using var e = ie.GetEnumerator();
            if (!e.MoveNext())
                return -1;

            var maxIx = 0;
            var t = e.Current;
            if (!e.MoveNext())
                return maxIx;

            var maxVal = fn(t);
            var i = 1;
            do
            {
                TArg tx;
                if ((tx = fn(e.Current)).CompareTo(maxVal) > 0)
                {
                    maxVal = tx;
                    maxIx = i;
                }

                i++;
            } while (e.MoveNext());

            return maxIx;
        }

        //http://www.glennslayden.com/code/linq/handy-extension-methods
        /// <summary>
        ///     Returns the index of the element which is smallest
        /// </summary>
        /// <typeparam name="TSrc"></typeparam>
        /// <typeparam name="TArg"></typeparam>
        /// <param name="ie"></param>
        /// <param name="fn"></param>
        /// <returns></returns>
        public static int IndexOfMin<TSrc, TArg>(this IEnumerable<TSrc> ie, Converter<TSrc, TArg> fn)
            where TArg : IComparable<TArg>
        {
            using var e = ie.GetEnumerator();
            if (!e.MoveNext())
                return -1;

            var minIx = 0;
            var t = e.Current;
            if (!e.MoveNext())
                return minIx;

            var minVal = fn(t);
            var i = 1;
            do
            {
                TArg tx;
                if ((tx = fn(e.Current)).CompareTo(minVal) < 0)
                {
                    minVal = tx;
                    minIx = i;
                }

                i++;
            } while (e.MoveNext());

            return minIx;
        }

        public static int IndexOf<TSrc>(this IEnumerable<TSrc> ie, TSrc matchMe)
            where TSrc : IEquatable<TSrc>
        {
            var index = 0;
            foreach (var guy in ie)
            {
                if (matchMe.Equals(guy)) return index;
                ++index;
            }

            return -1;
        }

        public static int IndexOfReference<TSrc>(this IEnumerable<TSrc> ie, TSrc matchMe)
            where TSrc : class
        {
            var index = 0;
            foreach (var guy in ie)
            {
                if (matchMe == guy) return index;
                ++index;
            }

            return -1;
        }


        //http://www.glennslayden.com/code/linq/handy-extension-methods
        public static TSrc ArgMax<TSrc, TArg>(this IEnumerable<TSrc> ie, Converter<TSrc, TArg> fn)
            where TArg : IComparable<TArg>
        {
            using var e = ie.GetEnumerator();
            if (!e.MoveNext())
                throw new InvalidOperationException("Sequence has no elements.");

            var t = e.Current;
            if (!e.MoveNext())
                return t;

            var maxVal = fn(t);
            do
            {
                TSrc tTry;
                TArg v;
                if ((v = fn(tTry = e.Current)).CompareTo(maxVal) > 0)
                {
                    t = tTry;
                    maxVal = v;
                }
            } while (e.MoveNext());

            return t;
        }


        //http://www.glennslayden.com/code/linq/handy-extension-methods
        public static TSrc? ArgMaxOrDefault<TSrc, TArg>(this IEnumerable<TSrc> ie, Converter<TSrc, TArg> fn)
            where TSrc : class
            where TArg : IComparable<TArg>
        {
            using var e = ie.GetEnumerator();
            if (!e.MoveNext())
                return default;

            var t = e.Current;
            if (!e.MoveNext())
                return t;

            var maxVal = fn(t);
            do
            {
                TSrc? tTry;
                TArg v;
                if ((v = fn(tTry = e.Current)).CompareTo(maxVal) > 0)
                {
                    t = tTry;
                    maxVal = v;
                }
            } while (e.MoveNext());

            return t;
        }

        //http://www.glennslayden.com/code/linq/handy-extension-methods
        public static TSrc ArgMin<TSrc, TArg>(this IEnumerable<TSrc> ie, Converter<TSrc, TArg> fn)
            where TArg : IComparable<TArg>
        {
            using var e = ie.GetEnumerator();
            if (!e.MoveNext())
                throw new InvalidOperationException("Sequence has no elements.");

            var t = e.Current;
            if (!e.MoveNext())
                return t;

            var minVal = fn(t);
            do
            {
                TSrc tTry;
                TArg v;
                if ((v = fn(tTry = e.Current)).CompareTo(minVal) < 0)
                {
                    t = tTry;
                    minVal = v;
                }
            } while (e.MoveNext());

            return t;
        }

        //http://www.glennslayden.com/code/linq/handy-extension-methods
        public static TSrc? ArgMinOrDefault<TSrc, TArg>(this IEnumerable<TSrc> ie, Converter<TSrc, TArg> fn)
            where TSrc : class
            where TArg : IComparable<TArg>
        {
            using var e = ie.GetEnumerator();
            if (!e.MoveNext())
                return default;

            var t = e.Current;
            if (!e.MoveNext())
                return t;

            var minVal = fn(t);
            do
            {
                TSrc? tTry;
                TArg v;
                if ((v = fn(tTry = e.Current)).CompareTo(minVal) < 0)
                {
                    t = tTry;
                    minVal = v;
                }
            } while (e.MoveNext());

            return t;
        }

        public static IEnumerable<TChildType> ConditionalCast<TParentType, TChildType>(
            this IEnumerable<TParentType> parents)
            where TChildType : class, TParentType
        {
            foreach (var parent in parents)
                if (parent is TChildType child)
                    yield return child;
        }

        public static T? MaybeGet<T>(this List<T?> list, int index) where T : class
        {
            return !list.ContainsIndex(index) ? default : list[index];
        }

        public static bool ContainsIndex<T>(this List<T> list, int index)
        {
            if (index < 0) return false;
            if (index >= list.Count) return false;
            return true;
        }

        public static List<List<T>> ToNestedList<T>(this IEnumerable<IEnumerable<T>> nestedEnumerable)
        {
            return nestedEnumerable.Select(e => e.ToList()).ToList();
        }

        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new[] { t });

            var array = list as T[] ?? list.ToArray();
            return GetPermutations(array, length - 1)
                .SelectMany(t => array.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new[] { t2 }));
        }
    }
}