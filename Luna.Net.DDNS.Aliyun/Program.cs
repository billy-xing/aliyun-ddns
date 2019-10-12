using System;
using Quartz;
using Quartz.Impl;
using System.Threading;

namespace Luna.Net.DDNS.Aliyun
{
    class Program
    {
        private static IScheduler scheduler;

        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();

            scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;

            scheduler.Start();
            Console.WriteLine($"{DateTime.Now} 后台服务，启动成功！");

            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                Console.WriteLine($"{DateTime.Now} 后台服务，准备退出！");

                cts.Cancel();    //设置IsCancellationRequested=true
                scheduler.Shutdown();   //等待 scheduler 结束执行

                Console.WriteLine($"{DateTime.Now} 恭喜，服务程序已正常退出！");
                Environment.Exit(0);
            };

            while (!cts.IsCancellationRequested)
            {
                System.Threading.Thread.Sleep(1000);
            }

            return;

            
        }

        
        
    }
}
