using Client.CustomControls;
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
using System.Windows.Media.Imaging;
using System.Windows.Threading;

#pragma warning disable SYSLIB0011

namespace Client
{
    /// <summary>
    /// Base controller class
    /// </summary>
    internal class Controller
    {
        User profile = null!;
        BitmapImage avatar = null!;
        Chat activeChat = null!;
        IPEndPoint ep = null!;

        bool isLast = false;
        bool loginStatus = false;

        public ObservableCollection<UserCell> ChatList = null!;
        public ObservableCollection<MessageContainer> MessagesList = null!;
        List<Chat> chats = null!;

        public User Profile
        {
            get { return profile; }
        }

        public BitmapImage Avatar
        {
            get { return avatar; }
        }

        public bool IsLoggedIn
        {
            get { return loginStatus; }
        }

        public bool IsAnyChatSelected()
        {
            if (activeChat == null)
                return false;
            else
                return true;
        }

        public Controller()
        {
            try
            {
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
                timer.Interval = new TimeSpan(0, 0, 10);
                Task.Run(timer.Start);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Background start synchronization Error",
                        MessageBoxButton.OK);
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

                if (response.Type == ResponseType.Success)
                {
                    List<IMessage> messages = StreamTools.Deserialize<List<IMessage>>(response.Data);

                    if (messages == null)
                        return;

                    foreach (IMessage message in messages)
                    {
                        if (message.Type == MessageType.SystemChatMessage)
                        {
                            SystemChatMessage sysMsg = (message as SystemChatMessage)!;

                            if (sysMsg.SystemMessageType == SystemChatMessageType.NewChat)
                            {
                                chats.Add(sysMsg.Chat);
                                NewChatsAdded();
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

                            if (chatMsg.ChatId == activeChat.ChatId)
                                NewMessagesAdded(activeChat.ChatName);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Background update Error",
                        MessageBoxButton.OK);
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

                client.Connect(ep);

                NetworkStream netStream = client.GetStream();
                Command command = new() { Type = type, Data = data, User = profile };
                StreamTools.NetworkSend(netStream, command);

                StreamReader reader = new(netStream, Encoding.UTF8);
                Response response = StreamTools.NetworkGet(reader.BaseStream);
                netStream.Close();

                return response;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Request Error",
                            MessageBoxButton.OK);
                return new Response() { Type = ResponseType.Error };
            }
        }

        /// <summary>
        /// Builds UI elements from current chats
        /// </summary>
        public void NewChatsAdded()
        {
            try
            {
                ChatList = new();
                foreach (Chat chat in chats)
                {
                    ChatList.Add(new UserCell()
                    {
                        AvatarSource = StreamTools.ToBitmapImage(chat.Avatar),
                        Nickname = chat.ChatName,
                        LastMessage = chat.Messages.Count > 0 ? chat.Messages.Last().MessageText : "No messages"
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Chats to cells Error",
                        MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Reflects selected chat
        /// </summary>
        /// <param name="name"></param>
        /// <returns>List of ChatMessages</returns>
        public void NewMessagesAdded(string chatName)
        {
            try
            {
                if (string.IsNullOrEmpty(chatName))
                    return;

                if (activeChat != null && activeChat.ChatName == chatName)
                    return;

                activeChat = chats.FirstOrDefault(chat => chat.ChatName.Equals(chatName))!;
                isLast = false;

                if (activeChat == null)
                    return;

                GetOnScroll();

                MessagesList = new();

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
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Recieve Chat Error",
                        MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Closes server connection
        /// </summary>
        public void CloseServerConnection()
        {
            if (ep != null)
                Request(CommandType.Disconnect, null);
        }

        #endregion

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

                if (response.Type == ResponseType.Error)
                    return false;

                profile = StreamTools.Deserialize<User>(response.Data);
                avatar = StreamTools.ToBitmapImage(profile.Avatar);

                response = Request(CommandType.Sync, null);

                if (response.Type == ResponseType.Error)
                    return false;

                chats = StreamTools.Deserialize<List<Chat>>(response.Data);
                NewChatsAdded();
                StartBackgroundSync();

                loginStatus = true;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "TryLogin Error",
                            MessageBoxButton.OK);
                return false;
            }
        }

        /// <summary>
        /// Supposed to search chat
        /// </summary>
        /// <param name="text">Name of chat</param>
        public void SearchChat(string name)
        {
            ChatList = new();

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

                if (response.Type == ResponseType.Error)
                    return;

                int index = chats.FindIndex(chat => chat.ChatId == activeChat.ChatId);

                chats[index].Messages.Add(StreamTools.Deserialize<ChatMessage>(response.Data));

                MessagesList.Add(new MessageContainer() { MessageText = messageText.Trim(), AvatartImage = Avatar });
                NewMessagesAdded(activeChat.ChatName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Send Message Error",
                        MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Gets old messages on scroll
        /// </summary>
        public void GetOnScroll()
        {
            if (isLast)
                return;

            SyncChatMessages dataToSend = new()
            {
                ChatId = activeChat.ChatId,
                MessageCount = 5,
                MessageId = activeChat.Messages.Last().ChatMessageId
            };

            Response response = Request(CommandType.SyncChatMessage, dataToSend);
            var data = StreamTools.Deserialize<List<ChatMessage>>(response.Data);
            activeChat.Messages.InsertRange(0, data);

            if (data.Count < 5)
                isLast = true;
        }

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

                Request(CommandType.NewSystemChatMessage, dataToSend);
                NewChatsAdded();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Add private chat Error",
                        MessageBoxButton.OK);
                return false;
            }
        }

        /// <summary>
        /// Adds group chat
        /// </summary>
        /// <param name="nickname"></param>
        /// <param name="imagePath"></param>
        /// <param name="logins"></param>
        public bool AddGroupChat(string nickname, string imagePath, string logins)
        {
            try
            {
                if (string.IsNullOrEmpty(nickname) || string.IsNullOrEmpty(logins)) return false;

                if (logins.Contains(" "))
                    logins = logins.Replace(" ", "");

                string[] loginArray = logins.Split(',');

                List<User> members = new();
                User data;
                Response? response = null!;

                foreach (string log in loginArray)
                {
                    data = new() { Nickname = nickname };

                    response = Request(CommandType.CheckForUser, data);

                    if (response.Type == ResponseType.Success)
                        members.Add(data);
                }

                if (members.Count < 1) return false;

                List<ChatMember> chatMembers = new();
                chatMembers.Add(new ChatMember() { User = profile, ChatMemberRole = ChatMemberRole.Owner });

                foreach (User user in members)
                {
                    chatMembers.Add(new ChatMember() { User = user, ChatMemberRole = ChatMemberRole.Member });
                }

                chats.Add(activeChat = new Chat()
                {
                    Avatar = Encoding.UTF8.GetBytes(imagePath),
                    ChatName = nickname,
                    ChatMembers = chatMembers,
                    ChatType = ChatType.Group,
                    Messages = new List<ChatMessage>()
                });

                SystemChatMessage dataToSend = new() { SystemMessageType = SystemChatMessageType.NewChat, Chat = activeChat };

                Request(CommandType.NewSystemChatMessage, dataToSend);
                NewChatsAdded();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Add group chat Error",
                        MessageBoxButton.OK);
                return false;
            }
        }

    }
}
