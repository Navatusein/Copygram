using ServerConsole.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.EntityFrameworkCore;

using TCP = ModelsLibrary;
using DB = ServerConsole.Models;
using System.Drawing.Imaging;

namespace ServerConsole
{
    internal class DbConnector
    {
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

        public static void RegisterNewUser(ref TCP.User user, string login, string password)
        {
            using (CopygramDbContext dbContext = new())
            {
                DB.User dbUser = Mapper.TcpModelToDbModel(user, login, password);

                var userData = dbContext.Users.Add(dbUser);

                dbContext.SaveChanges();

                user.UserId = userData.Entity.UserId;
            }
        }

        public static void RegisterNewMessage(ref TCP.ChatMessage tcpChatMessage)
        {
            using (CopygramDbContext dbContext = new())
            {
                DB.ChatMessage dbChatMessage = Mapper.TcpModelToDbModel(tcpChatMessage);

                var chatMessageData = dbContext.ChatMessages.Add(dbChatMessage);

                dbContext.SaveChanges();

                tcpChatMessage.ChatMessageId = chatMessageData.Entity.ChatMessageId;
            }
        }

        public static void RegisterNewChat(ref TCP.Chat tcpChat)
        {
            using (CopygramDbContext dbContext = new())
            {
                DB.Chat dbChat = Mapper.TcpModelToDbModel(tcpChat);

                List<DB.ChatMember> dbChatMembers = tcpChat.ChatMembers.Select(x => Mapper.TcpModelToDbModel(x)).ToList();

                var dbChatData = dbContext.Chats.Add(dbChat);
                dbContext.ChatMembers.AddRange(dbChatMembers);

                dbContext.SaveChanges();

                tcpChat.ChatId = dbChatData.Entity.ChatId;
            }
        }

        public static void UpdateChat(TCP.Chat tcpChat)
        {
            using (CopygramDbContext dbContext = new())
            {
                DB.Chat dbChat = Mapper.TcpModelToDbModel(tcpChat);

                List<DB.ChatMember> dbChatMembersFromDb = dbContext.ChatMembers.Where(x => x.ChatId == tcpChat.ChatId).ToList();
                List<DB.ChatMember> dbChatMembersFromTcp = tcpChat.ChatMembers.Select(x => Mapper.TcpModelToDbModel(x)).ToList();

                List<DB.ChatMember> addDbUsers = dbChatMembersFromTcp.Except(dbChatMembersFromDb).ToList();
                List<DB.ChatMember> removeDbUsers = dbChatMembersFromDb.Except(dbChatMembersFromTcp).ToList();

                dbContext.Chats.Update(dbChat);

                if (addDbUsers.Count > 0)
                    dbContext.ChatMembers.AddRange(addDbUsers);

                if (removeDbUsers.Count > 0)
                    dbContext.ChatMembers.RemoveRange(removeDbUsers);

                dbContext.ChatMembers.UpdateRange(dbChatMembersFromTcp);
                
                dbContext.SaveChanges();
            }
        }

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

        public static List<TCP.ChatMessage> SyncChatMessages(TCP.SyncChatMessages syncChatMessages)
        {
            List<TCP.ChatMessage> chatMessages;

            using (CopygramDbContext dbContext = new())
            {
                DB.Chat? dbChat = dbContext.Chats.FirstOrDefault(x => x.ChatId == syncChatMessages.ChatId);

                if (dbChat == null)
                    return null!;

                int messageIndex = dbChat.ChatMessages.ToList().FindIndex(x => x.ChatMessageId == syncChatMessages.MessageId);

                int range = syncChatMessages.MessageCount;
                int startIndex = messageIndex - range;

                if (startIndex < 0)
                {
                    range += startIndex;
                    startIndex = 0;
                }

                List<DB.ChatMessage> dbChatMessages = dbChat.ChatMessages.ToList().GetRange(startIndex, range);
                chatMessages = dbChatMessages.Select(x => Mapper.DbModelToTcpModel(x)).ToList();
            }

            return chatMessages;
        }

        public static List<TCP.User> GetUsersFromChat(int chatId)
        {
            List<TCP.User> users;

            using (CopygramDbContext dbContext = new())
            {
                users = dbContext.ChatMembers.Where(x => x.ChatId == chatId).Select(x => Mapper.DbModelToTcpModel(x.User)).ToList();
            }

            return users;
        }

        public static TCP.User? TryToLogin(TCP.LoginData loginData)
        {
            TCP.User? tcpUser = null;

            using (CopygramDbContext dbContext = new())
            {
                DB.User? dbUser = dbContext.Users.FirstOrDefault(x => x.Login == loginData.Login && x.Password == loginData.Password);

                tcpUser = dbUser == null ? null : Mapper.DbModelToTcpModel(dbUser);
            }

            return tcpUser;
        }

        public static TCP.User? CheckForUser(string nickname)
        {
            TCP.User? tcpUser = null;

            using (CopygramDbContext dbContext = new())
            {
                DB.User? dbUser = dbContext.Users.FirstOrDefault(x => x.Nickname == nickname);

                if (dbUser != null)
                {
                    tcpUser = Mapper.DbModelToTcpModel(dbUser);
                }
            }

            return tcpUser;
        }

        public static void Test()
        {
            //Bitmap bitmap = new Bitmap("Cat.jpg");

            //byte[] data;

            //using (MemoryStream ms = new MemoryStream())
            //{
            //    bitmap.Save(ms, ImageFormat.Png);
            //    data = ms.ToArray();
            //}

            //using (CopygramDbContext context = new CopygramDbContext())
            //{
            //    var user = context.Chats.ToArray()[0];

            //    user.Avatar = data;

            //    context.Chats.Update(user);

            //    context.SaveChanges();
            //}
        }

    }
}
