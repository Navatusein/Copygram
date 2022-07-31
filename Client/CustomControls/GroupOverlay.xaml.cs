using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Win32;

namespace Client.CustomControls
{
    /// <summary>
    /// Interaction logic for GroupOverlay.xaml
    /// </summary>
    public partial class GroupOverlay : UserControl
    {
        public string GroupName 
        {
            get { return tbGroupName.Text; }
        }
        public string ImagePath 
        { 
            get { return ImageBox.ImageSource.ToString(); }
        }

        public string Invites
        {
            get { return tbGroupUsers.Text; }
        }

        public static readonly RoutedEvent UserClickEvent = EventManager.RegisterRoutedEvent(
            "AddClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GroupOverlay));

        public event RoutedEventHandler AddClick
        {
            add { AddHandler(UserClickEvent, value); }
            remove { RemoveHandler(UserClickEvent, value); }
        }

        void RaiseClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(PrivateOverlay.UserClickEvent);
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
            }
        }

        private void btAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbGroupName.Text))
            {
                tbGroupName.Text = "Enter Name!";
                return;
            }

            if (string.IsNullOrEmpty(tbGroupUsers.Text))
            {
                tbGroupName.Text = "Should be at least one user";
                return;
            }

            if (ImagePath == null)
            { 
                ImageBox.ImageSource = (ImageSource)new ImageSourceConverter().ConvertFromString("../Resources/Icons/group_default.png")!;
            }
            Visibility = Visibility.Collapsed;
        }

        private void tbNameGotFocus(object sender, RoutedEventArgs e)
        {
            tbGroupName.Clear();
        }
        private void tbNameLostFocus(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(tbGroupName.Text))
                tbGroupName.Text = "Your group name";
        }

        private void tbInvitesGotFocus(object sender, RoutedEventArgs e)
        {
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
