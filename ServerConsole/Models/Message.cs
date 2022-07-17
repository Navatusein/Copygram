using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConsole.Models
{
    internal class Message
    {
        public int MessageId { get; set; }

        public int ChatId { get; set; }

        public int UserId { get; set; }

        public string MessageText { get; set; } = null!;

        public virtual User User { get; set; } = null!;

        public virtual Chat Chat { get; set; } = null!;
    }
}
