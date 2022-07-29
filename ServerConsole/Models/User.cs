using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConsole.Models
{
    public class User
    {
        public int UserId { get; set; }

        public byte[] Avatar { get; set; } = null!;

        public string Nickname { get; set; } = null!;

        public string Login { get; set; } = null!;

        public string Password { get; set; } = null!;

        public virtual HashSet<ChatMember> ChatMembers { get; set; } = new();
    }
}
