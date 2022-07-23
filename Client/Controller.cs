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

namespace Client
{
    internal class Controller
    {
        TcpClient client = null;
        NetworkStream ns = null;
        BinaryFormatter binFormat = null;
        public User profile = null;
        Response lastResponse = null;

        byte[] buff;
        long address; //temporary
        int port; //temporary

        public ObservableCollection<User> users = new();
        public ObservableCollection<Chat> chats = new();

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
            buff = Encoding.UTF8.GetBytes(username + ":" + password);

            using (ns = client.GetStream())
            {
                binFormat.Serialize(ns, BuildCommand(CommandType.Login, buff));
                ns.Flush();
            }
            buff = null;

            if (RecieveResponse() == ResponseType.Success)
            {
                profile = new() { Nickname = username };    
                return true;
            }
            else
            {
                return false;
            }
        }

        public void LoadData()
        {
            buff = Encoding.UTF8.GetBytes("what?");

            using (ns = client.GetStream())
            {
                binFormat.Serialize(ns, BuildCommand(CommandType.Sync, buff));
                ns.Flush();
            }
            buff = null;

            if (RecieveResponse() == ResponseType.Success)
            {
                chats = ByteToChat(lastResponse.Data);
            }
        }

        public void SendMessage(string messageText)
        {
            buff = Encoding.UTF8.GetBytes(messageText);
            using(ns = client.GetStream())
            {
                binFormat.Serialize(ns, BuildCommand(CommandType.NewMessage, buff));
                ns.Flush();
            }
            buff = null;
            if (RecieveResponse() == ResponseType.Success)
            {
                //Good :D
            }
            else
            {
                //Bad :(
            }
        }

        private ObservableCollection<Chat> ByteToChat(byte[] bytes)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                binFormat = new BinaryFormatter();
                ms.Write(bytes, 0, bytes.Length);
                ms.Seek(0, SeekOrigin.Begin);
                ObservableCollection<Chat> chat = (ObservableCollection<Chat>)binFormat.Deserialize(ms);
                return chat;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.Source, ex.StackTrace);
                return null;
            }
        }
    }
}
