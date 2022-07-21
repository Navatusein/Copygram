using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsLibrary
{
    [Serializable]
    public class UserProfile
    {
        public int Id { get; set; }
        public string Nickname { get; set; } = null!;
        public Image Avatar { get; set; } = null!;
    }
}
