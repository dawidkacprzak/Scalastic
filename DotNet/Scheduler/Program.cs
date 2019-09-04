using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using DAL.Models;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;
using DAL;

namespace Scheduler
{
    class Program
    {
        static List<Queries> scheduledTasks;

        private static void Main(string[] args)
        {
            scheduledTasks = new List<Queries>();
            Scheduler().GetAwaiter().GetResult();
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(5000);
                    var copyTasks = (Queries[])scheduledTasks.ToArray().Clone();
                    scheduledTasks.Clear();
                    Console.WriteLine("Wyczyszczono kolejke taskow. Znaleziono: " + copyTasks.Count());
                    foreach (var item in copyTasks)
                    {
                        Console.WriteLine("Rozpoczeto import zapytania: " + item.IndexName);
                        ElasticController.Instance.StartImportToElastic(item.IndexName, item.Query);
                    }
                }
            });
            while (true)
            {
                Thread.Sleep(5000);
            }
        }
        private static async Task Scheduler()
        {
            try
            {
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" },
                    { "quartz.threadPool.threadCount", "20" }
                };
                StdSchedulerFactory factory = new StdSchedulerFactory(props);
                IScheduler scheduler = await factory.GetScheduler();

                List<Queries> list = new List<Queries>();
                using(ElasticContext ctx = new ElasticContext())
                {
                    list = ctx.Queries.Where(x => x.Active == 1).ToList();
                    foreach (var item in list)
                    {
                        IJobDetail job = JobBuilder.Create<AddQueryToQueueJob>().
                           WithIdentity(item.Id.ToString())
                           .Build();
                        job.JobDataMap["query"] = item;


                        ITrigger trigger = TriggerBuilder.Create()

                        .WithIdentity(item.Id.ToString())
                        .WithSimpleSchedule(x => x.WithIntervalInMinutes(item.MinutePeriod).RepeatForever())
                        .Build();
                        Console.WriteLine("Dodano: " + item.IndexName);
                        await scheduler.ScheduleJob(job, trigger);
                    }
                }
                await scheduler.Start();
            }
            catch (SchedulerException se)
            {
                Console.WriteLine(se);
            }
        }

        public class AddQueryToQueueJob : IJob
        {

            public async Task Execute(IJobExecutionContext context)
            {
                JobDataMap dataMap = context.JobDetail.JobDataMap;
                Queries query = (Queries)dataMap.Get("query");
                Console.WriteLine("Dodano do listy zapytanie: "+query.IndexName);
                scheduledTasks.Add(query);
                return;
            }
        }
    }
}
