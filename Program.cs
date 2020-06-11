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
            ThreadPoolManager tpm = new ThreadPoolManager();
            
            tpm.Start();
            
            for (int i = 0; i < 20; i++)
            {
                var i1 = i;
                tpm.AddWork(() =>
                {
                    ThreadTest(i1);
                    Thread.Sleep(1000);
                });
            }
            
            tpm.Stop();
        }
    }
}
