using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    [Serializable]
    public class User
    {
        public int UserId { get; set; }

        public byte[] Avatar { get; set; } = null!;

        public string Nickname { get; set; } = null!; 
    }
}
