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
    internal static class Mapper
    {
        #region DB Models To TCP Model
        /// <summary>
        /// Function for passing a DB.Chat object to TCP.Chat.
        /// </summary>
        /// <param name="dbChat">DB.Chat object</param>
        /// <param name="messagesCount">The number of messages to be loaded from the database</param>
        /// <returns>The converted TCP.Chat object</returns>
        public static TCP.Chat DbModelToTcpModel(DB.Chat dbChat, int messagesCount)
        {
            TCP.Chat tcpChat = new();

            tcpChat.ChatId = dbChat.ChatId;
            tcpChat.ChatType = (TCP.ChatType)dbChat.ChatTypeId;
            tcpChat.ChatName = dbChat.ChatName;
            tcpChat.Avatar = dbChat.Avatar;
            tcpChat.ChatMembers = dbChat.ChatMembers.Select(x => DbModelToTcpModel(x)).ToList();
            tcpChat.Messages = dbChat.ChatMessages.TakeLast(messagesCount).Select(x => DbModelToTcpModel(x)).ToList();

            return tcpChat;
        }

        /// <summary>
        /// Function for passing a DB.ChatMember object to TCP.ChatMember.
        /// </summary>
        /// <param name="dbChatMember">DB.ChatMember object</param>
        /// <returns>The converted TCP.ChatMember object</returns>
        public static TCP.ChatMember DbModelToTcpModel(DB.ChatMember dbChatMember)
        {
            TCP.ChatMember tcpChatMember = new();

            tcpChatMember.ChatMemberId = dbChatMember.ChatMemberId;
            tcpChatMember.User = DbModelToTcpModel(dbChatMember.User);
            tcpChatMember.ChatId = dbChatMember.ChatId;
            tcpChatMember.ChatMemberRole = (TCP.ChatMemberRole)dbChatMember.ChatMemberId;

            return tcpChatMember;
        }

        /// <summary>
        /// Function for passing a DB.ChatMessage object to TCP.ChatMessage.
        /// </summary>
        /// <param name="dbChatMessage">DB.ChatMessage object</param>
        /// <returns>The converted TCP.ChatMessage object</returns>
        public static TCP.ChatMessage DbModelToTcpModel(DB.ChatMessage dbChatMessage)
        {
            TCP.ChatMessage tcpChatMessage = new();

            tcpChatMessage.ChatMessageId = dbChatMessage.ChatMessageId;
            tcpChatMessage.ChatId = dbChatMessage.ChatId;
            tcpChatMessage.FromUser = DbModelToTcpModel(dbChatMessage.FromUser);
            tcpChatMessage.MessageText = dbChatMessage.MessageText;
            tcpChatMessage.DispatchTime = dbChatMessage.DispatchTime;

            return tcpChatMessage;
        }

        /// <summary>
        /// Function for passing a DB.User object to TCP.User.
        /// </summary>
        /// <param name="dbUser">DB.User object</param>
        /// <returns>The converted TCP.User object</returns>
        public static TCP.User DbModelToTcpModel(DB.User dbUser)
        {
            TCP.User tcpUser = new();

            tcpUser.UserId = dbUser.UserId;
            tcpUser.Avatar = dbUser.Avatar;
            tcpUser.Nickname = dbUser.Nickname;

            return tcpUser;
        }

        #endregion

        #region TCP Model To DB Model
        /// <summary>
        /// Function for passing a TCP.Chat object to DB.Chat.
        /// </summary>
        /// <param name="tcpChat">TCP.Chat object</param>
        /// <returns>The converted DB.Chat object</returns>
        public static DB.Chat TcpModelToDbModel(TCP.Chat tcpChat)
        {
            DB.Chat dbChat = new();

            dbChat.ChatId = tcpChat.ChatId;
            dbChat.ChatTypeId = (int)tcpChat.ChatType;
            dbChat.ChatName = tcpChat.ChatName;
            dbChat.Avatar = tcpChat.Avatar;

            return dbChat;
        }


        /// <summary>
        /// Function for passing a TCP.ChatMember object to DB.ChatMember.
        /// </summary>
        /// <param name="tcpChatMember">TCP.ChatMember object</param>
        /// <returns>The converted DB.ChatMember object</returns>
        public static DB.ChatMember TcpModelToDbModel(TCP.ChatMember tcpChatMember)
        {
            DB.ChatMember dbChatMember = new();

            dbChatMember.ChatMemberId = tcpChatMember.ChatMemberId;
            dbChatMember.UserId = tcpChatMember.User.UserId;
            dbChatMember.ChatId = tcpChatMember.ChatMemberId;
            dbChatMember.ChatMemberRoleId = (int)tcpChatMember.ChatMemberRole;

            return dbChatMember;
        }


        /// <summary>
        /// Function for passing a TCP.User object to DB.User.
        /// </summary>
        /// <param name="tcpUser">TCP.User object</param>
        /// <param name="login">User login</param>
        /// <param name="password">User password</param>
        /// <returns>The converted DB.User object</returns>
        public static DB.User TcpModelToDbModel(TCP.User tcpUser, string login, string password)
        {
            DB.User dbUser = new();

            dbUser.UserId = tcpUser.UserId;
            dbUser.Avatar = tcpUser.Avatar;
            dbUser.Nickname = tcpUser.Nickname;
            dbUser.Login = login;
            dbUser.Password = password;

            return dbUser;
        }

        /// <summary>
        /// Function for passing a TCP.ChatMessage object to DB.ChatMessage.
        /// </summary>
        /// <param name="tcpChatMessage">TCP.ChatMessage object</param>
        /// <returns>The converted DB.ChatMessage object</returns>
        public static DB.ChatMessage TcpModelToDbModel(TCP.ChatMessage tcpChatMessage)
        {
            DB.ChatMessage dbChatMessage = new();

            dbChatMessage.ChatMessageId = tcpChatMessage.ChatMessageId;
            dbChatMessage.ChatId = tcpChatMessage.ChatId;
            dbChatMessage.UserId = tcpChatMessage.FromUser.UserId;
            dbChatMessage.MessageText = tcpChatMessage.MessageText;
            dbChatMessage.DispatchTime = tcpChatMessage.DispatchTime;

            return dbChatMessage;
        }

        #endregion
    }
}
