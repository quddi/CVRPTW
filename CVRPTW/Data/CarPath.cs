using System.Collections;

namespace CVRPTW;

public class CarPath : IList<PointVisitResult>
{
    public PointVisitResult StartPoint { get; set; }
    
    public PointVisitResult EndPoint { get; set; }
    
    public List<PointVisitResult> PathPoints { get; set; } = new();

    public int Count => PathPoints.Count + 2;
    
    public bool IsReadOnly => false;

    public double TravelTime => EndPoint.VisitTime;

    public PointVisitResult this[int index]
    {
        get
        {
            if (index == 0) return StartPoint;
            
            if (index == Count - 1) return EndPoint;
            
            return PathPoints[index - 1];
        }
        set
        {
            if (index == 0) StartPoint = value; 
            else if (index == Count - 1) EndPoint = value;
            else PathPoints[index - 1] = value;
        }
    }

    public CarPath() { }

    public CarPath(params PointVisitResult[] path)
    {
        SetPath(path);
    }

    public void AddNextPoint(PointVisitResult result)
    {
        PathPoints.Add(result);
    }

    public void SetPath(params PointVisitResult[] totalPath)
    {
        if (totalPath == null || totalPath.Length < 2)
        {
            throw new ArgumentException("Path must contain at least a start and an end point.");
        }

        StartPoint = totalPath[0];
        EndPoint = totalPath[^1];
        PathPoints = totalPath.Skip(1).Take(totalPath.Length - 2).ToList();
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
            (StartPoint, EndPoint) = (EndPoint, StartPoint);
            PathPoints.Reverse();
            return;
        }

        if (fromIndex == 0)
        {
            (StartPoint, this[toIndex]) = (this[toIndex], StartPoint);

            PathPoints.Reverse(0, toIndex - 1);
        }
        else if (toIndex == Count - 1)
        {
            (this[fromIndex], EndPoint) = (EndPoint, this[fromIndex]);
            
            PathPoints.Reverse(fromIndex, toIndex - fromIndex - 1);
        }
        else
        {
            PathPoints.Reverse(fromIndex - 1, toIndex - fromIndex + 1);
        }
    }

    public CarPath Clone()
    {
        return new CarPath
        {
            StartPoint = this.StartPoint,
            EndPoint = this.EndPoint,
            PathPoints = [..PathPoints]
        };
    }

    public int IndexOf(PointVisitResult item)
    {
        if (item == StartPoint) return 0;
        if (item == EndPoint) return Count - 1;
        return PathPoints.IndexOf(item) + 1;
    }

    public int IndexOf(int id)
    {
        if (id == StartPoint.Id) return 0;
        if (id == EndPoint.Id) return Count - 1;
        return PathPoints.FindIndex(point => point.Id == id) + 1;
    }

    public void Insert(int index, PointVisitResult item)
    {
        if (index < 0 || index > Count)
            throw new ArgumentOutOfRangeException("Invalid index for insertion.");

        if (index == 0)
        {
            PathPoints.Insert(0, StartPoint);
            StartPoint = item;
            return;
        }

        if (index == Count)
        {
            PathPoints.Add(EndPoint);
            EndPoint = item;
            return;
        }
        
        PathPoints.Insert(index - 1, item);
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException("Invalid index for removal.");
        
        if (index == 0)
        {
            StartPoint = PathPoints[0];
            PathPoints.RemoveAt(0);
            return;
        }

        if (index == Count - 1)
        {
            EndPoint = PathPoints[^1];
            PathPoints.RemoveAt(PathPoints.Count - 1);
            return;
        }
        
        PathPoints.RemoveAt(index - 1);
    }

    public void Add(PointVisitResult item)
    {
        Insert(Count, item);
    }

    public void Clear()
    {
        StartPoint = default;
        EndPoint = default;
        PathPoints.Clear();
    }

    public bool Contains(PointVisitResult item)
    {
        return PathPoints.Contains(item) || StartPoint == item || EndPoint == item;
    }
    
    public bool Contains(int pointId)
    {
        return PathPoints.Any(pointResult => pointResult.Id == pointId) || StartPoint.Id == pointId || EndPoint.Id == pointId;
    }

    public void CopyTo(PointVisitResult[] array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
    
        if (arrayIndex < 0 || arrayIndex >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Invalid array index.");
    
        if (array.Length - arrayIndex < Count)
            throw new ArgumentException("The number of elements in the source is greater than the available space from arrayIndex to the end of the destination array.");
    
        array[arrayIndex] = StartPoint;
        PathPoints.CopyTo(array, arrayIndex + 1);
        array[arrayIndex + Count - 1] = EndPoint;
    }

    public bool Remove(PointVisitResult item)
    {
        var index = IndexOf(item);
        
        if (index == -1) return false;
        
        RemoveAt(index);
        
        return true;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<PointVisitResult> GetEnumerator()
    {
        return new CarPathEnumerator(this);
    }

    public static implicit operator PointVisitResult[](CarPath carPath)
    {
        var result = new PointVisitResult[carPath.Count];
        for (int i = 0; i < carPath.Count; i++)
        {
            result[i] = carPath[i];
        }
        return result;
    }

    public override string ToString()
    {
        return $"[{StartPoint} [{string.Join(", ", PathPoints)}] {EndPoint}]";
    }
}