using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary
{
    public enum SystemMessageType
    {
        NewChat,
        UpdateChat
    }


    [Serializable]
    public class SystemMessage : IMessage
    {
        public MessageType Type { get; private set; }

        public SystemMessageType SystemMessageType { get; set; }

        public byte[] Data { get; set; } = null!;

        public SystemMessage()
        {
            Type = MessageType.SystemMessage;
        }
    }
}
