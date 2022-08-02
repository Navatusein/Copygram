using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConsole.Models
{
    public class ChatMemberRole
    {
        public int ChatMemberRoleId { get; set; }

        public string RoleName { get; set; } = null!;

        public virtual HashSet<ChatMember> ChatMembers { get; set; } = new();
    }
}
