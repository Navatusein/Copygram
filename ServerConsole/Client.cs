using ModelsLibrary;
using ServerConsole.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TCP = ModelsLibrary;
using DB = ServerConsole.Models;

namespace ServerConsole
{
    internal class Client
    {
        public TCP.User User { get; private set; }

        public List<IMessage> Changes { get; set; }

        public DateTime LastRequest { get; set; }

        public Client(TCP.User user)
        {
            this.User = user;
            this.Changes = new();
        }
    }
}
