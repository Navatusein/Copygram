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
    /// Логика взаимодействия для UserCellControl.xaml
    /// </summary>
    public partial class UserCell : UserControl
    {
        public static readonly DependencyProperty AvatarSourceProperty
            = DependencyProperty.Register(
                "AvatarSource",
                typeof(ImageSource),
                typeof(UserCell));

        public static readonly DependencyProperty UserNameProperty
            = DependencyProperty.Register(
                "Username",
                typeof(string),
                typeof(UserCell));

        public static readonly DependencyProperty LastMessageProperty
            = DependencyProperty.Register(
                "LastMessage",
                typeof(string),
                typeof(UserCell));

        public static readonly DependencyProperty DateProperty
            = DependencyProperty.Register(
                "Date",
                typeof(string),
                typeof(UserCell));

        public static readonly DependencyProperty UnReadMessagess
            = DependencyProperty.Register(
                "UnreadCount",
                typeof(int),
                typeof(UserCell));

        public ImageSource AvatarSource
        {
            get => (ImageSource)GetValue(AvatarSourceProperty);
            set => SetValue(AvatarSourceProperty, value);
        }
        public string Username
        {
            get => (string)GetValue(UserNameProperty);
            set => SetValue(UserNameProperty, value);
        }
        public string LastMessage
        {
            get => (string)GetValue(LastMessageProperty);
            set => SetValue(LastMessageProperty, value);
        }
        public string Date
        {
            get => (string)GetValue(DateProperty);
            set => SetValue(DateProperty, value);
        }
        public int UnreadCount
        {
            get => (int)GetValue(UnReadMessagess);
            set => SetValue(UnReadMessagess, value);
        }

        public UserCell()
        {
            InitializeComponent();
            this.DataContext = this; 
        }

    }
}
