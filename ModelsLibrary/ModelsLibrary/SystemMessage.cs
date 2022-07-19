using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary
{
    [Serializable]
    public class SystemMessage : IMessage
    {
        public int Id { get; set; }

        public MessageType Type { get; set; }

        public byte[] Data { get; set; } = null!;

        public UserProfile FromUser { get; set; } = null!;
    }
}
