using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading;
using Contract;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace StressClient
{
    static class Program
    {
        private static MongoCollection<StressResult> _collection;
        private static MongoCollection<StressResult> Collection
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                if (_collection == null)
                {
                    BsonSerializer.RegisterSerializer(typeof(DateTime), new DateTimeSerializer(DateTimeSerializationOptions.LocalInstance));
                    var client = new MongoClient("mongodb://" + Properties.Settings.Default.MongoDbHost);
                    var db = client.GetServer().GetDatabase("StressTest");
                    _collection = db.GetCollection<StressResult>(typeof(StressResult).Name);
                }
                return _collection;
            }
        }

        static void Work(ref bool flag, ref StressResult successfulResult, ConcurrentQueue<StressResult> failedResults, StringBuilder sbOfFailedStress)
        {
            while (flag)
            {
                try
                {
                    //生成客户端业务代理
                    using (var factory = new ChannelFactory<IStressService>(typeof(IStressService).Name))
                    {
                        var channel = factory.CreateChannel();
                        var subFlag = true;
                        while (flag && subFlag)
                        {
                            var now = DateTime.Now;
                            StressResult result;
                            try
                            {
                                //往压力服务端送数据
                                result = channel.Stress();
                                successfulResult.Speed = result.Speed;
                                successfulResult.TotalSpan = result.TotalSpan;
                           }
                            catch (Exception ex)
                            {
                                subFlag = false;
                                result = new StressResult
                                {
                                    SuccessFlag = -1,
                                    Msg = ex.ToString()
                                };
                                failedResults.Enqueue(result);
                           }

                            //增加压力结果
                            result.StartTime = now;
                            result.TotalSpan = DateTime.Now - now;

                            //Thread.Sleep(100);
                        }
                    }
                }
                catch (Exception ex)
                {
                    sbOfFailedStress.Clear();
                    sbOfFailedStress.AppendLine(ex.ToString());
                }
            }

            Console.Write(@".");
        }


        static void Main()
        {
            #region 要用到的公共数据
            var flag = true;
            var sbOfFailedStress = new StringBuilder();
            var sbOfFailedStatistic = new StringBuilder();
            var successfulResult = new StressResult();
            var failedResults = new ConcurrentQueue<StressResult>();
            #endregion

            #region 启动后台线程刷新屏幕显示
            new Thread(() =>
            {
                while (true)
                {
                    Console.Clear();

                    Console.WriteLine(@"速度（次/秒）：" + successfulResult.Speed);
                    Console.WriteLine(@"时长（秒）：" + successfulResult.TotalSpan.TotalSeconds);

                    Console.WriteLine(@"---------------------------------------");
                    Console.WriteLine(@"压力失败信息：");
                    Console.WriteLine(sbOfFailedStress.ToString());

                    Console.WriteLine(@"---------------------------------------");
                    Console.WriteLine(@"保存失败信息：");
                    Console.WriteLine(sbOfFailedStatistic.ToString());

                    Console.WriteLine(@"---------------------------------------");
                    Console.WriteLine(@"若要结束请按回车键后等待（不要直接退出）");

                    Thread.Sleep(1000);
                }
            })
            {
                Name = "刷新屏幕显示",
                IsBackground = true
            }.Start();
            #endregion

            #region 启动后台线程送出统计结果
            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        StressResult result;
                        while (failedResults.TryDequeue(out result))
                        {
                            Collection.Save(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        sbOfFailedStatistic.Clear();
                        sbOfFailedStatistic.AppendLine("保存统计结果时出错---------------");
                        sbOfFailedStatistic.AppendLine(ex.Message);
                        sbOfFailedStatistic.AppendLine("---------------------------------");
                    }

                    Thread.Sleep(1000);
                }
            })
            {
                Name = "刷新屏幕显示",
                IsBackground = true
            }.Start();
            #endregion

            #region 启动后台批量线程起压
            Enumerable.Range(1, 150).ToList().ForEach(i =>
            {
                var thread = new Thread(() => Work(ref flag, ref successfulResult, failedResults, sbOfFailedStress))
                {
                    Name = "压力线程" + i,
                    IsBackground = false,
                };
                thread.Start();
            });
            #endregion

            //结束运行
            Console.ReadKey();
            flag = false;
        }
    }
}
