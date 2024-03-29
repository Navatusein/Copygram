﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary
{
    [Serializable]
    public enum MessageType
    {
        SystemChatMessage,
        ChatMessage
    }

    public interface IMessage
    {
        public MessageType Type { get; }
    }
}
