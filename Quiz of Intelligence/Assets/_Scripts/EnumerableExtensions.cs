using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumerableExtensions
{
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> sequence)
    {
        if (sequence == null) throw new ArgumentNullException(nameof(sequence));

        var list = sequence.ToList();
        var seq = RandomSequence(0, list.Count());
        return seq.Select(list.ElementAt);
    }

    private static readonly Random RandomNumberGenerator = new Random();
    private static IEnumerable<int> RandomSequence(int minimum, int maximum)
    {
        var candidates = Enumerable.Range(minimum, maximum - minimum).ToList();
        while (candidates.Count > 0)
        {
            var index = RandomNumberGenerator.Next(candidates.Count);
            yield return candidates[index];
            candidates.RemoveAt(index);
        }
    }

}