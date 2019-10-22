using System;
using Quartz;
using Quartz.Impl;
using System.Threading;
using Quartz.Xml;
using Quartz.Simpl;
using System.Runtime.Loader;

namespace Luna.Net.DDNS.Aliyun
{
    class Program
    {
        private static IScheduler scheduler;
        static readonly System.Threading.CancellationTokenSource _cs = new System.Threading.CancellationTokenSource();

        static void Main(string[] args)
        {
            // var cts = new CancellationTokenSource();

            AppDomain.CurrentDomain.ProcessExit += Processor_ProcessExit;
            AssemblyLoadContext.Default.Unloading += Processor_Unloading;
            Console.CancelKeyPress += Processor_CancelKeyPress;

            XMLSchedulingDataProcessor processor = new XMLSchedulingDataProcessor(new SimpleTypeLoadHelper());
            ISchedulerFactory sf = new StdSchedulerFactory();
            scheduler = sf.GetScheduler().Result;

            IJobDetail job = JobBuilder.Create<DdnsJob>().WithIdentity("DdnsJob", "DdnsJobGroup").Build();

            int sec = 300;
            if (!int.TryParse(ConfigUtil.GetConfigVariableValue("RefreshIntervalInSecond", "300"), out sec))
                sec = 300;

            ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
                  .WithIdentity("DdnsJobTrigger", "DdnsJobTriggerGroup")
                  .StartNow().WithSimpleSchedule(x => x.WithIntervalInSeconds(sec).RepeatForever()).Build();

            scheduler.ScheduleJob(job, trigger);

            // processor.ProcessFileAndScheduleJobs("~/quartz_jobs.xml", scheduler);

            // scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;

            scheduler.Start();
            Console.WriteLine($"[{DateTime.Now}]:后台服务，启动成功！");

           

            while (!_cs.IsCancellationRequested)
            {
                System.Threading.Thread.Sleep(1000);
            }

            return;

            
        }

        #region "Common Utils"
        private static void Processor_ProcessExit(object sender, EventArgs e)
        {
            if (!_cs.IsCancellationRequested)
            {
                _cs.Cancel();
            }
        }

        private static void Processor_Unloading(AssemblyLoadContext obj)
        {
            if (!_cs.IsCancellationRequested)
            {
                _cs.Cancel();
            }
        }

        private static void Processor_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (!_cs.IsCancellationRequested)
            {
                _cs.Cancel();
            }
        }

        #endregion

    }
}
