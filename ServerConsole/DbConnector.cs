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
        /// <summary>
        /// Function to check if the login is free to register.
        /// </summary>
        /// <param name="login">Login to check</param>
        /// <returns>
        /// True if the login is free. 
        /// False if login is busy.
        /// </returns>
        public static bool IsLoginAllowed(string login)
        {
            bool allowed = false;

            using (CopygramDbContext dbContext = new())
            {
                allowed = !dbContext.Users.Any(x => x.Login == login);
            }

            return allowed;
        }

        /// <summary>
        /// Function to check if the nickname is free to register.
        /// </summary>
        /// <param name="nickname">Nickname to check</param>
        /// <returns>
        /// True if the nickname is free. 
        /// False if nickname is busy.
        /// </returns>
        public static bool IsNicknameAllowed(string nickname)
        {
            bool allowed = false;

            using (CopygramDbContext dbContext = new())
            {
                allowed = !dbContext.Users.Any(x => x.Nickname == nickname);
            }

            return allowed;
        }

        /// <summary>
        /// The function of adding a new client to the database.
        /// </summary>
        /// <param name="user">User object</param>
        /// <param name="login">User login</param>
        /// <param name="password">User password</param>
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

        /// <summary>
        /// Function for adding a new message to the database.
        /// </summary>
        /// <param name="tcpChatMessage">Message object</param>
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

        /// <summary>
        /// Function for adding a new chat to the database.
        /// </summary>
        /// <param name="tcpChat">Chat object</param>
        public static void RegisterNewChat(ref TCP.Chat tcpChat)
        {
            using (CopygramDbContext dbContext = new())
            {
                DB.Chat dbChat = Mapper.TcpModelToDbModel(tcpChat);

                var dbChatData = dbContext.Chats.Add(dbChat);

                dbContext.SaveChanges();

                int chatId = tcpChat.ChatId = dbChatData.Entity.ChatId;

                List<DB.ChatMember> dbChatMembers = tcpChat.ChatMembers.Select(x => Mapper.TcpModelToDbModel(x)).ToList();

                dbChatMembers.ForEach(x => x.ChatId = chatId);

                dbContext.ChatMembers.AddRange(dbChatMembers);

                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// The function of updating the chat in the database.
        /// </summary>
        /// <param name="tcpChat">Chat object</param>
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

        /// <summary>
        /// A function to get a list of chats for a specific user.
        /// </summary>
        /// <param name="tcpUser">User object</param>
        /// <returns></returns>
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

        /// <summary>
        /// Function to synchronize part of the messages in the chat.
        /// </summary>
        /// <param name="syncChatMessages">Synchronization class object</param>
        /// <returns>List of requested messages</returns>
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

        /// <summary>
        /// A function to get a list of users from a chat.
        /// </summary>
        /// <param name="chatId">Chat ID to get users from</param>
        /// <returns>A list of users</returns>
        public static List<TCP.User> GetUsersFromChat(int chatId)
        {
            List<TCP.User> users;

            using (CopygramDbContext dbContext = new())
            {
                users = dbContext.ChatMembers.Where(x => x.ChatId == chatId).Select(x => Mapper.DbModelToTcpModel(x.User)).ToList();
            }

            return users;
        }

        /// <summary>
        /// Function for checking and finding a user by login and password.
        /// </summary>
        /// <param name="loginData">LoginData object containing login and password</param>
        /// <returns>
        /// User object if found.
        /// Null if the user is not found.
        /// </returns>
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

        /// <summary>
        /// Function to check and get a user object with a given nickname.
        /// </summary>
        /// <param name="nickname">User nickname</param>
        /// <returns>User object</returns>
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
    }
}
