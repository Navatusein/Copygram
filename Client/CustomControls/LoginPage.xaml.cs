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

namespace Client.CustomControls
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : UserControl
    {

        public static readonly RoutedEvent LoginClickEvent = EventManager.RegisterRoutedEvent(
            "LoginClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LoginPage));

        public event RoutedEventHandler LoginClick
        {
            add { AddHandler(LoginClickEvent, value); }
            remove { RemoveHandler(LoginClickEvent, value); }
        }

        void RaiseLoginClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(LoginPage.LoginClickEvent);
            RaiseEvent(newEventArgs);
        }

        void OnLoginClick()
        {
            RaiseLoginClickEvent();
        }

        public static readonly RoutedEvent RegisterClickEvent = EventManager.RegisterRoutedEvent(
            "RegisterClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LoginPage));

        public event RoutedEventHandler RegisterClick
        {
            add { AddHandler(RegisterClickEvent, value); }
            remove { RemoveHandler(RegisterClickEvent, value); }
        }

        void RaiseRegisterClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(LoginPage.RegisterClickEvent);
            RaiseEvent(newEventArgs);
        }

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

        private void TextChangedEvent(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbUsername.Text) && !string.IsNullOrEmpty(tbPassword.Password))
            {
                btLogin.IsEnabled = true;
                btLogin.Background = new SolidColorBrush(Color.FromRgb(37, 150, 190));
            }
        }

        private void tbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbUsername.Text) && !string.IsNullOrEmpty(tbPassword.Password))
            {
                btLogin.IsEnabled = true;
                btLogin.Background = new SolidColorBrush(Color.FromRgb(37, 150, 190));
            }
        }


    }
}
