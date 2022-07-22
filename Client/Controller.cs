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

namespace Client
{
    internal class Controller
    {
        TcpClient client = null;
        NetworkStream ns = null;
        long address; //temporary
        int port; //temporary
        public ObservableCollection<IMessage> messages = new();
        public ObservableCollection<UserProfile> users = new();

        public Controller()
        {
            try
            {   
                /*
                client = new TcpClient();
                IPEndPoint ep = new IPEndPoint( ip addres , port);
                client.Connect(ep);
                ns = client.GetStream();

                int size = client.ReceiveBufferSize;
                byte[] buff = new byte[size];
                ns.Read(buff, 0, size);
                */
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace, "Error | " + ex.Source ,
                        MessageBoxButton.OK);
            }
        }

    }
}
