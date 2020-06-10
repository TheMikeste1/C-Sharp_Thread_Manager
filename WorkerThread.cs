using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;

class WorkerThread
{
    public delegate void Function();

    private Thread _workerThread;

    private Semaphore _workSemaphore;
    private Mutex _jobsMutex = new Mutex();

    private Queue<Function> _jobs = new Queue<Function>();

    private bool _abort = false;
    private bool _join = false;

    private bool _started = false;
    

    public Queue<Function> Jobs
    {
        get { return _jobs; }
    }



    public WorkerThread()
    {
        _workSemaphore = new Semaphore(0, Int32.MaxValue);
    }


    public WorkerThread(Function job) : this()
    {
        AddNewJob(job);
    }


    public WorkerThread(IEnumerable<Function> jobs) : this()
    {
        AddNewJob(jobs);
    }


    public void Start()
    {
        _workerThread = new Thread(ThreadLoop);
        _workerThread.Start();
        _started = true;
    }


    private void ThreadLoop()
    {
        while (true)
        {
            _workSemaphore.WaitOne();

            if (_abort)
                break;

            if (_jobs.Count != 0)
            {
                _jobsMutex.WaitOne();
                Function newJob = _jobs.Peek();
                _jobsMutex.ReleaseMutex();

                newJob.Invoke();

                _jobsMutex.WaitOne();
                _jobs.Dequeue();
                _jobsMutex.ReleaseMutex();
            }
            else if (_join)
            {
                break;
            }
        }
    }


    public void AddNewJob(IEnumerable<Function> jobs)
    {
        foreach (var job in jobs)
        {
            AddNewJob(job);
        }
    }

    public void AddNewJob(Function job)
    {
        _jobs.Enqueue(job);
        _workSemaphore.Release();
    }


    public void Abort()
    {
        _abort = true;

        if (_started)
        {
            try
            {
                _workerThread.Abort();
            }
            catch (Exception e)
            {
                Join();
            }
        }

        _abort = false;
    }


    public void Join()
    {
        if (!_started)
            return;

        _join = true;
        _workSemaphore.Release();
        _workerThread.Join();
        _join = false;
    }


    public bool Join(int milliseconds)
    {
        if (!_started)
            return true;

        _join = true;
        _workSemaphore.Release();
        if (!_workerThread.Join(milliseconds))
        {
            _join = false;
            _workSemaphore.WaitOne(10);
            return false;
        }

        _join = false;
        return true;
    }
}
