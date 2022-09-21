using System;
using System.Collections.Generic;
using System.Linq;

namespace Wordle
{
    public static class Utils
    {
        public static IEnumerable<T> Repeat<T>(this IEnumerable<T> source, int count)
        {
            for (int i = 0; i < count; i++)
                foreach (var s in source)
                    yield return s;
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.Shuffle(new Random());
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random random)
        {
            //var random = System.Security.Cryptography.RandomNumberGenerator.Create();
            var buffer = source.ToList();
            for (int i = 0; i < buffer.Count; i++)
            {
                int j = random.Next(i, buffer.Count);
                yield return buffer[j];
                buffer[j] = buffer[i];
            }
        }

        public static Random Random { get; } = new Random();
    }
}
