using ServerConsole.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using TCP = ModelsLibrary;
using DB = ServerConsole.Models;

namespace ServerConsole
{
    internal class DbConnector
    {
        public static List<TCP.Chat> GetChatsForUser(TCP.User tcpUser)
        {
            List<TCP.Chat> tcpChatList = new();

            int userId = tcpUser.UserId;

            using(CopygramDbContext dbContext = new())
            {
                tcpChatList = dbContext.Chats.Where(x => x.ChatMembers.Any(x => x.UserId == userId)).Select(x => Mapper.DbModelToTcpModel(x, 1)).ToList();
            }

            return tcpChatList;
        }
    }
}
