using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Nancy.Bootstrappers.Mef
{

    static class EnumerableExtensions
    {

        /// <summary>
        /// Prepends <paramref name="item"/> to the enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> self, T item)
        {
            Contract.Requires<ArgumentNullException>(self != null);
            Contract.Requires<ArgumentNullException>(item != null);

            yield return item;
            foreach (var i in self)
                yield return i;
        }

        /// <summary>
        /// Recurses into the given object, obtaining a single child using the given function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IEnumerable<T> Recurse<T>(this T self, Func<T, T> node)
        {
            Contract.Requires<ArgumentNullException>(self != null);
            Contract.Requires<ArgumentNullException>(node != null);

            for (var i = self; i != null; i = node(i))
                yield return i;
        }

        /// <summary>
        /// Recurses into the given object, obtaining children using the given function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public static IEnumerable<T> Recurse<T>(this T self, Func<T, IEnumerable<T>> nodes)
        {
            Contract.Requires<ArgumentNullException>(self != null);
            Contract.Requires<ArgumentNullException>(nodes != null);

            yield return self;

            foreach (var i in nodes(self) ?? Enumerable.Empty<T>())
                if (i != null)
                    yield return i;
        }

        /// <summary>
        /// Recurses into each of the given objects, obtaining children using the given function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public static IEnumerable<T> Recurse<T>(this IEnumerable<T> self, Func<T, IEnumerable<T>> nodes)
        {
            Contract.Requires<ArgumentNullException>(self != null);
            Contract.Requires<ArgumentNullException>(nodes != null);

            foreach (var i in self)
                foreach (var j in i.Recurse(nodes))
                    yield return j;
        }

        /// <summary>
        /// Calls ToList on the enumerable if in DEBUG mode.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<T> ToDebugList<T>(this IEnumerable<T> self)
        {
#if DEBUG
            return self.ToList();
#else
            return self;
#endif
        }

    }

}
