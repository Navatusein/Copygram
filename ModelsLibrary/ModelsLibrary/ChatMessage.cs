using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary
{
    [Serializable]
    public class ChatMessage : IMessage
    {
        public int ChatMessageId { get; set; }

        public User FromUser { get; set; } = null!;

        public string MessageText { get; set; } = null!;

        public MessageType Type { get; set; }

        public DateTime DispatchTime { get; set; } = DateTime.Now;
    }
}
