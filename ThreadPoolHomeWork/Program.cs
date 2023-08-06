using System;
using ThreadPoolHomeWork;
using ThreadPoolProject;

namespace ThreadPoolHomeWork
{
    internal class Program
    {
        private static readonly object _lockMain = new object();

        static void Main(string[] args)
        {
            IThreadPoolJuli threadPool = new ThreadPoolJuli(5, new MessageErrorShower());

            for(int i = 0; i < Console.WindowWidth; i++)
            {
                PriorityThread priority = i % 3 == 0? PriorityThread.HIGH :
                    i % 2 == 0? PriorityThread.LOW :
                    PriorityThread.NORMAL;              


                Action<object?> action = (ml) =>
                {
                    var marginLeft = (int)ml;

                    for(int k = 0; k < 8; k++)
                    {                        
                        lock (_lockMain)
                        {                            
                            Console.SetCursorPosition(marginLeft, k);
                            Console.WriteLine('*');
                        }
                    }               
                };

                threadPool.Execute(action, i, priority);

                if(i == Console.WindowWidth / 2)
                {
                    threadPool.Stop();
                }
            }

            Console.ReadLine();

        }

    }
}




