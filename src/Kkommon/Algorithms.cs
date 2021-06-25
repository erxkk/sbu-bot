using System;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

namespace Kkommon
{
    /// <summary>
    ///     A static class containing  algorithms.
    /// </summary>
    public static class Algorithms
    {
        /// <summary>
        ///     A static class containing common mathematical algorithms.
        /// </summary>
        public static class Math
        {
            /// <summary>
            ///     Computes the greatest common denominator out of two 64-bit integers.
            /// </summary>
            /// <param name="a">The first given number.</param>
            /// <param name="b">The second given number.</param>
            /// <returns>
            ///     The greatest common denominator of <paramref name="a" /> and <paramref name="b"></paramref>.
            /// </returns>
            /// <exception cref="OverflowException">The operation resulted in an overflow.</exception>
            [Pure]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static long Gcd(long a, long b)
            {
                // using Euclid's original algorithm instead of the improved euclidean algorithm as it avoids division
                while (a != b)
                {
                    if (a > b)
                    {
                        checked
                        {
                            a -= b;
                        }
                    }
                    else
                    {
                        checked
                        {
                            b -= a;
                        }
                    }
                }

                return a;
            }

            /// <summary>
            ///     Computes the least common multiple of two 64-bit integers.
            /// </summary>
            /// <param name="a">The first given number.</param>
            /// <param name="b">The second given number.</param>
            /// <returns>
            ///     The least common multiple of <paramref name="a" /> and <paramref name="b" />.
            /// </returns>
            /// <exception cref="OverflowException">The operation resulted in an overflow.</exception>
            [Pure]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static long Lcm(long a, long b)
            {
                long initialA = a;
                long initialB = b;

                while (a != b)
                {
                    if (a > b)
                    {
                        checked
                        {
                            b += initialB;
                        }
                    }
                    else
                    {
                        checked
                        {
                            a += initialA;
                        }
                    }
                }

                return a;
            }
        }
    }
}