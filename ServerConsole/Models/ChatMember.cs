using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConsole.Models
{
    internal class ChatMember
    {
        public int ChatMemberId { get; set; }

        public int UserId { get; set; }

        public int ChatId { get; set; }

        public int ChatMemberRoleId { get; set; }

        public virtual User User { get; set; } = null!;

        public virtual Chat Chat { get; set; } = null!;

        public virtual ChatMemberRole ChatMemberRole { get; set; } = null!;
    }
}
