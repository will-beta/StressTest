using System;
using System.Collections.Concurrent;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using Contract;


namespace StressServer
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“Service1”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 Service1.svc 或 Service1.svc.cs，然后开始调试。
    public class StressService : IStressService
    {
        private static DateTime _starTime;
        private static long _concurrentCalls;

        static StressService()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    _starTime = DateTime.Now;
                    _concurrentCalls = 0;

                    Thread.Sleep(5000);
                }
            });
        }

        public StressResult Stress()
        {
            Interlocked.Increment(ref _concurrentCalls);

            Thread.SpinWait(1000);
            return new StressResult
            {
                Speed = _concurrentCalls / (DateTime.Now - _starTime).TotalSeconds
            };
        }
    }
}
