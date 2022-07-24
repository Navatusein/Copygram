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
        public static List<TCP.Chat> SyncChats(TCP.User tcpUser)
        {
            List<TCP.Chat> tcpChatList = new();

            int userId = tcpUser.UserId;

            using(CopygramDbContext dbContext = new())
            {
                tcpChatList = dbContext.Chats.Where(x => x.ChatMembers.Any(x => x.UserId == userId))
                    .Select(x => Mapper.DbModelToTcpModel(x, 1))
                    .ToList();
            }

            return tcpChatList;
        }

        public static TCP.User? TryToLogin(TCP.LoginData loginData)
        {
            TCP.User? tcpUser =null;

            using (CopygramDbContext dbContext = new())
            {
                DB.User? dbUser = dbContext.Users.FirstOrDefault(x => x.Login == loginData.Login && x.Password == loginData.Password);

                tcpUser = dbUser == null ? null : Mapper.DbModelToTcpModel(dbUser);
            }

            return tcpUser;
        }

        public static List<TCP.ChatMessage> SyncChatMessages(TCP.SyncChatMessages syncChatMessages)
        {
            List<TCP.ChatMessage> chatMessages;

            using (CopygramDbContext dbContext = new())
            {
                chatMessages = dbContext.Messages.Where(x => x.ChatId == syncChatMessages.ChatId)
                    .SkipWhile(x => x.ChatMessageId == syncChatMessages.MessageId)
                    .Take(syncChatMessages.MessageCount)
                    .Select(x => Mapper.DbModelToTcpModel(x))
                    .ToList();
            }

            return chatMessages;
        }

        public static bool IsLoginAllowed(string login)
        {
            bool allowed = false;

            using (CopygramDbContext dbContext = new())
            {
                allowed = !dbContext.Users.Any(x => x.Login == login);
            }

            return allowed;
        }

        public static bool IsNicknameAllowed(string nickname)
        {
            bool allowed = false;

            using (CopygramDbContext dbContext = new())
            {
                allowed = !dbContext.Users.Any(x => x.Nickname == nickname);
            }

            return allowed;
        }

        public static void RegisterUser(ref TCP.User user, string login, string password)
        {
            using (CopygramDbContext dbContext = new())
            {
                DB.User dbUser = Mapper.TcpModelToDbModel(user, login, password);
            
                var userData = dbContext.Users.Add(dbUser);

                dbContext.SaveChanges();

                user.UserId = userData.Entity.UserId;
            }
        }
    }
}
