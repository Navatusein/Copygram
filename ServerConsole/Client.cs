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
        private TCP.User User = null!;

        public List<IMessage> Changes { get; set; } = null!;

        public Client()
        {

        }
    }
}
