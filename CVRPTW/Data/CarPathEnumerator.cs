using System.Collections;

namespace CVRPTW;

public class CarPathEnumerator(CarPath carPath) : IEnumerator<int>
{
    private int _position = -1;

    public int Current
    {
        get
        {
            if (_position < 0 || _position >= carPath.Count)
                throw new InvalidOperationException();
            return carPath[_position];
        }
    }

    object IEnumerator.Current => Current;

    public bool MoveNext()
    {
        _position++;
        return _position < carPath.Count;
    }

    public void Reset()
    {
        _position = -1;
    }

    public void Dispose() { }
}