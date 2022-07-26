using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Media;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ModelsLibrary;
using System.Runtime.Serialization;

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

        byte[] buff = null!;
        long address; //temporary
        int port; //temporary

        public ObservableCollection<User> users = new();
        public ObservableCollection<Chat> chats = new();

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
                client = new TcpClient();
                IPEndPoint ep = new IPEndPoint(address , port);
                client.Connect(ep);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace, "Error | " + ex.Source ,
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

        object Deserialize(byte[] byteArray)
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
                chats = ByteToChat(lastResponse.Data);
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

        ObservableCollection<Chat> ByteToChat(byte[] bytes)
        {
            try
            {
                using (ms = new MemoryStream())
                {
                    ms.Write(bytes, 0, bytes.Length);
                    ObservableCollection<Chat> chat = (ObservableCollection<Chat>)binFormat.Deserialize(ms);
                    return chat;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.Source, ex.StackTrace);
                return null!;
            }
        }

        public Chat GetChatOnUser(string nickname)
        {
            User toSend = users.FirstOrDefault(user => user.Nickname == nickname);
            return chats.FirstOrDefault(chat => chat.ChatMembers.Any(a => a.User == profile) && chat.ChatMembers.Any(a => a.User == toSend));
        }
    }
}
