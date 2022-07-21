using ModelsLibrary;
using ServerConsole.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConsole
{
    internal class Client
    {
        private User User = null!;

        public List<IMessage> Changes { get; set; } = null!;

        public Client()
        {

        }
    }
}
