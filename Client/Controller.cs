using Client.CustomControls;
using Client.Resources.Tools;
using Microsoft.Extensions.Configuration;
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
        User profile = null!; //Your user profile
        Chat activeChat = null!; //Chat wich is currently open
        IPEndPoint ep = null!; //Sever endpoint

        public bool isTimeout = false;
        public bool isLast = false; //Is loaded message was last in sequence
        int limit = 12; //Amount of messages to get from server

        #region Collections
        ObservableCollection<UserCell> ChatList = null!; //GUI Collection of your chats
        ObservableCollection<MessageContainer> MessagesList = null!; //GUI Collection of messages from active chat
        List<Chat> chats = null!; //Collections of your chats
        #endregion

        #region Props

        /// <summary>
        /// Property to get you as user
        /// </summary>
        public User Profile
        {
            get { return profile; }
        }

        /// <summary>
        /// Property get open chat
        /// </summary>
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

                var configuration = new ConfigurationBuilder().AddJsonFile("Config.json").Build();

                ep = new(IPAddress.Parse(configuration["ServerIp"]), int.Parse(configuration["ServerPort"]));

                //178.151.124.250,27015
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
                timer.Tick += BackgroundSync;
                timer.Interval = new TimeSpan(0, 0, 7);
                Task.Run(timer.Start);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Background start synchronization Error",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Back ground synchronization
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundSync(object? sender, EventArgs e)
        {
            try
            {
                Response response = Request(CommandType.RequestChanges, null); //Sending request to get changes

                if (response.Type == ResponseType.Error) return;

                List<IMessage> messages = StreamTools.Deserialize<List<IMessage>>(response.Data); //Getting new message objects

                if (messages == null) return;

                foreach (IMessage message in messages)//Processing of each type of new change
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
                        Chat chat = chats.FirstOrDefault(chat => chat.ChatId == chatMsg.ChatId)!;
                        chat.Messages.Add(chatMsg);

                        if (activeChat != null && chatMsg.ChatId == activeChat.ChatId)
                            NewMessagesAdded(activeChat.ChatId);
                    }
                }

                if(messages.Count > 0) //if any change was made
                    NewChatsAdded(); //Refreshes collection of chats
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
        /// Rwfreshes collection chats
        /// </summary>
        public void NewChatsAdded()
        {
            try
            {
                ChatList.Clear();
                foreach (Chat chat in chats)
                {
                    User otherUser = chat.ChatMembers.FirstOrDefault(member => member.User.UserId != profile.UserId)!.User; //Getting other user from this chat
                    
                    ChatList.Add(new UserCell() //Adding this chat to GUI
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
        /// Refreshes selected chat messages
        /// </summary>
        /// <param name="name">Active chat id</param>
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
                    MessagesList.Add(new MessageContainer()//Adding message to GUI
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

        /// <summary>
        /// Loads old messages on scroll
        /// </summary>
        public void LoadOlderData()
        {
            try
            {
                if (activeChat == null || isLast)
                    return;

                if (activeChat.Messages.Count == 0)
                {
                    MessagesList.Clear();
                    return;
                }

                SyncChatMessages dataToSend = new()//Creating data request info
                {
                    ChatId = activeChat.ChatId,
                    MessageCount = limit,
                    MessageId = activeChat.Messages.First().ChatMessageId
                };

                Response response = Request(CommandType.SyncChatMessage, dataToSend); //Sending request
                List<ChatMessage> data = StreamTools.Deserialize<List<ChatMessage>>(response.Data);//Getting new list of messages
                activeChat.Messages.InsertRange(0, data);//Inserting them into chat

                if (data.Count < limit)//If no more messages
                    isLast = true;

                NewMessagesAdded(activeChat.ChatId);//Refreshes GUI messages
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
                
                if (!client.ConnectAsync(ep).Wait(300))//Checking if server is active
                {
                    isTimeout = true;
                    byte[] dataToSend = StreamTools.Serialize(new Error() {
                        Type = KnownErrors.UnknownError,
                        Text = "Connection timout" })!;
                    response = new() { Type = ResponseType.Error, Data = dataToSend };
                    OnError(response);
                    return response;
                }

                if(isTimeout)
                    isTimeout = false;

                NetworkStream netStream = client.GetStream();
                Command command = new() { Type = type, Data = data, User = profile };//Command to send
                StreamTools.NetworkSend(netStream, command);

                StreamReader reader = new(netStream, Encoding.UTF8);
                response = StreamTools.NetworkGet(reader.BaseStream);//Getting response
                netStream.Close();

                if (response.Type == ResponseType.Error) OnError(response);//If response is error - processing it

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
        /// Closes server connection on close
        /// </summary>
        public void CloseServerConnection()
        {
            Response response = Request(CommandType.Disconnect, null);
        }

        /// <summary>
        /// Searches chat
        /// </summary>
        /// <param name="text">Name of chat to find</param>
        public void SearchChat(string name)
        {
            try
            {
                ChatList.Clear();

                foreach (Chat chat in chats)
                {
                    if (chat.ChatName.ToLower().Contains(name.ToLower()))
                    {
                        ChatList.Add(new UserCell()//Adding chat with similar name as given
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
        /// Prcoesses errors from response
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
        /// <param name="login">Input login</param>
        /// <param name="password">Input password</param>
        /// <returns>Status of login info</returns>
        public bool TryLogin(string login, string password)
        {
            try
            {
                LoginData dataToSend = new() { Login = login, Password = password };

                Response response = Request(CommandType.Login, dataToSend);//Sending request for login

                if (response.Type == ResponseType.Error) return false;

                profile = StreamTools.Deserialize<User>(response.Data);//Getting user from response

                Sync();//Synchronizes 
                
                StartBackgroundSync();//Launches bg synchronization

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "TryLogin Error",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
        }

        /// <summary>
        /// Tries to register user with given credetinals
        /// </summary>
        /// <param name="username">Input username</param>
        /// <param name="login">Input login</param>
        /// <param name="password">Input password</param>
        /// <param name="avatarImage">Input avatar</param>
        /// <returns></returns>
        public bool TryRegister(string username, string login, string password, byte[] avatarImage)
        {
            try
            {
                User loginUser = new() { Nickname = username, Avatar = avatarImage };//Creating user localy
                LoginData dataToSend = new() { Login = login, Password = password, User = loginUser };//Creting data to get on this user

                Response response = Request(CommandType.Register, dataToSend);//Requesting this user

                if (response.Type == ResponseType.Error) return false;

                profile = StreamTools.Deserialize<User>(response.Data);//Setting user

                Sync();//Synchronizing

                StartBackgroundSync();//Starting bg sync

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
                Response response = Request(CommandType.Sync, null);//Requesting sync

                if (response.Type == ResponseType.Error) return;

                chats = StreamTools.Deserialize<List<Chat>>(response.Data);//Getting new data from response
                NewChatsAdded();//Refreshing GUI
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
        /// <param name="messageText">Text to send</param>
        public void SendMessage(string messageText)
        {
            try
            {
                ChatMessage dataToSend = new()//Data to send
                {
                    MessageText = messageText,
                    FromUser = profile,
                    ChatId = activeChat.ChatId
                };

                Response response = Request(CommandType.NewChatMessage, dataToSend);//Requesting current as new message

                if (response.Type == ResponseType.Error) return;

                int index = chats.FindIndex(chat => chat.ChatId == activeChat.ChatId);//Getting index of chat from which to send

                chats[index].Messages.Add(StreamTools.Deserialize<ChatMessage>(response.Data));//Adding sent message 

                MessagesList.Add(new MessageContainer() { 
                    MessageText = messageText.Trim(),
                    AvatartImage = StreamTools.ToBitmapImage(profile.Avatar) });//Adding sent message to GUI
                
                NewMessagesAdded(activeChat.ChatId);//Refreshes GUI
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

                Response response = Request(CommandType.CheckForUser, nickname);//Request to check user

                if (response.Type == ResponseType.Error) return false;

                User toUser = StreamTools.Deserialize<User>(response.Data);//Adding other user 

                ChatMember me = new ChatMember() { User = profile, ChatMemberRole = ChatMemberRole.Owner };//Our user as chat memeber
                ChatMember to = new ChatMember() { User = toUser, ChatMemberRole = ChatMemberRole.Owner };//Other user as chat member

                chats.Add(activeChat = new Chat()//Adding new chat
                {
                    Avatar = toUser.Avatar,
                    ChatName = toUser.Nickname,
                    ChatMembers = new List<ChatMember>() { me, to },
                    ChatType = ChatType.Private,
                    Messages = new List<ChatMessage>()
                });

                SystemChatMessage dataToSend = new() { SystemMessageType = SystemChatMessageType.NewChat, Chat = activeChat };//Data to send on new chat

                response = Request(CommandType.NewSystemChatMessage, dataToSend);//Request for new chat

                if(response.Type == ResponseType.Error) return false;

                NewChatsAdded();//Refreshing GUI
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

                foreach (string log in loginArray) //Checking user on their existance
                {
                    response = Request(CommandType.CheckForUser, log);

                    if (response.Type == ResponseType.Success)
                        members.Add(StreamTools.Deserialize<User>(response.Data));
                }

                if (members.Count < 1) return false;

                List<ChatMember> chatMembers = new();

                chatMembers.Add(new ChatMember() { //Adding chat opur uses as member
                    User = profile,
                    ChatMemberRole = ChatMemberRole.Owner });

                foreach (User user in members) //Adiing given users as members
                {
                    chatMembers.Add(new ChatMember() { User = user, ChatMemberRole = ChatMemberRole.Member });
                }

                chats.Add(activeChat = new Chat() //Adding chat to list
                {
                    Avatar = image,
                    ChatName = nickname,
                    ChatMembers = chatMembers,
                    ChatType = ChatType.Group,
                    Messages = new List<ChatMessage>()
                });

                SystemChatMessage dataToSend = new() { //Request data on creation
                    SystemMessageType = SystemChatMessageType.NewChat,
                    Chat = activeChat };

                response = Request(CommandType.NewSystemChatMessage, dataToSend);//Request on creation 

                if(response.Type == ResponseType.Error) return false;

                NewChatsAdded();//Refreshing GUI
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
