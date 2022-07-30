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

        public static readonly DependencyProperty NicknameProperty
            = DependencyProperty.Register(
                "Nickname",
                typeof(string),
                typeof(UserCell));

        public static readonly DependencyProperty LastMessageProperty
            = DependencyProperty.Register(
                "LastMessage",
                typeof(string),
                typeof(UserCell));

        //public static readonly DependencyProperty DateProperty
        //    = DependencyProperty.Register(
        //        "Date",
        //        typeof(string),
        //        typeof(UserCell));

        public static readonly DependencyProperty UnreadMessageCountProperty
            = DependencyProperty.Register(
                "UnreadMessageCount",
                typeof(int),
                typeof(UserCell));

        public ImageSource AvatarSource
        {
            get => (ImageSource)GetValue(AvatarSourceProperty);
            set => SetValue(AvatarSourceProperty, value);
        }
        public string Nickname
        {
            get => (string)GetValue(NicknameProperty);
            set => SetValue(NicknameProperty, value);
        }
        public string LastMessage
        {
            get => (string)GetValue(LastMessageProperty);
            set => SetValue(LastMessageProperty, value);
        }
        //public string Date
        //{
        //    get => (string)GetValue(DateProperty);
        //    set => SetValue(DateProperty, value);
        //}
        public int UnreadMessageCount
        {
            get => (int)GetValue(UnreadMessageCountProperty);
            set => SetValue(UnreadMessageCountProperty, value);
        }

        public UserCell()
        {
            InitializeComponent();
            this.DataContext = this;
            if (UnreadCountBubble.Text == null )
            {
                UnreadCountBubble.Visibility = Visibility.Collapsed;
                Bubble.Visibility = Visibility.Collapsed;
            }
        }

        public UserCell(string name, string message, int count, BitmapImage image)
        {
            InitializeComponent();
            this.DataContext = this;
            if (string.IsNullOrEmpty(name) || count < 0) return;
            
            Nickname = name;
            LastMessage = message;
            UnreadMessageCount = count;
            AvatarSource = image;
        }

        private void UnreadCountBubble_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            if (Convert.ToInt32(UnreadCountBubble.Text) > 1)
            {
                UnreadCountBubble.Visibility = Visibility.Visible;
                Bubble.Visibility = Visibility.Visible;
            }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            UnreadCountBubble.Visibility = Visibility.Collapsed;
            Bubble.Visibility = Visibility.Collapsed;
        }
    }
}
