using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Client.CustomControls
{
    /// <summary>
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : UserControl
    {
        public static readonly RoutedEvent RegisterClickEvent = EventManager.RegisterRoutedEvent(
            "RegisterClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RegisterPage));

        public event RoutedEventHandler RegisterClick
        {
            add { AddHandler(RegisterClickEvent, value); }
            remove { RemoveHandler(RegisterClickEvent, value); }
        }

        void RaiseRegisterClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(RegisterPage.RegisterClickEvent);
            RaiseEvent(newEventArgs);
        }

        void OnRegisterClick()
        {
            RaiseRegisterClickEvent();
        }

        public byte[] AvatarImage{ get; set; }

        public RegisterPage()
        {
            InitializeComponent();
            this.DataContext = this;

            btRegister.PreviewMouseLeftButtonUp += (sender, args) => OnRegisterClick();
            AvatarIcon.IconImageSource = (ImageSource)new ImageSourceConverter().ConvertFromString("../../../Resources/Icons/default_user.png")!;
            AvatarImage = File.ReadAllBytes("../../../Resources/Icons/default_user.png");
        }

        private void TextChangedEvent(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbUsername.Text) && 
                !string.IsNullOrEmpty(tbPassword.Password) &&
                !string.IsNullOrEmpty(tbLogin.Text))
            {
                btRegister.IsEnabled = true;
                btRegister.Background = new SolidColorBrush(Color.FromRgb(37, 150, 190));
            }
        }

        private void tbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbUsername.Text) &&
                !string.IsNullOrEmpty(tbPassword.Password) &&
                !string.IsNullOrEmpty(tbLogin.Text))
            {
                btRegister.IsEnabled = true;
                btRegister.Background = new SolidColorBrush(Color.FromRgb(37, 150, 190));
            }
        }

        public void Clear()
        {
            tbPassword.Clear();
            tbUsername.Clear();
            tbLogin.Clear();
        }

        private void IconButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new();
            ofd.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*";
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            if (ofd.ShowDialog() == true)
            {
                AvatarIcon.IconImageSource = (ImageSource)new ImageSourceConverter().ConvertFromString(ofd.FileName)!;
                AvatarImage = File.ReadAllBytes(ofd.FileName);
            }
        }
    }
}
