using System;
using System.Collections.Generic;

namespace Framework.Core.Extensions
{
    public static class Extensions
    {
        public const string All_IMAGE_REGEX = "image/[a-z]*";
        public static T AgainstNegativeOrZero<T>(this T input, string parameterName) where T : struct, IComparable
        {
            if (input.CompareTo(default(T)) <= 0)
            {
                throw new ArgumentException($"Required input {parameterName} cannot be zero or negative.", parameterName);
            }

            return input;
        }

        public static T AgainstNegative<T>(this T input, string parameterName) where T : struct, IComparable
        {
            if (input.CompareTo(default(T)) < 0)
            {
                throw new ArgumentException($"Required input {parameterName} cannot be negative.", parameterName);
            }

            return input;
        }
        public static T AgainstNull<T>(this T input, string parameterName)
        {
            if (input is null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return input;
        }

        public static string AgainstNullOrEmpty(this string input, string parameterName)
        {
            input = input.AgainstNull(parameterName);
            if (input == string.Empty)
            {
                throw new ArgumentException($"Required input {parameterName} was empty.", parameterName);
            }

            return input!;
        }
        public static T AgainstDefault<T>(this T input, string parameterName)
        {
            if (EqualityComparer<T>.Default.Equals(input, default!) || input is null)
            {
                throw new ArgumentException($"Parameter [{parameterName}] is default value for type {typeof(T).Name}", parameterName);
            }

            return input;
        }
        public static T AgainstNullOrDefault<T>(this T input, string parameterName)
        {
            input = input.AgainstNull(parameterName);
            input = input.AgainstDefault(parameterName);

            return input!;
        }

    }
}
