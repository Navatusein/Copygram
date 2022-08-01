using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace Client.CustomControls
{
    /// <summary>
    /// Interaction logic for GroupOverlay.xaml
    /// </summary>
    public partial class GroupOverlay : UserControl
    {
        public byte[] Image { get; set; }

        public static readonly RoutedEvent UserClickEvent = EventManager.RegisterRoutedEvent(
            "AddClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GroupOverlay));

        public event RoutedEventHandler AddClick
        {
            add { AddHandler(UserClickEvent, value); }
            remove { RemoveHandler(UserClickEvent, value); }
        }

        void RaiseClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(GroupOverlay.UserClickEvent);
            RaiseEvent(newEventArgs);
        }

        void OnClick()
        {
            RaiseClickEvent();
        }

        public GroupOverlay()
        {
            InitializeComponent();
            this.DataContext = DataContext;

            btAdd.PreviewMouseLeftButtonUp += (sender, args) => OnClick();
            Image = File.ReadAllBytes("../../../Resources/Icons/group_default.png");
        }

        private void btSelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new();
            ofd.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*";
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            if (ofd.ShowDialog() == true)
            {
                ImageBox.ImageSource = (ImageSource)new ImageSourceConverter().ConvertFromString(ofd.FileName)!;
                Image = File.ReadAllBytes(ofd.FileName);
            }
        }

        private void tbNameGotFocus(object sender, RoutedEventArgs e) 
        {
            if(string.IsNullOrEmpty(tbGroupName.Text))
                tbGroupName.Clear();
        }
        private void tbNameLostFocus(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(tbGroupName.Text))
                tbGroupName.Text = "Your group name";
        }

        private void tbInvitesGotFocus(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(tbGroupUsers.Text))
                tbGroupUsers.Clear();
        }
        private void tbInvitesLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbGroupUsers.Text))
                tbGroupUsers.Text = "Your invites";
        }

        public void Clear()
        {
            tbGroupName.Clear();
            tbGroupUsers.Clear();
            ImageBox.ImageSource = (ImageSource)new ImageSourceConverter().ConvertFromString("../../../Resources/Icons/group_default.png")!;
        }
    }
}
