using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Client.CustomControls
{
    /// <summary>
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : UserControl
    {
        /// <summary>
        /// Routed event
        /// </summary>
        public static readonly RoutedEvent RegisterClickEvent = EventManager.RegisterRoutedEvent(
            "RegisterClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RegisterPage));

        /// <summary>
        /// Property for routed event
        /// </summary>
        public event RoutedEventHandler RegisterClick
        {
            add { AddHandler(RegisterClickEvent, value); }
            remove { RemoveHandler(RegisterClickEvent, value); }
        }

        /// <summary>
        /// On routed event
        /// </summary>
        void RaiseRegisterClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(RegisterPage.RegisterClickEvent);
            RaiseEvent(newEventArgs);
        }

        /// <summary>
        /// Raises routed event
        /// </summary>
        void OnRegisterClick()
        {
            RaiseRegisterClickEvent();
        }

        /// <summary>
        /// Routed event
        /// </summary>
        public static readonly RoutedEvent GoBackClickEvent = EventManager.RegisterRoutedEvent(
            "GoBackClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RegisterPage));

        /// <summary>
        /// Property for routed event
        /// </summary>
        public event RoutedEventHandler GoBackClick
        {
            add { AddHandler(GoBackClickEvent, value); }
            remove { RemoveHandler(GoBackClickEvent, value); }
        }

        /// <summary>
        /// On routed event
        /// </summary>
        void RaiseGoBackClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(RegisterPage.GoBackClickEvent);
            RaiseEvent(newEventArgs);
        }

        /// <summary>
        /// Raises routed event
        /// </summary>
        void OnGoBackClick()
        {
            RaiseGoBackClickEvent();
        }

        /// <summary>
        /// Avatar in bytes
        /// </summary>
        public byte[] AvatarImage{ get; set; }

        public RegisterPage()
        {
            InitializeComponent();
            this.DataContext = this;

            btRegister.PreviewMouseLeftButtonUp += (sender, args) => OnRegisterClick();
            FckGoBack.PreviewMouseLeftButtonUp += (Sender, args) => OnGoBackClick();
            AvatarImage = File.ReadAllBytes("../../../Resources/Icons/default_user.png");//Default avatar
        }

        /// <summary>
        /// Checks if all text was filled
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Check if password was filled
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Clears input text
        /// </summary>
        public void Clear()
        {
            tbPassword.Clear();
            tbUsername.Clear();
            tbLogin.Clear();
        }

        /// <summary>
        /// Gets avatar from file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
