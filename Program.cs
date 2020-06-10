using System;
using System.Collections.Generic;
using System.Threading;



namespace C_Sharp_Threading_Test
{










    class Program
    {
        public static void ThreadTest(int num)
        {
            Thread th = Thread.CurrentThread;
            Console.WriteLine("This is {0}, received {1}", th.ManagedThreadId, num);
        }


        static void Main(string[] args)
        {
            int num = 1234;


            WorkerThread wtm = new WorkerThread();
            wtm.Start();

            List<WorkerThread.Function> functions = new List<WorkerThread.Function>();

            for (int i = 0; i < 100; i++)
            {
                var i1 = i;
                functions.Add((() => ThreadTest(i1)));
            }

            wtm.AddNewJob(functions);

            wtm.Join();
        }
    }
}
