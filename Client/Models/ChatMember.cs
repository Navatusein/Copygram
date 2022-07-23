using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public enum ChatMemberRole
    {
        Member,
        Moderator,
        Owner
    }

    [Serializable]
    public class ChatMember
    {
        public int ChatMemberId { get; set; }

        public User User { get; set; } = null!;

        public ChatMemberRole ChatMemberRole { get; set; }
    }
}
