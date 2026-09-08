// code from:
// https://stackoverflow.com/a/6392606
//
// This is not my work. It is not a library, so I created a file for it.


using System.Collections;

namespace ParkAir___Assignment.Syslog
{
  public class SlidingBuffer<T> : IEnumerable<T>
  {
    private readonly Queue<T> _queue;
    private readonly int _maxCount;

    public SlidingBuffer(int maxCount)
    {
      _maxCount = maxCount;
      _queue = new Queue<T>(maxCount);
    }

    public void Add(T item)
    {
      if (_queue.Count == _maxCount)
        _queue.Dequeue();
      _queue.Enqueue(item);
    }

    public IEnumerator<T> GetEnumerator()
    {
      return _queue.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}
