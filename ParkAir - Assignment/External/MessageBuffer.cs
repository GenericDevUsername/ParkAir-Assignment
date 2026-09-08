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
      this._maxCount = maxCount;
      this._queue = new(maxCount);
    }

    public void Add(T item)
    {
      if (this._queue.Count == this._maxCount) this._queue.Dequeue();
      this._queue.Enqueue(item);
    }

    public IEnumerator<T> GetEnumerator()
    {
      return this._queue.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}
