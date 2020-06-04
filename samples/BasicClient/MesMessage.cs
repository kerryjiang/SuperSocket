using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BasicClient
{
    public class MesMessage
    {
        /// <summary>
        /// 事务码，全局唯一，同一组会话的所有Session共用同一个Transaction
        /// </summary>
        public string Transaction { get; set; }
        /// <summary>
        /// 发送消息的时间，要求格式为yyyy-MM-dd HH:mm:ss
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// Session发起者的Id
        /// </summary>
        public string SourceId { get; set; }
        /// <summary>
        /// Session响应者的Id
        /// </summary>
        public string TargetId { get; set; }
        /// <summary>
        /// 指令类型
        /// </summary>
        public string Command { get; set; }
        /// <summary>
        /// 指令参数，其具体格式需要在接收到消息时根据SourceId和TargetId判断，可能是
        /// CommandParameter（如果SourceId是消息接收者）或者FeedbackParameter（如果
        /// SourceId是消息发送者）
        /// </summary>
        public object CommandParameter { get; set; }
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
