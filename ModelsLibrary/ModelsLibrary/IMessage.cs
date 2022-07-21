using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary
{
    [Serializable]
    public enum MessageType
    {
        SystemMessage,
        Message
    }

    public interface IMessage
    {
        public int Id { get; set; }

        public MessageType Type { get; set; }
    }
}
