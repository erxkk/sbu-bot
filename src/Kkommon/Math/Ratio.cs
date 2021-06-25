using System;
using System.Collections.Generic;
using System.Diagnostics;

using JetBrains.Annotations;

namespace Kkommon.Math
{
    /// <summary>
    ///     A 64-bit struct that encapsulates a ratio of two 32-bit integers.
    /// </summary>
    /// <remarks>
    ///     All arithmetic operations can be see as operations on (Numerator / Denominator), returning a new ratio.
    /// </remarks>
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(Ratio.DebuggerDisplay) + ",nq}")]
    public readonly partial struct Ratio : IEquatable<Ratio>, IComparable<Ratio>
    {
        /// <summary>
        ///     The numerator of the <see cref="Ratio" />.
        /// </summary>
        public int Numerator { get; }

        /// <summary>
        ///     The denominator of the <see cref="Ratio" />.
        /// </summary>
        [ValueRange(1, int.MaxValue)]
        public int Denominator { get; }

        /// <summary>
        ///     The <see langword="decimal" /> value of the <see cref="Ratio" />.
        /// </summary>
        public decimal Value => (decimal) Numerator / Denominator;

        /// <summary>
        ///     The <see langword="float" /> value of the <see cref="Ratio" />.
        /// </summary>
        public float FValue => (float) Numerator / Denominator;

        /// <summary>
        ///     The <see langword="double" /> value of the <see cref="Ratio" />.
        /// </summary>
        public double DValue => (double) Numerator / Denominator;

        private string DebuggerDisplay => ToString();

        /// <summary>
        ///     Creates a new <see cref="Ratio" /> with the given <paramref name="numerator" /> and
        ///     <paramref name="denominator" />.
        /// </summary>
        /// <param name="numerator">The numerator of this <see cref="Ratio" />.</param>
        /// <param name="denominator">The denominator of this <see cref="Ratio" />.</param>
        /// <exception cref="ArgumentOutOfRangeException">The denominator is 0.</exception>
        public Ratio(int numerator, int denominator)
        {
            if (denominator == 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(denominator),
                    denominator,
                    "The denominator must not be zero."
                );
            }

            // normalize all ratios to have sign at numerator if at all
            // this allows for easier equality
            Numerator = numerator * System.Math.Sign(denominator);
            Denominator = System.Math.Abs(denominator);
        }

        /// <summary>
        ///     Returns a simplified <see cref="Ratio" />, that is, it eliminates the greatest common denominator.
        /// </summary>
        /// <example>20/40 => 1/2</example>
        /// <returns>
        ///     A new simplified <see cref="Ratio" />.
        /// </returns>
        [Pure]
        public Ratio Simplify() => Ratio.GetSimplifiedRatio(Numerator, Denominator);

        /// <summary>
        ///     Returns the reciprocal of this <see cref="Ratio" />, that is, a <see cref="Ratio" /> with the numerator
        ///     and denominator flipped.
        /// </summary>
        /// <remarks>
        ///     If this <see cref="Ratio" /> was negative then the sign will be kept on the numerator after switching.
        /// </remarks>
        /// <example>20/40 => 40/20</example>
        /// <returns>
        ///     The a new reciprocal <see cref="Ratio" />.
        /// </returns>
        [Pure]
        public Ratio Reciprocal() => new(Denominator, Numerator);

        /// <summary>
        ///     Returns a simplified reciprocal <see cref="Ratio" />, see <see cref="Simplify" /> and
        ///     <see cref="Reciprocal" /> for more information.
        /// </summary>
        /// <example>20/40 => 2/1</example>
        /// <returns>
        ///     A new simplified reciprocal <see cref="Ratio" />.
        /// </returns>
        [Pure]
        public Ratio SimplifiedReciprocal() => Ratio.GetSimplifiedRatio(Denominator, Numerator);

        /// <summary>
        ///     Deconstructs this ratio into a tuple.
        /// </summary>
        /// <param name="numerator">The numerator of this ratio.</param>
        /// <param name="denominator">The denominator of this ratio.</param>
        public void Deconstruct(out int numerator, out int denominator)
        {
            numerator = Numerator;
            denominator = Denominator;
        }

        /// <inheritdoc />
        public int CompareTo(Ratio other) => (this - other).Numerator;

        /// <inheritdoc />
        [Pure]
        public bool Equals(Ratio other) => Numerator == other.Numerator && Denominator == other.Denominator;

        /// <inheritdoc />
        [Pure]
        public override bool Equals(object? obj) => obj is Ratio ratio && Equals(ratio);

        /// <inheritdoc />
        [Pure]
        public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);

        /// <inheritdoc />
        [Pure]
        public override string ToString() => $"{Numerator}/{Denominator}";

        /// <summary>
        ///     Returns a string object representing this <see cref="Ratio" />.
        /// </summary>
        /// <param name="format">The format to pass to the numerator and denominator.</param>
        /// <returns>
        ///     The string representation of this <see cref="Ratio" />.
        /// </returns>
        [Pure]
        public string ToString(string format) => $"{Numerator.ToString(format)}/{Denominator.ToString(format)}";

        /// <summary>
        ///     Creates a simplified <see cref="Ratio" /> from the given <paramref name="numerator" /> and
        ///     <paramref name="denominator" />.
        /// </summary>
        /// <remarks>
        ///     The simplified <see cref="Ratio" /> is found by using the <see cref="MathAlgorithms.Gcd"/> function for
        ///     elimination.
        /// </remarks>
        /// <param name="numerator">The given numerator.</param>
        /// <param name="denominator">The given denominator.</param>
        /// <returns>
        ///     Returns a new simplified <see cref="Ratio" />.
        /// </returns>
        /// <exception cref="OverflowException">The internal gcd-computation resulted in an overflow.</exception>
        [Pure]
        public static Ratio GetSimplifiedRatio(int numerator, int denominator)
        {
            var gcd = (int) Algorithms.Math.Gcd(numerator, denominator);

            return new(numerator / gcd, denominator / gcd);
        }

        /// <summary>
        ///     An <see cref="IEqualityComparer{T}" /> implementation for <see cref="Ratio" />s.
        /// </summary>
        [PublicAPI]
        public class Comparer : IEqualityComparer<Ratio>
        {
            /// <summary>
            ///     The singleton <see cref="Ratio.Comparer" /> instance.
            /// </summary>
            public static readonly Comparer INSTANCE = new();

            private Comparer() { }

            /// <inheritdoc />
            [Pure]
            public bool Equals(Ratio x, Ratio y) => x.Equals(y);

            /// <inheritdoc />
            [Pure]
            public int GetHashCode(Ratio obj) => obj.GetHashCode();
        }
    }
}