using System;
using System.Threading;
using System.Diagnostics;

namespace parLab2
{
    class Program
    {
        private static readonly int numberThreads = 4;
        private static readonly int dim = 1000000000;
        private static Thread[] threads = new Thread[numberThreads];
        private static int[] arr = new int[dim];
        private static int minValue = int.MaxValue;
        private static int minIndex = -1;
        private static readonly object lockForMin = new object();
        private static readonly object lockerForCount = new object();
        private static int threadCount = 0;

        static void Main(string[] args)
        {
            Program main = new Program();
            main.InitializeArray();
            Stopwatch stopwatch = Stopwatch.StartNew();
            main.SplitPartsAndStartThreads();
            stopwatch.Stop();
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Minimum value: {minValue} at index: {minIndex}");
        }
        private void InitializeArray()
        {
            Random random = new Random();
            for (int i = 0; i < dim; i++)
            {
                arr[i] = random.Next(1, 1000000);
            }
            int negIndex = random.Next(dim);
            arr[negIndex] = -random.Next(1, 1000000);
        }
        public void SplitPartsAndStartThreads()
        {
            int partSize = dim / numberThreads;
            for (int i = 0; i < numberThreads; i++)
            {
                int start = i * partSize;
                int end = (i == numberThreads - 1) ? dim : (i+1) * partSize;
                threads[i] = new Thread ( () => {
                    FindMin(start, end);
                    IncThreadCount();
                });
                threads[i].Start();
            }
           lock(lockerForCount)
           {
            while (threadCount < numberThreads)
            {
                Monitor.Wait(lockerForCount);
            }
           }
        }

    private void FindMin(int start, int end)
    {
        int localMinValue = int.MaxValue;
        int localMinIndex = -1;
        for (int i = start; i < end; i++)
        {
            if (arr[i] < localMinValue)
            {
                localMinValue = arr[i];
                localMinIndex = i;
            }
        }
        lock (lockForMin)
        {
            if (localMinValue < minValue)
            {
                minValue = localMinValue;
                minIndex = localMinIndex;
            }
        }
    }
    
    private void IncThreadCount()
        {
            lock(lockerForCount)
            {
                threadCount++;
                Monitor.Pulse(lockerForCount);
            }
        }
    }
}