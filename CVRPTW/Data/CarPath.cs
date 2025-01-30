using System.Collections;

namespace CVRPTW;

public class CarPath : IList<int>, IEnumerable<int>
{
    public int StartPointId { get; set; }
    
    public int EndPointId { get; set; }
    
    public List<int> PathPointsIds { get; set; } = new();

    public int Count => PathPointsIds.Count + 2;
    
    public bool IsReadOnly => false;

    public int this[int index]
    {
        get
        {
            if (index == 0) return StartPointId;
            
            if (index == Count - 1) return EndPointId;
            
            return PathPointsIds[index - 1];
        }
        set
        {
            if (index == 0) StartPointId = value; 
            else if (index == Count - 1) EndPointId = value;
            else PathPointsIds[index - 1] = value;
        }
    }

    public CarPath() { }

    public CarPath(params int[] path)
    {
        SetPath(path);
    }

    public void AddNextPoint(int pointId)
    {
        PathPointsIds.Add(pointId);
    }

    public void SetPath(params int[] totalPath)
    {
        if (totalPath == null || totalPath.Length < 2)
        {
            throw new ArgumentException("Path must contain at least a start and an end point.");
        }

        StartPointId = totalPath[0];
        EndPointId = totalPath[^1];
        PathPointsIds = totalPath.Skip(1).Take(totalPath.Length - 2).ToList();
    }

    public void Reverse(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || toIndex >= Count)
        {
            throw new ArgumentOutOfRangeException($"Invalid indices for reversing the path. {fromIndex} {toIndex}");
        }

        if (fromIndex > toIndex) (fromIndex, toIndex) = (toIndex, fromIndex);

        if (fromIndex == 0 && toIndex == Count - 1)
        {
            (StartPointId, EndPointId) = (EndPointId, StartPointId);
            PathPointsIds.Reverse();
            return;
        }

        if (fromIndex == 0)
        {
            (StartPointId, this[toIndex]) = (this[toIndex], StartPointId);

            PathPointsIds.Reverse(0, toIndex - 1);
        }
        else if (toIndex == Count - 1)
        {
            (this[fromIndex], EndPointId) = (EndPointId, this[fromIndex]);
            
            PathPointsIds.Reverse(fromIndex, toIndex - fromIndex - 1);
        }
        else
        {
            PathPointsIds.Reverse(fromIndex - 1, toIndex - fromIndex + 1);
        }
    }

    public CarPath Clone()
    {
        return new CarPath
        {
            StartPointId = this.StartPointId,
            EndPointId = this.EndPointId,
            PathPointsIds = [..PathPointsIds]
        };
    }

    public int IndexOf(int item)
    {
        if (item == StartPointId) return 0;
        if (item == EndPointId) return Count - 1;
        return PathPointsIds.IndexOf(item) + 1;
    }

    public void Insert(int index, int item)
    {
        if (index < 0 || index > Count)
            throw new ArgumentOutOfRangeException("Invalid index for insertion.");

        if (index == 0)
        {
            PathPointsIds.Insert(0, StartPointId);
            StartPointId = item;
            return;
        }

        if (index == Count)
        {
            PathPointsIds.Add(EndPointId);
            EndPointId = item;
            return;
        }
        
        PathPointsIds.Insert(index - 1, item);
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException("Invalid index for removal.");
        
        if (index == 0)
        {
            StartPointId = PathPointsIds[0];
            PathPointsIds.RemoveAt(0);
            return;
        }

        if (index == Count - 1)
        {
            EndPointId = PathPointsIds[^1];
            PathPointsIds.RemoveAt(PathPointsIds.Count - 1);
            return;
        }
        
        PathPointsIds.RemoveAt(index - 1);
    }

    public void Add(int item)
    {
        Insert(Count, item);
    }

    public void Clear()
    {
        StartPointId = 0;
        EndPointId = 0;
        PathPointsIds.Clear();
    }

    public bool Contains(int item)
    {
        return PathPointsIds.Contains(item) || StartPointId == item || EndPointId == item;
    }

    public void CopyTo(int[] array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
    
        if (arrayIndex < 0 || arrayIndex >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Invalid array index.");
    
        if (array.Length - arrayIndex < Count)
            throw new ArgumentException("The number of elements in the source is greater than the available space from arrayIndex to the end of the destination array.");
    
        array[arrayIndex] = StartPointId;
        PathPointsIds.CopyTo(array, arrayIndex + 1);
        array[arrayIndex + Count - 1] = EndPointId;
    }

    public bool Remove(int item)
    {
        var index = IndexOf(item);
        
        if (index == -1) return false;
        
        RemoveAt(index);
        
        return true;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<int> GetEnumerator()
    {
        return new CarPathEnumerator(this);
    }

    public static implicit operator int[](CarPath carPath)
    {
        var result = new int[carPath.Count];
        for (int i = 0; i < carPath.Count; i++)
        {
            result[i] = carPath[i];
        }
        return result;
    }

    public override string ToString()
    {
        return $"[{StartPointId} [{string.Join(", ", PathPointsIds)}] {EndPointId}]";
    }
}