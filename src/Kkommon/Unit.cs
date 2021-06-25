namespace Kkommon
{
    /// <summary>
    ///     A unit type that is an empty struct.
    /// </summary>
    /// <remarks>
    ///     This type is mainly useful when a union type like <see cref="Result{TSuccess, TError}"/> does not return
    ///     anything on one of it's valid states.
    /// </remarks>
    public readonly struct Unit { }
}