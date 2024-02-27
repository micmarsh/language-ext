﻿using LanguageExt.Traits;
using System.Diagnostics.Contracts;

namespace LanguageExt.ClassInstances;

/// <summary>
/// Floating point equality
/// </summary>
public struct EqFloat : Eq<float>
{
    /// <summary>
    /// Equality test
    /// </summary>
    /// <param name="x">The left hand side of the equality operation</param>
    /// <param name="y">The right hand side of the equality operation</param>
    /// <returns>True if x and y are equal</returns>
    [Pure]
    public static bool Equals(float a, float b) =>
        a.Equals(b);

    /// <summary>
    /// Get hash code of the value
    /// </summary>
    /// <param name="x">Value to get the hash code of</param>
    /// <returns>The hash code of x</returns>
    [Pure]
    public static int GetHashCode(float x) =>
        HashableFloat.GetHashCode(x);
}
