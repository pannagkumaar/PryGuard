using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace PryGuard.Resources.Helpers
{
    /// <summary>
    /// Provides utilities for generating random numbers and selecting random values from collections.
    /// </summary>
    public static class RandomHelper
    {
        private static readonly RNGCryptoServiceProvider _randomGenerator = new RNGCryptoServiceProvider();

        /// <summary>
        /// Generates a random integer between the specified minimum and maximum values (inclusive).
        /// </summary>
        /// <param name="minimumValue">The minimum value of the random number.</param>
        /// <param name="maximumValue">The maximum value of the random number.</param>
        /// <returns>A random integer between the specified bounds.</returns>
        public static int GenerateRandomNumber(int minimumValue, int maximumValue)
        {
            if (minimumValue > maximumValue)
                throw new ArgumentOutOfRangeException(nameof(minimumValue), "Minimum value must be less than or equal to maximum value.");

            byte[] randomBytes = new byte[4];
            _randomGenerator.GetBytes(randomBytes);
            uint randomUnsignedInt = BitConverter.ToUInt32(randomBytes, 0);
            double normalizedValue = randomUnsignedInt / (double)uint.MaxValue;

            return (int)(minimumValue + normalizedValue * (maximumValue - minimumValue + 1));
        }

        /// <summary>
        /// Selects a random value from a list.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to select a random value from.</param>
        /// <returns>A random value from the list.</returns>
        public static T GetRandomValue<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
                throw new ArgumentException("List cannot be null or empty.", nameof(list));

            return list[GenerateRandomNumber(0, list.Count - 1)];
        }

        /// <summary>
        /// Selects a random value from a dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary to select a random value from.</param>
        /// <returns>A random value from the dictionary.</returns>
        public static TValue GetRandomValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null || dictionary.Count == 0)
                throw new ArgumentException("Dictionary cannot be null or empty.", nameof(dictionary));

            var randomKey = dictionary.Keys.ToList()[GenerateRandomNumber(0, dictionary.Count - 1)];
            return dictionary[randomKey];
        }
    }
}
