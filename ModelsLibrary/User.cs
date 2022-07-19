using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ModelsLibrary
{
    [Serializable]
    public class User
    {
        public int Id { get; set; }
        public string Nickname { get; set; } = null!;
        public Image Avatar { get; set; } = null!;
    }
}
