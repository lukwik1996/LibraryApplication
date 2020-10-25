using System;
using System.Collections.Generic;
using System.Text;

namespace CommonData.Messages
{
    public interface IMessage<T>
    {
        string Action { get; set; }
        string UserId { get; set; }
        short StatusCode { get; set; }
        List<T> MessageContent { get; set; }
    }
}
