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
        }
    }
}
