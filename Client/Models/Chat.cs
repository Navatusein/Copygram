using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public enum ChatType
    {
        Private,
        Group
    }

    [Serializable]
    public class Chat
    {
        public int ChatId { get; set; }

        public ChatType ChatType { get; set; }

        public string ChatName { get; set; } = null!;

        public byte[] Avatar { get; set; } = null!;

        public List<ChatMember> ChatMembers { get; set; } = null!;

        public List<ChatMessage> Messages { get; set; } = null!;
    }
}
