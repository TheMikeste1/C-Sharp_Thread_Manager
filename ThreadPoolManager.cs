using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


class ThreadPoolManager
{
    private List<Thread> _threadPool;
    private List<bool> _freeThreads;

    private Semaphore _workSemaphore;
    private Semaphore _openThreadSemaphore;

    private int _maxThreads;

    public int MaxThreads
    {
        get => _maxThreads;

        set
        {
            if (value > _maxThreads)
            {
                _threadPool.Capacity = value;
                _freeThreads.Capacity = value;
            }
            else if (value < _maxThreads)
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value), "At least one thread must be enabled!");

                int numThreadsToRemove = _maxThreads - value;
                _freeThreads.RemoveRange(value, numThreadsToRemove);

                new Thread(() =>
                {
                    int i = 0;
                    do
                    {
                        if (_threadPool[i].Join(100))
                        {
                            _threadPool.RemoveAt(i);
                            numThreadsToRemove--;
                        }
                        else
                        {
                            i = (i + 1) % _threadPool.Count;
                        }
                    } while (numThreadsToRemove > 0);
                }).Start();
            }

            _maxThreads = value;
        }
    }

    public int NumActiveThreads
    {
        get
        {
            return _threadPool.Count;
        }
    }

    public ThreadPoolManager() : this(10) {}

    public ThreadPoolManager(int maxThreads)
    {
        if (maxThreads <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxThreads), "At least one thread must be enabled!");
        
        _maxThreads = maxThreads;
        
        _workSemaphore = new Semaphore(0, Int32.MaxValue, "Work Semaphore");
        _openThreadSemaphore = new Semaphore(_maxThreads, Int32.MaxValue, "Open Thread Semaphore");

        _threadPool = new List<Thread>(maxThreads);
        _freeThreads = new List<bool>(Enumerable.Repeat(true, maxThreads));
    }

    ~ThreadPoolManager()
    {
        try
        {
            foreach (var thread in _threadPool)
            {
                thread.Abort();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
