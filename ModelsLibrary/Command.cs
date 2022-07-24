﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary
{
    public enum CommandType
    {
        Login,
        Register,
        Sync,
        SyncChatMessage,
        Exception,
        RequestChanges,
        NewMessage,
        NewChat
    }

    [Serializable]
    public class Command
    {
        public CommandType Type { get; set; }

        public byte[] Data { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}