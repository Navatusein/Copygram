using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    [Serializable]
    public class UserProfile
    {
        public int Id { get; set; }
        public string Nickname { get; set; } = null!;
        public byte[] Avatar { get; set; } = null!;
    }
}
