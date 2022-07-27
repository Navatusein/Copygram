using Client.CustomControls;
using ModelsLibrary;
using System;
using System.Collections.Generic;
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
        long address; //temporary
        int port; //temporary

        public List<User> users = null!;
        public List<UserCell> cells = null!;
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
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace, "Error | " + ex.Source,
                        MessageBoxButton.OK);
            }
        }

        Command BuildCommand(CommandType type, byte[] data)
        {
            return new Command() { Type = type, Data = data, User = profile };
        }

        ResponseType RecieveResponse()
        {
            using (ns = client.GetStream())
            {
                buff = new byte[client.ReceiveBufferSize];
                ns.Read(buff, 0, buff.Length);
                lastResponse = (Response)binFormat.Deserialize(ns);
            }
            return lastResponse.Type;
        }

        void Serialize(object obj)
        {
            using (ms = new MemoryStream())
            {
                binFormat.Serialize(ms, obj);
                buff = ms.ToArray();
            }
        }

        public object Deserialize(byte[] byteArray)
        {
            using (ms = new MemoryStream())
            {
                ms.Write(byteArray, 0, byteArray.Length);
                return binFormat.Deserialize(ms);
            }
        }

        void Request(CommandType type)
        {
            using (ns = client.GetStream())
            {
                binFormat.Serialize(ns, BuildCommand(type, buff));
                ns.Flush();
            }
            buff = null!;
        }

        public bool TryLogin(string username, string password)
        {

            Serialize(new LoginData() { Login = username, Password = password });

            Request(CommandType.Login);

            if (RecieveResponse() == ResponseType.Success)
            {
                using (ms = new MemoryStream())
                {
                    buff = new byte[client.ReceiveBufferSize];
                    profile = (User)Deserialize(buff);

                    avatar = (BitmapImage)Deserialize(profile.Avatar);
                    LoadData();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public void LoadData()
        {
            Request(CommandType.Sync);

            if (RecieveResponse() == ResponseType.Success)
            {
                DisplayChat();
                StartBackgroundSync();
            }
        }

        public bool SendMessage(string messageText)
        {
            buff = Encoding.UTF8.GetBytes(messageText);

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

        void DisplayChat()
        {
            try
            {
                cells = new();
                chats = new();

                using (ms = new MemoryStream())
                {
                    ms.Write(lastResponse.Data, 0, lastResponse.Data.Length);
                    chats = (List<Chat>)binFormat.Deserialize(ms);
                }

                foreach (Chat chat in chats)
                {
                    if (chat.ChatType == ChatType.Private)
                        cells.Add(new UserCell()
                        {
                            AvatarSource = (BitmapImage)Deserialize(chat.Avatar),
                            Nickname = chat.ChatMembers.FirstOrDefault(user => user.User != profile).User.Nickname,
                            LastMessage = "No message",
                            Date = DateTime.Now.ToString()
                        });
                    else
                        cells.Add(new UserCell()
                        {
                            AvatarSource = (BitmapImage)Deserialize(chat.Avatar),
                            Nickname = chat.ChatName,
                            LastMessage = "No message",
                            Date = DateTime.Now.ToString()
                        });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.Source, ex.StackTrace);
            }
        }

        public List<ChatMessage> GetChatOnUser(string nickname)
        {
            User? toSend = users.FirstOrDefault(user => user.Nickname == nickname);

            int? id = chats.FirstOrDefault(chat => chat.ChatMembers.Any(a => a.User == profile)
                                            && chat.ChatMembers.Any(a => a.User == toSend)).ChatId;
            if (id == -1 || id == null)
                return null;

            Serialize(id);
            Request(CommandType.SyncChatMessage);

            if (RecieveResponse() == ResponseType.Success)
            {
                activeChat = (Chat)Deserialize(lastResponse.Data);
                return activeChat.Messages;
            }
            else
            {
                return null;
            }
        }

        void StartBackgroundSync()
        {
            DispatcherTimer timer = new();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 15);
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            Request(CommandType.Sync);
            if (RecieveResponse() == ResponseType.Success)
            {
                DisplayChat();
            }
        }

        public void AddPrivateChat(string nickname)
        {
            if (nickname == null) return;

            User toUser = users.FirstOrDefault(user => user.Nickname.Trim().ToLower() == nickname.Trim().ToLower());

            if (toUser == null) return;

            ChatMember me = new ChatMember() { User = profile, ChatMemberRole = ChatMemberRole.Owner };
            ChatMember to = new ChatMember() { User = toUser, ChatMemberRole = ChatMemberRole.Owner };

            chats.Add(new Chat()
            {
                Avatar = toUser.Avatar,
                ChatName = toUser.Nickname,
                ChatMembers = new List<ChatMember>() { me, to },
                ChatType = ChatType.Private,
                Messages = new List<ChatMessage>()
            });

            cells.Add(new UserCell()
            {
                AvatarSource = (BitmapImage)Deserialize(toUser.Avatar),
                Nickname = toUser.Nickname,
                LastMessage = "No message",
                Date = DateTime.Now.ToString()
            });
        }

        public void AddGroupChat(string nickname, string imagePath)
        {
            if (nickname == null) return;

            ChatMember me = new ChatMember() { User = profile, ChatMemberRole = ChatMemberRole.Owner };

            chats.Add(new Chat()
            {
                Avatar = Encoding.UTF8.GetBytes(imagePath),
                ChatName = nickname,
                ChatMembers = new List<ChatMember>() { me },
                ChatType = ChatType.Group,
                Messages = new List<ChatMessage>()
            });

            cells.Add(new UserCell()
            {
                AvatarSource = (ImageSource)new ImageSourceConverter().ConvertFromString(imagePath),
                Nickname = nickname,
                LastMessage = "No message",
                Date = DateTime.Now.ToString()
            });

        }

    }
}
