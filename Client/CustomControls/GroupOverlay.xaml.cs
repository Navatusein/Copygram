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
        public string GroupName { get; set; }
        public string ImagePath { get; set; }

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
            btAdd.PreviewMouseLeftButtonUp += (sender, args) => OnClick();
        }

        private void btSelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new();
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            if (ofd.ShowDialog() == true)
            {
                ImagePath = ofd.FileName;
                ImageBox.ImageSource = (ImageSource)new ImageSourceConverter().ConvertFromString(ofd.FileName);
            }
        }

        private void btAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbGroupName.Text))
            {
                tbGroupName.Text = "Enter Name!";
                return;
            }

            GroupName = tbGroupName.Text.Trim();

            if (ImagePath == null)
            { 
                //default
            }
            Visibility = Visibility.Collapsed;
        }

        private void tbGotFocus(object sender, RoutedEventArgs e)
        {
            tbGroupName.Clear();
        }
        private void tbLostFocus(object sender, RoutedEventArgs e)
        {
            tbGroupName.Text = "Your group name";
        }
    }
}
