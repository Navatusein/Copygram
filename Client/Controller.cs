﻿using Client.CustomControls;
using ModelsLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Media;
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
        TcpClient client = null!;
        NetworkStream ns = null!;
        MemoryStream ms = null!;
        BinaryFormatter binFormat = null!;
        User profile = null!;
        BitmapImage avatar = null!;
        Response lastResponse = null!;
        Chat activeChat = null!;

        byte[] buff = null!;

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

        public Controller()
        {
            try
            {
                binFormat = new BinaryFormatter();
                //client = new TcpClient();
                //IPEndPoint ep = new IPEndPoint(address , port);
                //client.Connect(ep);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Controller Constructor Error" ,
                        MessageBoxButton.OK);
            }
        }
        /// <summary>
        /// Command constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data">Data to send</param>
        /// <returns></returns>
        Command BuildCommand(CommandType type, byte[] data)
        {
            return new Command() { Type = type, Data = data, User = profile };
        }

        /// <summary>
        /// send request of specific type
        /// </summary>
        /// <param name="type">Request command type</param>
        void Request(CommandType type)
        {
            try
            {
                using (ns = client.GetStream())
                {
                    binFormat.Serialize(ns, BuildCommand(type, buff));
                    ns.Flush();
                }
                buff = null!;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Request Error",
                            MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Recieves server response on Request
        /// </summary>
        /// <returns>ResponseType</returns>
        ResponseType RecieveResponse()
        {
            try
            {
                using (ns = client.GetStream())
                {
                    buff = new byte[client.ReceiveBufferSize];
                    ns.Read(buff, 0, buff.Length);
                    lastResponse = (Response)binFormat.Deserialize(ns);
                }
                return lastResponse.Type;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Recieve Response Error",
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
                using (ms = new MemoryStream())
                {
                    binFormat.Serialize(ms, obj);
                    buff = ms.ToArray();
                }
            }
            catch(Exception ex)
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
        public object Deserialize(byte[] byteArray)
        {
            try
            {
                using (ms = new MemoryStream())
                {
                    ms.Write(byteArray, 0, byteArray.Length);
                    return binFormat.Deserialize(ms);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Deserialize Error",
                        MessageBoxButton.OK);
                return null;
            }
        }

        /// <summary>
        /// Tries to login user with current credentials
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool TryLogin(string username, string password)
        {
            try
            {
                Serialize(new LoginData() { Login = username, Password = password });
                Request(CommandType.Login);

                if (RecieveResponse() == ResponseType.Success)
                {
                    buff = new byte[client.ReceiveBufferSize];
                    profile = (User)Deserialize(buff);

                    avatar = (BitmapImage)Deserialize(profile.Avatar);
                    LoadData();
                    StartBackgroundSync();
                    return true;
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
        /// Loads data from server
        /// </summary>
        public void LoadData()
        {
            try
            {
                Request(CommandType.Sync);

                if (RecieveResponse() == ResponseType.Success)
                {
                    UpdateChatLayout();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Load Data Error",
                        MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Sends messsage to srever and recieves reaction
        /// </summary>
        /// <param name="messageText"></param>
        /// <returns></returns>
        public bool SendMessage(string messageText)
        {
            try
            {
                Serialize(messageText);
                Request(CommandType.NewChatMessage);

                if (RecieveResponse() == ResponseType.Success)
                {
                    return true;//Good :D
                }
                else
                {
                    return false;//Bad :(
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Send Message Error",
                        MessageBoxButton.OK);
                return false;
            }
        }

        /// <summary>
        /// Updates current list of chats
        /// </summary>
        void UpdateChatLayout()
        {
            try
            {
                chats = (List<Chat>)Deserialize(lastResponse.Data);
                GetCells();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Update Chats Error",
                        MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Gets chats from server
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<ChatMessage> GetChat(string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                    return activeChat.Messages;

                activeChat = chats.FirstOrDefault(chat => chat.ChatName == name)!;

                Serialize(activeChat);
                Request(CommandType.SyncChatMessage);

                if (RecieveResponse() == ResponseType.Success)
                    activeChat = (Chat)Deserialize(lastResponse.Data);

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
        /// Background synchronization
        /// </summary>
        void StartBackgroundSync()
        {
            try
            {
                DispatcherTimer timer = new();
                timer.Tick += Timer_Tick;
                timer.Interval = new TimeSpan(0, 0, 15);
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
                Request(CommandType.Sync);
                if (RecieveResponse() == ResponseType.Success)
                {
                    UpdateChatLayout();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Background update Error",
                        MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Adds private chat
        /// </summary>
        /// <param name="nickname"></param>
        public void AddPrivateChat(string nickname)
        {
            try
            {
                if (nickname == null) return;

                Serialize(nickname);
                Request(CommandType.NewChatMessage);
                User? toUser = null;

                if (RecieveResponse() == ResponseType.Success)
                    toUser = (User)Deserialize(lastResponse.Data);

                if (toUser == null) return;

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

                UpdateChatLayout();
                Serialize(activeChat);
                Request(CommandType.SyncChatMessage);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Add private chat Error",
                        MessageBoxButton.OK);
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
                if (string.IsNullOrEmpty(nickname)|| string.IsNullOrEmpty(logins)) return;

                if (logins.Contains(" "))
                    logins = logins.Replace(" ", "");

                string[] loginArray = logins.Split(',');

                //Serialize(loginArray);
                //Request(CommandType.RequestChanges); --> Пока не понятно как получить если данные юзеры и добавлять ли их

                ChatMember me = new ChatMember() { User = profile, ChatMemberRole = ChatMemberRole.Owner };

                chats.Add(activeChat = new Chat()
                {
                    Avatar = Encoding.UTF8.GetBytes(imagePath),
                    ChatName = nickname,
                    ChatMembers = new List<ChatMember>() { me },
                    ChatType = ChatType.Group,
                    Messages = new List<ChatMessage>()
                });

                UpdateChatLayout();
                Serialize(activeChat);
                Request(CommandType.SyncChatMessage);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Add group chat Error",
                        MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Builds UI elements from current chats
        /// </summary>
        public void GetCells()
        {
            try
            {
                ChatList = new();
                foreach (Chat chat in chats)
                {
                    ChatList.Add(new UserCell()
                    {
                        AvatarSource = (ImageSource)Deserialize(chat.Avatar),
                        Nickname = chat.ChatName,
                        LastMessage = "No message"
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Chtas to cells Error",
                        MessageBoxButton.OK);
            }
        }
    }
}
