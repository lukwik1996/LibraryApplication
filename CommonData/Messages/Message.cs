using System;
using System.Collections.Generic;
using System.Text;

namespace CommonData.Messages
{
    public class Message<T> : IMessage<T>
    {
        public string Action { get; set; }
        public string UserId { get; set; }
        public short StatusCode { get; set; }
        public List<T> MessageContent { get; set; }
    }
}
