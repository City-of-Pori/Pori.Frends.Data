using System;
using System.Collections.Generic;
using System.Linq;

namespace Pori.Frends.Data
{
    namespace Linq
    {
        internal static class Extensions
        {
            /// <summary>
            /// Reorder values in an enumerable to match the order in a given enumerable. 
            /// The relative order of values not in the second enumerable does not change.
            /// </summary>
            /// <typeparam name="TValue"></typeparam>
            /// <param name="values">The values to reorder.</param>
            /// <param name="order">The order in which the values should appear in the result</param>
            /// <returns></returns>
            public static IEnumerable<TValue> Reorder<TValue>(this IEnumerable<TValue> values, IEnumerable<TValue> order)
            {
                // The specified column order as a queue (consumed later)
                var reordered = new Queue<TValue>(order);

                // Reoder values. Preserve as much of the original order as possible.
                // Go through current values in order.
                foreach(TValue value in values)
                {
                    // If the value is specified in the new order,
                    // take the next value from the new order
                    // (not necessarily the same value as the current one)
                    if(order.Contains(value))
                        yield return reordered.Dequeue();
                    // If the value isn't specified in the new order, return it
                    else
                        yield return value;
                }
            }

            /// <summary>
            /// Produce the values of the source enumerable but catch
            /// exceptions encountered during the iteration. For each
            /// exception caught, call the catchAction with the index
            /// of the item and the exception thrown.
            /// </summary>
            /// <typeparam name="TSource">The value type of the enumerable.</typeparam>
            /// <param name="source">The source iterable to wrap.</param>
            /// <param name="catchAction"></param>
            /// <returns></returns>
            public static IEnumerable<TSource> Catch<TSource>(this IEnumerable<TSource> source,
                                                              Func<int, Exception, bool> catchAction)
            {
                return source.Catch(catchAction, () => default);
            }

            /// <summary>
            /// Produce the values of the source enumerable but catch
            /// exceptions encountered during the iteration. For each
            /// exception caught, call the catchAction with the index
            /// of the item and the exception thrown.
            /// </summary>
            /// <typeparam name="TSource">The value type of the enumerable.</typeparam>
            /// <param name="source">The source iterable to wrap.</param>
            /// <param name="catchAction"></param>
            /// <param name="defaultValueSelector">
            /// A function used to produce a value for rows that throw and exception
            /// </param>
            /// <returns></returns>
            public static IEnumerable<TSource> Catch<TSource>(this IEnumerable<TSource> source,
                                                              Func<int, Exception, bool> catchAction,
                                                              Func<TSource> defaultValueSelector)
            {
                var enumerator = source.GetEnumerator();
                TSource value;

                for(int i = 0; true; i++)
                {
                    try
                    {
                        if(!enumerator.MoveNext())
                            break;

                        value = enumerator.Current;
                    }
                    catch(Exception e)
                    {
                        value = defaultValueSelector();

                        if(!catchAction(i, e))
                            continue;
                    }

                    yield return value;
                }

            }
        }
    }
}
