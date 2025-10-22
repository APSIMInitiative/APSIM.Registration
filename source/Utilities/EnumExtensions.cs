using System;

/// <summary>
/// Extension methods for working with enums. 
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Parse a string to an enum, ignoring case and hyphens.
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static TEnum ParseCustomEnum<TEnum>(string value) where TEnum : struct
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(nameof(value));
        }

        // Remove hyphens for matching
        var cleanValue = value.Replace("-", "");

        if (Enum.TryParse(cleanValue, true, out TEnum result))
        {
            return result;
        }

        throw new InvalidOperationException($"Cannot convert string value '{value}' to enum '{typeof(TEnum).Name}'.");
    }
}