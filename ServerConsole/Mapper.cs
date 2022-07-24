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
        public static TCP.Chat DbModelToTcpModel(DB.Chat dbChat, int messagesCount)
        {
            TCP.Chat tcpChat = new();

            tcpChat.ChatId = dbChat.ChatId;
            tcpChat.ChatType = (TCP.ChatType)dbChat.ChatTypeId;
            tcpChat.ChatName = dbChat.ChatName;
            tcpChat.Avatar = dbChat.Avatar;
            tcpChat.ChatMembers = dbChat.ChatMembers.Select(x => DbModelToTcpModel(x)).ToList();
            tcpChat.Messages = dbChat.Messages.TakeLast(messagesCount).Select(x => DbModelToTcpModel(x)).ToList();

            return tcpChat;
        }

        public static TCP.ChatMember DbModelToTcpModel(DB.ChatMember dbChatMember)
        {
            TCP.ChatMember tcpChatMember = new();

            tcpChatMember.ChatMemberId = dbChatMember.ChatMemberId;
            tcpChatMember.User = DbModelToTcpModel(dbChatMember.User);
            tcpChatMember.ChatId = dbChatMember.ChatId;
            tcpChatMember.ChatMemberRole = (TCP.ChatMemberRole)dbChatMember.ChatMemberId;

            return tcpChatMember;
        }

        public static TCP.ChatMessage DbModelToTcpModel(DB.ChatMessage dbChatMessage)
        {
            TCP.ChatMessage tcpChatMessage = new();

            tcpChatMessage.ChatMessageId = dbChatMessage.ChatMessageId;
            tcpChatMessage.ChatId = dbChatMessage.ChatId;
            tcpChatMessage.FromUser = DbModelToTcpModel(dbChatMessage.FromUser);
            tcpChatMessage.MessageText = dbChatMessage.MessageText;
            tcpChatMessage.Type = TCP.MessageType.ChatMessage;
            tcpChatMessage.DispatchTime = dbChatMessage.DispatchTime;

            return tcpChatMessage;
        }

        public static TCP.User DbModelToTcpModel(DB.User dbUser)
        {
            TCP.User tcpUser = new();

            tcpUser.UserId = dbUser.UserId;
            tcpUser.Avatar = dbUser.Avatar;
            tcpUser.Nickname = dbUser.Nickname;

            return tcpUser;
        }

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
    }
}
