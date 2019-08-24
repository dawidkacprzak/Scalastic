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
       private static void Main(string[] args)
        {
            RunProgramRunExample().GetAwaiter().GetResult();
            while (true)
            {
                Thread.Sleep(50000);
            }
        }

        private static async Task RunProgramRunExample()
        {
            try
            {
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" },
                    { "quartz.threadPool.threadCount", "1" }
                };
                StdSchedulerFactory factory = new StdSchedulerFactory(props);
                IScheduler scheduler = await factory.GetScheduler();

                List<Queries> list = new List<Queries>();
                using(ElasticContext ctx = new ElasticContext())
                {
                    list = ctx.Queries.Where(x => x.Active == 1).ToList();
                    foreach (var item in list)
                    {
                        IJobDetail job = JobBuilder.Create<QueryJob>()
                            .Build();
                        job.JobDataMap["query"] = item;

                        ITrigger trigger = TriggerBuilder.Create()
                            .StartNow()
                            .WithSimpleSchedule(x => x
                                .WithIntervalInMinutes(item.MinutePeriod)
                                .RepeatForever()).StartNow()
                            .Build();
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

        public class QueryJob : IJob
        {

            public async Task Execute(IJobExecutionContext context)
            {
                JobDataMap dataMap = context.JobDetail.JobDataMap;
                Queries query = (Queries)dataMap.Get("query");
                Console.WriteLine("Zaczeto task z zapytaniem:\n"+query.WhereQuery);
                ElasticController.Instance.StartImportToElastic(query.IndexName,query.WhereQuery,query.CountQuery);
            }
        }
    }
}
