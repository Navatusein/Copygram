﻿using Client.CustomControls;
using Client.Resources.Tools;
using ModelsLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

#pragma warning disable SYSLIB0011

namespace Client
{
    /// <summary>
    /// My controller class
    /// </summary>
    internal class Controller
    {
        User profile = null!;
        Chat activeChat = null!;
        IPEndPoint ep = null!;

        bool isLast = false;

        #region Collections
        ObservableCollection<UserCell> ChatList = null!;
        ObservableCollection<MessageContainer> MessagesList = null!;
        List<Chat> chats = null!;
        #endregion

        #region Props
        public User Profile
        {
            get { return profile; }
        }

        public bool IsLast
        {
            get { return isLast; }
            set { isLast = value; }
        }

        public Chat? GetActiveChat
        { 
            get { return activeChat == null ? null : activeChat; }
        }
        #endregion

        public Controller(ObservableCollection<UserCell> cl, ObservableCollection<MessageContainer> ml)
        {
            try
            {
                ChatList = cl;
                MessagesList = ml;
                ep = new IPEndPoint(IPAddress.Parse("178.151.124.250"), 27015);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Controller Constructor Error",
                        MessageBoxButton.OK);
            }
        }

        #region Background

        /// <summary>
        /// Background synchronization
        /// </summary>
        void StartBackgroundSync()
        {
            try
            {
                DispatcherTimer timer = new();
                timer.Tick += Timer_Tick;
                timer.Interval = new TimeSpan(0, 0, 5);
                Task.Run(timer.Start);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Background start synchronization Error",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Synchronization action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                Response response = Request(CommandType.RequestChanges, null);

                if (response.Type == ResponseType.Error) return;

                List<IMessage> messages = StreamTools.Deserialize<List<IMessage>>(response.Data);

                if (messages == null) return;

                foreach (IMessage message in messages)
                {
                    if (message.Type == MessageType.SystemChatMessage)
                    {
                        SystemChatMessage sysMsg = (message as SystemChatMessage)!;

                        if (sysMsg.SystemMessageType == SystemChatMessageType.NewChat)
                        {
                            chats.Add(sysMsg.Chat);
                        }
                        else
                        {
                            int index = chats.FindIndex(chat => chat.ChatId == sysMsg.Chat.ChatId);
                            chats[index] = sysMsg.Chat;
                        }
                    }
                    else
                    {
                        ChatMessage chatMsg = (message as ChatMessage)!;
                        chats.FirstOrDefault(chat => chat.ChatId == chatMsg.ChatId)!.Messages.Add(chatMsg);

                        if (activeChat != null && chatMsg.ChatId == activeChat.ChatId)
                            NewMessagesAdded(activeChat.ChatId);
                    }
                }

                if(messages.Count > 0)
                    NewChatsAdded();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Background update Error",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion

        #region View Loads

        /// <summary>
        /// Reflects chats
        /// </summary>
        public void NewChatsAdded()
        {
            try
            {
                ChatList.Clear();
                foreach (Chat chat in chats)
                {
                    User otherUser = chat.ChatMembers.FirstOrDefault(member => member.User.UserId != profile.UserId)!.User;
                    ChatList.Add(new UserCell()
                    {
                        ChatId = chat.ChatId,

                        AvatarSource = chat.ChatType == ChatType.Private ? 
                        StreamTools.ToBitmapImage(otherUser.Avatar) : StreamTools.ToBitmapImage(chat.Avatar),

                        Nickname = chat.ChatName == profile.Nickname ? otherUser.Nickname : chat.ChatName,

                        LastMessage = chat.Messages.Count > 0 ? chat.Messages.Last().MessageText : "No messages"
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Chats to cells Error",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Reflects selected chat
        /// </summary>
        /// <param name="name"></param>
        public void NewMessagesAdded(int chatId)
        {
            try
            {
                if (chatId < 0)
                    return;

                activeChat = chats.FirstOrDefault(chat => chat.ChatId == chatId)!;

                MessagesList.Clear();

                foreach (ChatMessage msg in activeChat.Messages)
                {
                    MessagesList.Add(new MessageContainer()
                    {
                        MessageText = msg.MessageText,
                        AvatartImage = StreamTools.ToBitmapImage(msg.FromUser.Avatar)
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "NewChats Chat Error",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void ScrollToOldMessages(int chatId)
        {
            try
            {
                if (chatId < 0 || isLast)
                    return;

                activeChat = chats.FirstOrDefault(chat => chat.ChatId == chatId)!;

                if (activeChat.Messages.Count == 0)
                {
                    MessagesList.Clear();
                    return;
                }

                int limit = 15;

                SyncChatMessages dataToSend = new()
                {
                    ChatId = activeChat.ChatId,
                    MessageCount = limit,
                    MessageId = activeChat.Messages.First().ChatMessageId
                };

                Response response = Request(CommandType.SyncChatMessage, dataToSend);
                List<ChatMessage> data = StreamTools.Deserialize<List<ChatMessage>>(response.Data);
                activeChat.Messages.InsertRange(0, data);

                if (data.Count < limit)
                    isLast = true;

                NewMessagesAdded(chatId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Scroll Chat Error",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion

        #region System funcs

        /// <summary>
        /// Send request of specific type and recieve response
        /// </summary>
        /// <param name="type">Request command type</param>
        /// <param name="toSerialize">Data to send on request</param>
        /// <returns>Type of response</returns>
        Response Request(CommandType type, object? toSerialize)
        {
            try
            {
                byte[] data = null!;

                if (toSerialize != null)
                    data = StreamTools.Serialize(toSerialize)!;

                TcpClient client = new();
                Response response = null!;
                
                if (!client.ConnectAsync(ep).Wait(300))
                {
                    byte[] dataToSend = StreamTools.Serialize(new Error() {
                        Type = KnownErrors.UnknownError,
                        Text = "Connection timout" })!;
                    response = new() { Type = ResponseType.Error, Data = dataToSend };
                    OnError(response);
                    return response;
                }

                NetworkStream netStream = client.GetStream();
                Command command = new() { Type = type, Data = data, User = profile };
                StreamTools.NetworkSend(netStream, command);

                StreamReader reader = new(netStream, Encoding.UTF8);
                response = StreamTools.NetworkGet(reader.BaseStream);
                netStream.Close();

                if (response.Type == ResponseType.Error) OnError(response);

                return response;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Request Error",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                return new Response() { Type = ResponseType.Error };
            }
        }     

        /// <summary>
        /// Closes server connection
        /// </summary>
        public void CloseServerConnection()
        {
            Response response = Request(CommandType.Disconnect, null);
            if(response.Type == ResponseType.Error) OnError(response);
        }

        /// <summary>
        /// Supposed to search chat
        /// </summary>
        /// <param name="text">Name of chat</param>
        public void SearchChat(string name)
        {
            try
            {
                ChatList.Clear();

                foreach (Chat chat in chats)
                {
                    if (chat.ChatName.ToLower().Contains(name.ToLower()))
                    {
                        ChatList.Add(new UserCell()
                        {
                            AvatarSource = StreamTools.ToBitmapImage(chat.Avatar),
                            Nickname = chat.ChatName,
                            LastMessage = chat.Messages.Count > 0 ? chat.Messages.Last().MessageText : "No message"
                        });
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Search Error",
                            MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// prcoesses errors from response
        /// </summary>
        /// <param name="response">Response with error type</param>
        void OnError(Response response)
        { 
            Error error = StreamTools.Deserialize<Error>(response.Data);

            switch (error.Type)
            {
                case KnownErrors.UnknownError:
                    MessageBox.Show(error.Text, "Unknown Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case KnownErrors.OutOfSync:
                    Sync();
                    break;
                case KnownErrors.SecondClient:
                    MessageBox.Show("There is already active sesion on other device", "Simulatious login", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case KnownErrors.BadPasswordOrLogin:
                    MessageBox.Show("Wrong login or password", "Bad login creditinals", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case KnownErrors.LoginBusy:
                    MessageBox.Show("There is already user with this login", "Try another login", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case KnownErrors.NicknameBusy:
                    MessageBox.Show("There is already user with this nickname", "Try another nickname", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case KnownErrors.UnknownCommand://///////////
                    break;
                case KnownErrors.UnknownUser://////////////// ----> Test purpose only
                    break;
                case KnownErrors.UnknownCommandArguments:///
                    break;
                case KnownErrors.ProcessingError:
                    MessageBox.Show(error.Text, "Something went wrong", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Main

        /// <summary>
        /// Tries to login user with current credentials
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>Status of login info</returns>
        public bool TryLogin(string username, string password)
        {
            try
            {
                LoginData dataToSend = new() { Login = username, Password = password };

                Response response = Request(CommandType.Login, dataToSend);

                if (response.Type == ResponseType.Error) return false;

                profile = StreamTools.Deserialize<User>(response.Data);

                Sync();
                
                StartBackgroundSync();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "TryLogin Error",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
        }

        public bool TryRegister(string username, string login, string password, byte[] avatarImage)
        {
            try
            {
                User loginUser = new() { Nickname = username, Avatar = avatarImage };
                LoginData dataToSend = new() { Login = login, Password = password, User = loginUser };

                Response response = Request(CommandType.Register, dataToSend);

                if (response.Type == ResponseType.Error) return false;

                profile = StreamTools.Deserialize<User>(response.Data);

                Sync();

                StartBackgroundSync();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "TryRegisterError",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
        }

        /// <summary>
        /// Syunchronizes chat messages
        /// </summary>
        void Sync()
        {
            try
            {
                Response response = Request(CommandType.Sync, null);

                if (response.Type == ResponseType.Error) return;

                chats = StreamTools.Deserialize<List<Chat>>(response.Data);
                NewChatsAdded();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Sync Error",
                            MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Sends messsage to srever and recieves reaction
        /// </summary>
        /// <param name="messageText"></param>
        /// <returns></returns>
        public void SendMessage(string messageText)
        {
            try
            {
                ChatMessage dataToSend = new()
                {
                    MessageText = messageText,
                    FromUser = profile,
                    ChatId = activeChat.ChatId
                };

                Response response = Request(CommandType.NewChatMessage, dataToSend);

                if (response.Type == ResponseType.Error) return;

                int index = chats.FindIndex(chat => chat.ChatId == activeChat.ChatId);

                chats[index].Messages.Add(StreamTools.Deserialize<ChatMessage>(response.Data));

                MessagesList.Add(new MessageContainer() { MessageText = messageText.Trim(), AvatartImage = StreamTools.ToBitmapImage(profile.Avatar) });
                NewMessagesAdded(activeChat.ChatId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Send Message Error",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion

        #region Adding chats

        /// <summary>
        /// Adds private chat
        /// </summary>
        /// <param name="nickname"></param>
        public bool AddPrivateChat(string nickname)
        {
            try
            {
                if (nickname == null) return false;

                Response response = Request(CommandType.CheckForUser, nickname);

                if (response.Type == ResponseType.Error) return false;

                User toUser = StreamTools.Deserialize<User>(response.Data);

                ChatMember me = new ChatMember() { User = profile, ChatMemberRole = ChatMemberRole.Owner };
                ChatMember to = new ChatMember() { User = toUser, ChatMemberRole = ChatMemberRole.Owner };

                chats.Add(activeChat = new Chat()
                {
                    Avatar = toUser.Avatar,
                    ChatName = toUser.Nickname,
                    ChatMembers = new List<ChatMember>() { me, to },
                    ChatType = ChatType.Private,
                    Messages = new List<ChatMessage>()
                });

                SystemChatMessage dataToSend = new() { SystemMessageType = SystemChatMessageType.NewChat, Chat = activeChat };

                response = Request(CommandType.NewSystemChatMessage, dataToSend);

                if(response.Type == ResponseType.Error) return false;

                NewChatsAdded();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Add private chat Error",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
        }

        /// <summary>
        /// Adds group chat
        /// </summary>
        /// <param name="nickname"></param>
        /// <param name="imagePath"></param>
        /// <param name="logins"></param>
        public bool AddGroupChat(string nickname, byte[] image, string logins)
        {
            try
            {
                if (string.IsNullOrEmpty(nickname) || string.IsNullOrEmpty(logins)) return false;

                if (logins.Contains(" "))
                    logins = logins.Replace(" ", "");

                string[] loginArray = logins.Split(',');

                List<User> members = new();
                Response? response = null!;

                foreach (string log in loginArray)
                {
                    response = Request(CommandType.CheckForUser, log);

                    if (response.Type == ResponseType.Success)
                        members.Add(StreamTools.Deserialize<User>(response.Data));
                }

                if (members.Count < 1) return false;

                List<ChatMember> chatMembers = new();

                chatMembers.Add(new ChatMember() {
                    User = profile,
                    ChatMemberRole = ChatMemberRole.Owner });

                foreach (User user in members)
                {
                    chatMembers.Add(new ChatMember() { User = user, ChatMemberRole = ChatMemberRole.Member });
                }

                chats.Add(activeChat = new Chat()
                {
                    Avatar = image,
                    ChatName = nickname,
                    ChatMembers = chatMembers,
                    ChatType = ChatType.Group,
                    Messages = new List<ChatMessage>()
                });

                SystemChatMessage dataToSend = new() {
                    SystemMessageType = SystemChatMessageType.NewChat,
                    Chat = activeChat };

                response = Request(CommandType.NewSystemChatMessage, dataToSend);

                if(response.Type == ResponseType.Error) return false;

                NewChatsAdded();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Add group chat Error",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
        }

        #endregion
    }
}
