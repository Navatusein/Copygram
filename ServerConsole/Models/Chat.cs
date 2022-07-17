using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConsole.Models
{
    internal class Chat
    {
        public Chat()
        {
            ChatMembers = new HashSet<ChatMember>();
            Messages = new HashSet<Message>();
        }

        public int ChatId { get; set; }

        public int ChatTypeId { get; set; }

        public string ChatName { get; set; }

        public byte[] Avatar { get; set; }

        public virtual ICollection<ChatMember> ChatMembers { get; set; }

        public virtual ICollection<Message> Messages { get; set; }

        public virtual ChatType ChatType { get; set; }
    }
}
