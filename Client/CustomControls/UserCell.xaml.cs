using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Client.CustomControls
{
    /// <summary>
    /// Логика взаимодействия для UserCellControl.xaml
    /// </summary>
    public partial class UserCell : UserControl
    {
        /// <summary>
        /// Attached property
        /// </summary>
        public static readonly DependencyProperty AvatarSourceProperty
            = DependencyProperty.Register(
                "AvatarSource",
                typeof(ImageSource),
                typeof(UserCell));
        /// <summary>
        /// Property for attached property
        /// </summary>
        public ImageSource AvatarSource
        {
            get => (ImageSource)GetValue(AvatarSourceProperty);
            set => SetValue(AvatarSourceProperty, value);
        }

        /// <summary>
        /// Attached property
        /// </summary>
        public static readonly DependencyProperty NicknameProperty
            = DependencyProperty.Register(
                "Nickname",
                typeof(string),
                typeof(UserCell));
        
        /// <summary>
        /// Property for attached property
        /// </summary>
        public string Nickname
        {
            get => (string)GetValue(NicknameProperty);
            set => SetValue(NicknameProperty, value);
        }

        /// <summary>
        /// Attached property
        /// </summary>
        public static readonly DependencyProperty LastMessageProperty
            = DependencyProperty.Register(
                "LastMessage",
                typeof(string),
                typeof(UserCell));
        
        /// <summary>
        /// Property for attached property
        /// </summary>
        public string LastMessage
        {
            get => (string)GetValue(LastMessageProperty);
            set => SetValue(LastMessageProperty, value);
        }

        /// <summary>
        /// Attached property
        /// </summary>
        public static readonly DependencyProperty ChatIdProperty
            = DependencyProperty.Register(
                "ChatId",
                typeof(int),
                typeof(UserCell));

        /// <summary>
        /// Property for attached property
        /// </summary>
        public int ChatId
        {
            get => (int)GetValue(ChatIdProperty);
            set => SetValue(ChatIdProperty, value);
        }

        public UserCell()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        /// <summary>
        /// Parametrized constructor
        /// </summary>
        /// <param name="name">Nickname</param>
        /// <param name="message">Last message</param>
        /// <param name="image">Avatar</param>
        public UserCell(string name, string message, BitmapImage image)
        {
            InitializeComponent();
            this.DataContext = this;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(message)) return;
            
            Nickname = name;
            LastMessage = message;
            AvatarSource = image;
        }

        /// <summary>
        /// Reacts on click and disables new meessage bubble
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Bubble.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// If message is too long - shorten it with "..."
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBlock_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if(LastMessage.Length > 15)
                LastMessage = LastMessage.Substring(0, 10) + "...";
        }
    }
}
