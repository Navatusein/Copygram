using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TelegramCopyPasta
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //private void RecieveMessage()
        //{
        //    try
        //    {
        //        UdpClient receiver = new UdpClient(Convert.ToInt32(tbLocalPort.Text.Trim().ToString()));
        //        //UdpClient receiver = new UdpClient();
        //        IPEndPoint ep = null;
        //        while (true)
        //        {
        //            byte[] data = receiver.Receive(ref ep);
        //            string message = Encoding.UTF8.GetString(data);
        //            this.Dispatcher.Invoke((MethodInvoker)delegate ()
        //            {
        //                tbChat.Text += message + Environment.NewLine;
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, "Recieve Message |" + ex.StackTrace);
        //    }
        //}

        //private void SendMessage()
        //{
        //    if (string.IsNullOrEmpty(tbMessage.Text.Trim().ToString()))
        //        return;

        //    try
        //    {
        //        UdpClient sender = new UdpClient();
        //        string message = tbMessage.Text.Trim().ToString();
        //        byte[] data = Encoding.UTF8.GetBytes(message);
        //        sender.Send(data, data.Length, tbAddress.Text.ToString(), Convert.ToInt32(tbRemotePort.Text));
        //        //IPEndPoint epSend = new IPEndPoint(IPAddress.Broadcast, Convert.ToInt32(tbRemotePort.Text));
        //        //sender.Send(data, data.Length, epSend);
        //        //sender.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, "Send Message |" + ex.StackTrace);
        //    }

        //}

        //private void tbMessage_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Enter)
        //    {
        //        tbChat.Text += tbMessage.Text + Environment.NewLine;
        //        SendMessage();
        //        tbMessage.Clear();
        //    }
        //}

        //private void btListen_Click(object sender, EventArgs e)
        //{
        //    Thread thread = new Thread(RecieveMessage);
        //    thread.IsBackground = true;
        //    thread.Start();

        //    btListen.BackColor = Color.GreenYellow;
        //}

        //private void btSend_Click(object sender, EventArgs e)
        //{
        //    tbChat.Text += tbMessage.Text + Environment.NewLine;
        //    SendMessage();
        //    tbMessage.Clear();
        //}
    }
}
