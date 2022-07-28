using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary
{
    public enum SystemChatMessageType
    {
        NewChat,
        UpdateChat
    }


    [Serializable]
    public class SystemChatMessage : IMessage
    {
        public MessageType Type { get; private set; }

        public SystemChatMessageType SystemMessageType { get; set; }

        public Chat Chat { get; set; } = null!;

        public SystemChatMessage()
        {
            Type = MessageType.SystemMessage;
        }
    }
}
