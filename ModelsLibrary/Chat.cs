using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ModelsLibrary
{
    public enum ChatType
    {
        Private,
        Group
    }

    [Serializable]
    public class Chat
    {
        public int Id { get; set; }

        public ChatType Type { get; set; }

        public string Name { get; set; } = null!;

        public Image Avatar { get; set; } = null!;

        public List<Message> messages { get; set; } = null!;
    }
}
