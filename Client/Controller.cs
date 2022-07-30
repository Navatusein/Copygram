using Client.CustomControls;
using ModelsLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
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
        BinaryFormatter binFormat = null!;
        User profile = null!;
        BitmapImage avatar = null!;
        Response lastResponse = null!;
        Chat activeChat = null!;
        DispatcherTimer timer = null!;
        IPEndPoint ep = null!;

        byte[] buff = null!;
        bool isLast = false;

        public ObservableCollection<UserCell> ChatList = null!;
        List<Chat> chats = null!;

        public User Profile
        {
            get { return profile; }
        }

        public BitmapImage Avatar
        {
            get { return avatar; }
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
                binFormat = new BinaryFormatter();
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
                timer = new();
                timer.Tick += Timer_Tick;
                timer.Interval = new TimeSpan(0, 0, 10);
                timer.Start();
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
                LoadData();
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
        ResponseType Request(CommandType type, object? toSerialize)
        {
            try
            {
                if(toSerialize != null)
                    Serialize(toSerialize);

                TcpClient client = new();
                client.Connect(ep);

                NetworkStream ns = client.GetStream();
                binFormat.Serialize(ns, new Command() { Type = type, Data = buff, User = profile });
                ns.Flush();
                buff = null!;

                StreamReader reader = new(ns, Encoding.UTF8);
                lastResponse = (Response)binFormat.Deserialize(reader.BaseStream);
                ns.Close();

                return lastResponse.Type;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Request Error",
                            MessageBoxButton.OK);
                return ResponseType.Error;
            }
        }

        /// <summary>
        /// Serializator
        /// </summary>
        /// <param name="obj">Data to serialize</param>
        void Serialize(object obj)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    binFormat.Serialize(ms, obj);
                    buff = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Serialize Error",
                        MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Deserializator
        /// </summary>
        /// <param name="byteArray">Data to deserialize</param>
        /// <returns></returns>
        T Deserialize<T>(byte[] data)
        {
            if (data != null)
            {
                T DeserializedObject;

                using (MemoryStream ms = new MemoryStream(data))
                {
                    DeserializedObject = (T)binFormat.Deserialize(ms);
                }

                return DeserializedObject;
            }
            else
                throw new Exception("Argument could not be deserialized");
        }

        /// <summary>
        /// Special converter for images
        /// </summary>
        /// <param name="data">image byte array</param>
        /// <returns>BitmapImage</returns>
        public static BitmapImage ToBitmapImage(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.StreamSource = ms;
                img.EndInit();

                if (img.CanFreeze)
                {
                    img.Freeze();
                }

                return img;
            }
        }
        
        /// <summary>
        /// Builds UI elements from current chats
        /// </summary>
        public void RefreshView()
        {
            try
            {
                ChatList = new();
                foreach (Chat chat in chats)
                {
                    ChatList.Add(new UserCell()
                    {
                        AvatarSource = ToBitmapImage(chat.Avatar),////////////////////////////////////////////////////////////////////////////////////////////
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
                if (Request(CommandType.Login, new LoginData() { Login = username, Password = password }) == ResponseType.Success)
                {
                    profile = Deserialize<User>(lastResponse.Data);
                    avatar = ToBitmapImage(profile.Avatar);////////////////////////////////////////////////////////////////////////////////////////////
                    if (Request(CommandType.Sync, null) == ResponseType.Success)
                    {
                        chats = Deserialize<List<Chat>>(lastResponse.Data);
                        RefreshView();
                        StartBackgroundSync();
                        return true;
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "TryLogin Error",
                            MessageBoxButton.OK);
                return false;
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
                if (Request(CommandType.NewChatMessage, new ChatMessage() { MessageText = messageText, FromUser = profile, ChatId = activeChat.ChatId }) == ResponseType.Success)
                {
                    chats[chats.FindIndex(chat => chat.ChatId == activeChat.ChatId)].Messages.Add(Deserialize<ChatMessage>(lastResponse.Data));
                    RefreshView();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Send Message Error",
                        MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Reflects selected chat
        /// </summary>
        /// <param name="name"></param>
        /// <returns>List of ChatMessages</returns>
        public List<ChatMessage> GetChat(string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentNullException("name");

                activeChat = chats.FirstOrDefault(chat => chat.ChatName.Equals(name))!;

                GetOnScroll();

                return activeChat.Messages;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Recieve Chat Error",
                        MessageBoxButton.OK);
                return activeChat.Messages;
            }
        }

        /// <summary>
        /// Gets old messages on scroll
        /// </summary>
        public void GetOnScroll()
        {
            if (!isLast)
            {
                Request(CommandType.SyncChatMessage, new SyncChatMessages() { ChatId = activeChat.ChatId, MessageCount = 5, MessageId = activeChat.Messages.Last().ChatId });
                activeChat.Messages.InsertRange(0, Deserialize<List<ChatMessage>>(lastResponse.Data));
            }

            if (Deserialize<List<ChatMessage>>(lastResponse.Data).Count < 5)
                isLast = true;
        }

        /// <summary>
        /// Loads data from server
        /// </summary>
        public void LoadData()
        {
            try
            {
                if (Request(CommandType.RequestChanges, null) == ResponseType.Success)
                {
                    List<IMessage> messages = Deserialize<List<IMessage>>(lastResponse.Data);

                    foreach (IMessage message in messages)
                    {
                        switch (message.Type)
                        {
                            case MessageType.SystemMessage:

                                SystemChatMessage sysMsg = (message as SystemChatMessage)!;

                                switch (sysMsg.SystemMessageType)
                                {
                                    case SystemChatMessageType.NewChat:
                                        chats.Add(sysMsg.Chat);
                                        break;
                                    case SystemChatMessageType.UpdateChat:
                                        int index = chats.FindIndex(chat => chat.ChatId == sysMsg.Chat.ChatId);
                                        chats[index] = sysMsg.Chat;
                                        break;
                                    default:
                                        break;
                                }

                                break;
                            case MessageType.ChatMessage:
                                ChatMessage? chat = message as ChatMessage;
                                chats.FirstOrDefault(chat => chat.ChatId == chat.ChatId)!.Messages.Add(chat!);
                                RefreshView();
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Load Data Error",
                        MessageBoxButton.OK);
            }
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

                if (Request(CommandType.CheckForUser, new User() { Nickname = nickname }) == ResponseType.Error) return false;

                User toUser = Deserialize<User>(lastResponse.Data);

                ChatMember me = new ChatMember() { User = profile, ChatMemberRole = ChatMemberRole.Owner };
                ChatMember to = new ChatMember() { User = toUser, ChatMemberRole = ChatMemberRole.Owner };

                chats.Add(activeChat = new Chat()
                {
                    Avatar = toUser.Avatar,////////////////////////////////////////////////////////////////////////////////////////////
                    ChatName = toUser.Nickname,
                    ChatMembers = new List<ChatMember>() { me, to },
                    ChatType = ChatType.Private,
                    Messages = new List<ChatMessage>()
                });

                Request(CommandType.NewSystemChatMessage, new SystemChatMessage() { SystemMessageType = SystemChatMessageType.NewChat, Chat = activeChat });
                RefreshView();
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
        public void AddGroupChat(string nickname, string imagePath, string logins)
        {
            try
            {
                if (string.IsNullOrEmpty(nickname) || string.IsNullOrEmpty(logins)) return;

                if (logins.Contains(" "))
                    logins = logins.Replace(" ", "");

                string[] loginArray = logins.Split(',');

                List<User> members = new();
                User temp;

                foreach (string log in loginArray)
                {
                    if (Request(CommandType.CheckForUser, temp = new User() { Nickname = nickname }) == ResponseType.Success)
                    {
                        members.Add(temp);
                    }
                }

                List<ChatMember> chatMembers = new();
                chatMembers.Add(new ChatMember() { User = profile, ChatMemberRole = ChatMemberRole.Owner });

                foreach (User user in members)
                {
                    chatMembers.Add(new ChatMember() { User = user, ChatMemberRole = ChatMemberRole.Member });
                }

                chats.Add(activeChat = new Chat()
                {
                    Avatar = Encoding.UTF8.GetBytes(imagePath),////////////////////////////////////////////////////////////////////////////////////////////
                    ChatName = nickname,
                    ChatMembers = chatMembers,
                    ChatType = ChatType.Group,
                    Messages = new List<ChatMessage>()
                });

                Request(CommandType.NewSystemChatMessage, new SystemChatMessage() { SystemMessageType = SystemChatMessageType.NewChat, Chat = activeChat });
                RefreshView();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Add group chat Error",
                        MessageBoxButton.OK);
            }
        }


    }
}
