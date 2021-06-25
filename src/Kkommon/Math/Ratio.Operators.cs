using System.Diagnostics.Contracts;

namespace Kkommon.Math
{
    // documentation omitted for obvious implementations
    public readonly partial struct Ratio
    {
        // equality
        [Pure]
        public static bool operator ==(Ratio left, Ratio right) => left.Equals(right);

        [Pure]
        public static bool operator !=(Ratio left, Ratio right) => !left.Equals(right);

        // ratio arithmetic
        [Pure]
        public static Ratio operator +(Ratio @this) => new(
            +@this.Numerator,
            +@this.Denominator
        );

        [Pure]
        public static Ratio operator -(Ratio @this) => new(
            -@this.Numerator,
            @this.Denominator
        );

        [Pure]
        public static Ratio operator +(Ratio left, Ratio right) => new(
            left.Denominator * right.Numerator + left.Numerator * right.Denominator,
            left.Denominator * right.Denominator
        );

        [Pure]
        public static Ratio operator -(Ratio left, Ratio right) => new(
            left.Numerator * right.Denominator - left.Denominator * right.Numerator,
            left.Denominator * right.Denominator
        );

        [Pure]
        public static Ratio operator *(Ratio left, Ratio right) => new(
            left.Numerator * right.Numerator,
            left.Denominator * right.Denominator
        );

        [Pure]
        public static Ratio operator /(Ratio left, Ratio right) => new(
            left.Numerator * right.Denominator,
            left.Denominator * right.Numerator
        );

        // integer arithmetic
        [Pure]
        public static Ratio operator +(Ratio left, int right) => new(
            left.Numerator + left.Denominator * right,
            left.Denominator
        );

        [Pure]
        public static Ratio operator +(int left, Ratio right) => right + left;

        [Pure]
        public static Ratio operator -(Ratio left, int right) => new(
            left.Numerator - left.Denominator * right,
            left.Denominator
        );

        [Pure]
        public static Ratio operator -(int left, Ratio right) => new(
            right.Denominator * left - right.Numerator,
            right.Denominator
        );

        [Pure]
        public static Ratio operator *(Ratio left, int right) => new(
            left.Numerator * right,
            left.Denominator
        );

        [Pure]
        public static Ratio operator *(int left, Ratio right) => right * left;

        [Pure]
        public static Ratio operator /(Ratio left, int right) => new(
            left.Numerator,
            left.Denominator * right
        );

        [Pure]
        public static Ratio operator /(int left, Ratio right) => new(
            left * right.Denominator,
            right.Numerator
        );

        // conversions
        [Pure]
        public static implicit operator decimal(Ratio @this) => @this.Value;

        [Pure]
        public static implicit operator float(Ratio @this) => @this.FValue;

        [Pure]
        public static implicit operator double(Ratio @this) => @this.DValue;
    }
}