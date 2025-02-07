using System.Collections;
using CVRPTW.Computing.Estimators;

namespace CVRPTW;

public static class ExtensionsMethods
{
    public static bool IsDividerLine(this string line)
    {
        return line.Contains(Constants.TitleDividerSymbol);
    }

    public static T Random<T>(this List<T> list)
    {
        return list[System.Random.Shared.Next(list.Count)];
    }

    public static T2 RandomValue<T2>(this Dictionary<int, T2> dictionary)
    {
        return dictionary[System.Random.Shared.Next(dictionary.Count)];
    }

    public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> collection)
    {
        foreach (var item in collection)
            hashSet.Add(item);
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;

        while (n > 1)
        {
            n--;

            int k = System.Random.Shared.Next(n + 1);

            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    public static T SnatchFirst<T>(this List<T> list)
    {
        var first = list.First();

        list.RemoveAt(0);

        return first;
    }

    public static string ToString<T>(this IList<T> array)
    {
        var sb = Constants.SharedStringBuilder;

        foreach (var item in array)
            sb.AppendLine(item?.ToString());

        var result = Constants.SharedStringBuilder.ToString();

        Constants.SharedStringBuilder.Clear();

        return result;
    }

    public static void Invert<T>(this IList[] array, int fromIndex, int toIndex)
    {
        ArgumentNullException.ThrowIfNull(array);

        if (fromIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(fromIndex), "fromIndex is less than 0.");

        if (toIndex >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(toIndex),
                "toIndex is greater than or equal to the array length.");

        if (fromIndex > toIndex)
            throw new ArgumentException("fromIndex is greater than toIndex.");

        while (fromIndex < toIndex)
        {
            (array[fromIndex], array[toIndex]) = (array[toIndex], array[fromIndex]);
            fromIndex++;
            toIndex--;
        }
    }

    public static void Invert(this CarPath path, int fromIndex, int toIndex)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (fromIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(fromIndex), "fromIndex is less than 0.");

        if (toIndex >= path.Count)
            throw new ArgumentOutOfRangeException(nameof(toIndex),
                "toIndex is greater than or equal to the array length.");

        if (fromIndex > toIndex)
            throw new ArgumentException("fromIndex is greater than toIndex.");

        while (fromIndex < toIndex)
        {
            (path[fromIndex], path[toIndex]) = (path[toIndex], path[fromIndex]);
            fromIndex++;
            toIndex--;
        }
    }

    public static string ToFormattedString(this double value)
    {
        return value.ToString("#,0.00");
    }

    public static void SwapSegments<T>(this IList<T> list, int i, int j, int m, int n)
    {
        if (i < 0 || j >= list.Count || m < 0 || n >= list.Count)
            throw new ArgumentException($"Invalid indices: {i}, {j}, {m}, {n}");

        if (j >= m && i <= n)
            throw new ArgumentException("Segments are intersecting.");
        
        if (i > j) (i, j) = (j, i);
        
        if (m > n) (m, n) = (n, m);

        if (j > m) (i, j, m, n) = (m, n, i, j);

        var length1 = j - i + 1;
        var length2 = n - m + 1;

        for (int k = 0; k < Math.Min(length1, length2); k++)
        {
            (list[i + k], list[m + k]) = (list[m + k], list[i + k]);
        }

        if (length1 == length2) return;

        var difference = Math.Abs(length1 - length2);
        
        if (length1 < length2)
        {
            for (int k = 0; k < difference; k++)
            {
                var removeIndex = m + length1 + k;
                var insertIndex = i + length1 + k;
                
                var element = list[removeIndex];
                
                list.RemoveAt(removeIndex);
                
                list.Insert(insertIndex, element);
            }
        }
        else
        {
            for (int k = 0; k < difference; k++)
            {
                var removeIndex = i + length2;
                var insertIndex = m + length2 - 1;
                
                var element = list[removeIndex];
                
                list.RemoveAt(removeIndex);
                
                list.Insert(insertIndex, element);
            }
        }
    }

    public static void SetDefaults<T>(this IList<T?> list)
    {
        for (var i = 0; i < list.Count; i++)
        {
            list[i] = default;
        }
    }

    public static bool ArePermutations(CarPath firstPath, CarPath secondPath)
    {
        var firstPointsCounts = firstPath
            .Distinct()
            .ToDictionary(pointId => pointId, pointId => firstPath.Count(point => point == pointId));
        
        var secondsPointsCounts = secondPath
            .Distinct()
            .ToDictionary(pointId => pointId, pointId => secondPath.Count(point => point == pointId));
        
        return firstPointsCounts.All(pair => secondsPointsCounts.ContainsKey(pair.Key) && pair.Value == secondsPointsCounts[pair.Key]) &&
               secondsPointsCounts.All(pair => firstPointsCounts.ContainsKey(pair.Key) &&  pair.Value == firstPointsCounts[pair.Key]);
    }

    public static void ReEstimate(this CarResult result, PathEstimator estimator)
    {
        result.PathCost = result.Path.Count == 2 
            ? 0
            : estimator.Estimate(result.Path);
    }

    public static T TakeAt<T>(this IList<T> list, int index)
    {
        var result = list[index];
        
        list.RemoveAt(index);
        
        return result;
    }
    
    public static void Write<T>(this T[] arr)
    {
        foreach (var x in arr)
        {
            Console.Write(x + " ");
        }

        Console.WriteLine();
    }

    public static void Write(this CarPath path)
    {
        foreach (var point in path)
        {
            Console.Write(point + " ");
        }

        Console.WriteLine();
    }

    public static void Write<T>(this IList<T> list)
    {
        foreach (var x in list)
        {
            Console.Write(x + " ");
        }

        Console.WriteLine();
    }
}