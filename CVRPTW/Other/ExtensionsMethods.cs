using System.Collections;

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
    
    public static T2 RandomValue<T2>(this Dictionary<int,T2> dictionary)
    {
        return dictionary[System.Random.Shared.Next(dictionary.Count)];
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

    public static void Invert<T>(this IList[] array, int fromIndex, int toIndex)
    {
        ArgumentNullException.ThrowIfNull(array);

        if (fromIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(fromIndex), "fromIndex is less than 0.");

        if (toIndex >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(toIndex), "toIndex is greater than or equal to the array length.");

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

        if (toIndex >= path.Length)
            throw new ArgumentOutOfRangeException(nameof(toIndex), "toIndex is greater than or equal to the array length.");

        if (fromIndex > toIndex)
            throw new ArgumentException("fromIndex is greater than toIndex.");

        while (fromIndex < toIndex)
        {
            (path[fromIndex], path[toIndex]) = (path[toIndex], path[fromIndex]);
            fromIndex++;
            toIndex--;
        }
    }
}