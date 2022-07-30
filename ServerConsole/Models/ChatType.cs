using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConsole.Models
{
    public class ChatType
    {
        public int ChatTypeId { get; set; }

        public string ChatTypeName { get; set; } = null!;

        public virtual HashSet<Chat> Chats { get; set; } = new();
    }
}
