using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


class ThreadPoolManager
{
    private List<WorkerThread> _threadPool;

    private int _maxThreads;

    public int MaxThreads
    {
        get => _maxThreads;

        set
        {
            if (value > _maxThreads)
            {
                _threadPool.Capacity = value;
            }
            else if (value < _maxThreads)
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value), "At least one thread must be enabled!");

                int numThreadsToRemove = _maxThreads - value;

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
            int count = 0;

            foreach (var thread in _threadPool)
            {
                if (thread.Jobs.Count > 0)
                    count++;
            }
            return count;
        }
    }

    public ThreadPoolManager() : this(10)
    {
    }

    public ThreadPoolManager(int maxThreads)
    {
        if (maxThreads <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxThreads), "At least one thread must be enabled!");

        _maxThreads = maxThreads;

        _threadPool = new List<WorkerThread>();
        for (int i = 0; i < maxThreads; i++)
        {
            _threadPool.Add(new WorkerThread());
        }
    }

    ~ThreadPoolManager()
    {
        Shutdown();
    }



    public void Start()
    {
        new Thread(() =>
        {
            foreach (var thread in _threadPool)
            {
                thread.Start();
            }
        }).Start();
    }


    public void Stop()
    {
        foreach (var thread in _threadPool)
        {
            thread.Join();
        }
    }


    public void Stop(int milliseconds)
    {
        foreach (var thread in _threadPool)
        {
            thread.Join(milliseconds);
        }
    }


    public void StopAsync()
    {
        new Thread(Stop).Start();
    }


    public void StopAsync(int milliseconds)
    {
        new Thread(() => Stop(milliseconds)).Start();
    }


    public void ShutdownAsync()
    {
        new Thread(Shutdown).Start();
    }


    public void AddWork(WorkerThread.Function job)
    {
        int leastWork = Int32.MaxValue;
        int iLeastWork = 0;

        for (int i = 0; i < _maxThreads; i++)
        {
            int work = _threadPool[i].NumJobs;

            if (work == 0)
            {
                iLeastWork = i;
                break;
            }

            if (work < leastWork)
            {
                leastWork = work;
                iLeastWork = i;
            }
        }

        _threadPool[iLeastWork].AddNewJob(job);
    }


    public void Shutdown()
    {
        foreach (var thread in _threadPool)
            thread.Abort();
    }
}
