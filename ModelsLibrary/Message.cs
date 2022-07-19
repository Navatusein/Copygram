using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary
{
    [Serializable]
    public class Message : IMessage
    {
        public int Id { get; set; }

        public MessageType Type { get; set; }

        public string MessageText { get; set; } = null!;

        public User FromUser { get; set; } = null!;
    }
}
