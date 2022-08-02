using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConsole.Models
{
    public class ChatMessage
    {
        public int ChatMessageId { get; set; }

        public int ChatId { get; set; }

        public int UserId { get; set; }

        public string MessageText { get; set; } = null!;

        public DateTime DispatchTime { get; set; } = DateTime.Now;

        public virtual User FromUser { get; set; } = null!;

        public virtual Chat Chat { get; set; } = null!;
    }
}
