using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConsole.Models
{
    public class Chat
    {
        public int ChatId { get; set; }

        public int ChatTypeId { get; set; }

        public string ChatName { get; set; } = null!;

        public byte[] Avatar { get; set; } = null!;

        public virtual HashSet<ChatMember> ChatMembers { get; set; } = new();

        public virtual HashSet<ChatMessage> ChatMessages { get; set; } = new();

        public virtual ChatType ChatType { get; set; } = null!;
    }
}
