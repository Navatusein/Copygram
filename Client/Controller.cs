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

#pragma warning disable SYSLIB0011

namespace Client
{
    internal class Controller
    {
        TcpClient client = null!;
        NetworkStream ns = null!;
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

        public bool TryLogin(string username, string password)
        {
            //TODO buff = Encoding.UTF8.GetBytes(new LoginData() { Login = username, Password = password });

            using (ns = client.GetStream())
            {
                binFormat.Serialize(ns, BuildCommand(CommandType.Login, buff));
                ns.Flush();
            }
            buff = null!;

            if (RecieveResponse() == ResponseType.Success)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    buff = new byte[client.ReceiveBufferSize];
                    ms.Write(buff, 0, buff.Length);
                    profile = (User)binFormat.Deserialize(ms);

                    ms.Write(profile.Avatar, 0, profile.Avatar.Length);
                    avatar = (BitmapImage)binFormat.Deserialize(ms);

                    
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
            buff = Encoding.UTF8.GetBytes("Emptyness");

            using (ns = client.GetStream())
            {
                binFormat.Serialize(ns, BuildCommand(CommandType.Sync, buff));
                ns.Flush();
            }
            buff = null!;

            if (RecieveResponse() == ResponseType.Success)
            {
                chats = ByteToChat(lastResponse.Data);
            }
        }

        public bool SendMessage(string messageText)
        {
            buff = Encoding.UTF8.GetBytes(messageText);
            using(ns = client.GetStream())
            {
                binFormat.Serialize(ns, BuildCommand(CommandType.NewChatMessage, buff));
                ns.Flush();
            }
            buff = null!;

            if (RecieveResponse() == ResponseType.Success)
            {
                return true;//Good :D
            }
            else
            {
                return false;//Bad :(
            }
        }

        private ObservableCollection<Chat> ByteToChat(byte[] bytes)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
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
    }
}
