using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Client.CustomControls
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : UserControl
    {
        /// <summary>
        /// Routed event
        /// </summary>
        public static readonly RoutedEvent LoginClickEvent = EventManager.RegisterRoutedEvent(
            "LoginClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LoginPage));

        /// <summary>
        /// Property for attached event
        /// </summary>
        public event RoutedEventHandler LoginClick
        {
            add { AddHandler(LoginClickEvent, value); }
            remove { RemoveHandler(LoginClickEvent, value); }
        }

        /// <summary>
        /// On routed event
        /// </summary>
        void RaiseLoginClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(LoginPage.LoginClickEvent);
            RaiseEvent(newEventArgs);
        }

        /// <summary>
        /// Raises routed event
        /// </summary>
        void OnLoginClick()
        {
            RaiseLoginClickEvent();
        }

        /// <summary>
        /// Routed event
        /// </summary>
        public static readonly RoutedEvent RegisterClickEvent = EventManager.RegisterRoutedEvent(
            "RegisterClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LoginPage));

        /// <summary>
        /// Routed event property
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
            RoutedEventArgs newEventArgs = new RoutedEventArgs(LoginPage.RegisterClickEvent);
            RaiseEvent(newEventArgs);
        }

        /// <summary>
        /// Raises routed event
        /// </summary>
        void OnRegisterClick()
        {
            RaiseRegisterClickEvent();
        }

        public LoginPage()
        {
            InitializeComponent();
            this.DataContext = this;

            btLogin.PreviewMouseLeftButtonUp += (sender, args) => OnLoginClick();
            btRegister.PreviewMouseLeftButtonUp += (sender, args) => OnRegisterClick();
        }

        /// <summary>
        /// Checking creditinal to enable login
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextChangedEvent(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbUsername.Text) && !string.IsNullOrEmpty(tbPassword.Password))
            {
                btLogin.IsEnabled = true;
                btLogin.Background = new SolidColorBrush(Color.FromRgb(37, 150, 190));
            }
        }

        /// <summary>
        /// Checking creditinal to enable login
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbUsername.Text) && !string.IsNullOrEmpty(tbPassword.Password))
            {
                btLogin.IsEnabled = true;
                btLogin.Background = new SolidColorBrush(Color.FromRgb(37, 150, 190));
            }
        }

        /// <summary>
        /// Clears all input text
        /// </summary>
        public void Clear()
        {
            tbPassword.Clear();
            tbUsername.Clear();
        }
    }
}
