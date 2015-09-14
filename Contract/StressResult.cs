using System;
using System.Runtime.Serialization;

namespace Contract
{
    [DataContract]
    public class StressResult
    {
        /// <summary>
        /// 标识
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// 成功标识
        /// </summary>
        [DataMember]
        public int SuccessFlag { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        [DataMember]
        public string Msg { get; set; }

        /// <summary>
        /// 压力客户端开始调用时间
        /// </summary>
        [DataMember]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 调用总时长，单位为秒
        /// </summary>
        [DataMember]
        public TimeSpan TotalSpan { get; set; }
        
        /// <summary>
        /// 速度
        /// </summary>
        [DataMember]
        public double Speed { get; set; }

        public StressResult()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
