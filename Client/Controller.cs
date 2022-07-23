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

namespace Client
{
    internal class Controller
    {
        TcpClient client = null;
        NetworkStream ns = null;
        BinaryFormatter binFromat;
        UserProfile profile;
        Response lastResponse;

        long address; //temporary
        int port; //temporary

        public ObservableCollection<IMessage> messages = new();
        public ObservableCollection<UserProfile> users = new();

        public Controller()
        {
            try
            {
                binFromat = new BinaryFormatter();
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


        public bool TryLogin(string username, string password)
        {
            byte[] buff = Encoding.UTF8.GetBytes(username + ":" + password);
            Command command = new() { Type = CommandType.Login, Data = buff, User = null};

            ns = client.GetStream();
            binFromat.Serialize(ns, command);
            ns.Flush();

            buff = new byte[client.ReceiveBufferSize];
            ns.Read(buff, 0, buff.Length);
            ns.Close();
            lastResponse = (Response)binFromat.Deserialize(ns);

            if (lastResponse.Type == ResponseType.Success)
            {
                profile = new() { Nickname = username };
                return true;
            }
            else
                return false;
        }

        public void LoadData()
        {
            byte[] buff = Encoding.UTF8.GetBytes("what?");
            Command command = new() { Type = CommandType.Sync, Data = buff, User = profile };

            ns = client.GetStream();
            binFromat.Serialize(ns, command);
            ns.Flush();

            buff = new byte[client.ReceiveBufferSize];
            ns.Read(buff, 0, buff.Length);
            ns.Close();
            lastResponse = (Response)binFromat.Deserialize(ns);
            
            if (lastResponse.Type == ResponseType.Success)
            { 
                
            }
        }
    }
}
