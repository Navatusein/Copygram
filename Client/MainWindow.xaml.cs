using Client.CustomControls;
using Client.Resources.Tools;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double toSize;
        Controller ctrl = null!;

        ObservableCollection<UserCell> ChatList = null!;
        ObservableCollection<MessageContainer> MessagesList = null!;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            ChatList = new();
            MessagesList = new();

            ctrl = new(ChatList, MessagesList);

            ChatsList.ItemsSource = ChatList;
            Chat.ItemsSource = MessagesList;

            LoginLayout.Visibility = Visibility.Visible;
            BackgroundOverlayGrid.Visibility = Visibility.Visible;
            SidePanelOverlayGrid.Visibility = Visibility.Visible;
            PrivateOverlayGrid.Visibility = Visibility.Visible;
            GroupOverlayGrid.Visibility = Visibility.Visible;
            DonatePlsGrid.Visibility = Visibility.Visible;

            MainGrid.Visibility = Visibility.Collapsed;
            ChatThumbnailGrid.Visibility = Visibility.Collapsed;

            MessageSets.IsEnabled = false;

            toSize = Width / 4;
        }

        #region Focus events
        void tbSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            tbSearch.Clear();
        }

        void tbSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(tbSearch.Text))
                tbSearch.Text = "Search";
        }

        void tbMessage_GotFocus(object sender, RoutedEventArgs e)
        {
            tbMessage.Clear();
        }

        void tbMessage_LostFocus(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(tbMessage.Text))
                tbMessage.Text = "Write a message...";
        }
        #endregion

        #region Overlays functions
        void IconButton_Tap(object sender, RoutedEventArgs e)
        {
            sidePanelOverlay.Visibility = Visibility.Visible;
            sidePanelOverlay.BeginAnimation(WidthProperty, new DoubleAnimation(0, toSize, TimeSpan.FromSeconds(0.5)));

            rectOverlay.Visibility = Visibility.Visible;
            rectOverlay.BeginAnimation(HeightProperty, new DoubleAnimation(0, this.Height, TimeSpan.FromSeconds(0.5)));
        }

        void rectOverlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (sidePanelOverlay.Visibility == Visibility.Visible)
                    sidePanelOverlay.BeginAnimation(WidthProperty, new DoubleAnimation(toSize, 0, TimeSpan.FromSeconds(0.3)));

                PrivateChatOverlay.Visibility = Visibility.Collapsed;
                GroupChatOverlay.Visibility = Visibility.Collapsed;
                DonateOverlay.Visibility = Visibility.Collapsed;
                rectOverlay.Visibility = Visibility.Collapsed;
            }
        }

        void NotImplementedClick(object sender, RoutedEventArgs e)
        {
            sidePanelOverlay.Visibility = Visibility.Collapsed;

            if (rectOverlay.Visibility == Visibility.Collapsed)
                rectOverlay.Visibility = Visibility.Visible;

            DonateOverlay.Visibility = Visibility.Visible;
        }

        void PrivateOverlay_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (PrivateChatOverlay.Visibility == Visibility.Collapsed)
                rectOverlay.Visibility = Visibility.Collapsed;
            else
                PrivateChatOverlay.Clear();
        }

        void GroupOverlay_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (GroupChatOverlay.Visibility == Visibility.Collapsed)
                rectOverlay.Visibility = Visibility.Collapsed;
            else
                GroupChatOverlay.Clear();
        }

        void DonateOverlay_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DonateOverlay.Visibility == Visibility.Collapsed)
                rectOverlay.Visibility = Visibility.Collapsed;
        }

        void DonateOverlay_CloseClick(object sender, RoutedEventArgs e)
        {
            rectOverlay.Visibility = Visibility.Collapsed;
            DonateOverlay.Visibility = Visibility.Collapsed;
        }

        #endregion

        void ChatsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ChatsList.SelectedIndex != -1 || ChatsList.SelectedItem != null)
            {
                int chatId = (int)(ChatsList.SelectedItem as UserCell)!.Tag;

                if (ctrl.GetActiveChat != null && ctrl.GetActiveChat!.ChatId == chatId)
                    return;
                else
                    ctrl.isLast = false;

                ChatThumbnailGrid.Visibility = Visibility.Visible;
                MessageSets.IsEnabled = true;

                ctrl.NewMessagesAdded(chatId);
                ctrl.LoadOlderData();

                if (Chat.Items.Count > 1)
                    Chat.ScrollIntoView(Chat.Items[Chat.Items.Count - 1]);

                if (ctrl.GetActiveChat!.ChatType == ModelsLibrary.ChatType.Group)
                    TimeStamp.Text = "Members coutn: " + ctrl.GetActiveChat.ChatMembers.Count.ToString();
                else
                    TimeStamp.Text = "Last seen long time ago";

                Username.Text = ctrl.GetActiveChat!.ChatName;

            }
        }

        #region Chat add clicks

        void PrivateChatOverlay_AddClick(object sender, RoutedEventArgs e)
        {
            if (ctrl.AddPrivateChat(PrivateChatOverlay.tbWhoToAddress.Text))
                PrivateChatOverlay.Visibility = Visibility.Collapsed;

        }

        void GroupChatOverlay_AddClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(GroupChatOverlay.tbGroupName.Text))
                GroupChatOverlay.tbGroupName.Text = "HERE must be a name!";

            if (string.IsNullOrEmpty(GroupChatOverlay.tbGroupUsers.Text))
                GroupChatOverlay.tbGroupUsers.Text = "HERE must at least one user";

            if (ctrl.AddGroupChat(GroupChatOverlay.tbGroupName.Text,
                GroupChatOverlay.Image, GroupChatOverlay.tbGroupUsers.Text))
                GroupChatOverlay.Visibility = Visibility.Collapsed;
        }

        void AddPrivate_Click(object sender, RoutedEventArgs e)
        {
            PrivateChatOverlay.Visibility = Visibility.Visible;
            rectOverlay.Visibility = Visibility.Visible;
        }

        void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            GroupChatOverlay.Visibility = Visibility.Visible;
            rectOverlay.Visibility = Visibility.Visible;
        }

        #endregion

        #region Window habbits
        private void Window_Closed(object sender, EventArgs e)
        {
            if (LoginLayout.Visibility == Visibility.Collapsed &&
                RegisterOverlay.Visibility == Visibility.Collapsed && !ctrl.isTimeout)
                ctrl.CloseServerConnection();
        }

        void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                if (e.NewSize.Width > e.PreviousSize.Width)
                {
                    if (e.NewSize.Width > 900)
                    {
                        ibEmoji.Visibility = Visibility.Visible;
                        ibSearch.Visibility = Visibility.Visible;
                    }
                    else if (e.NewSize.Width > 700)
                    {
                        ibVoice.Visibility = Visibility.Visible;
                        ibInfo.Visibility = Visibility.Visible;
                    }
                    else if (e.NewSize.Width > MinWidth)
                    {
                        ibMenu.Visibility = Visibility.Visible;
                        ibInsert.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    if (e.NewSize.Width < e.PreviousSize.Width)
                    {
                        if (e.NewSize.Width < 700)
                        {
                            ibMenu.Visibility = Visibility.Collapsed;
                            ibInsert.Visibility = Visibility.Collapsed;
                        }
                        else if (e.NewSize.Width < 800)
                        {
                            ibVoice.Visibility = Visibility.Collapsed;
                            ibInfo.Visibility = Visibility.Collapsed;
                        }
                        else if (e.NewSize.Width < 900)
                        {
                            ibEmoji.Visibility = Visibility.Collapsed;
                            ibSearch.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        #endregion

        void tbSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ctrl.SearchChat(tbSearch.Text);

            if (e.Key == Key.Escape)
                ctrl.NewChatsAdded();
        }

        #region In chat

        void tbMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(tbMessage.Text))
            {
                ctrl.SendMessage(tbMessage.Text.Trim());
                ctrl.NewChatsAdded();
                tbMessage.Clear();
                
                if(Chat.Items.Count > 1)
                    Chat.ScrollIntoView(Chat.Items[Chat.Items.Count - 1]);
            }
        }

        void Chat_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ctrl.GetActiveChat != null && e.Delta > 5)
                ctrl.LoadOlderData();
        }

        #endregion

        #region Login screens
        void btLogin_Click(object sender, RoutedEventArgs e)
        {
            if (ctrl.TryLogin(LoginOverlay.tbUsername.Text, LoginOverlay.tbPassword.Password))
            {
                LoginLayout.Visibility = Visibility.Collapsed;
                MainGrid.Visibility = Visibility.Visible;

                sidePanelOverlay.MyUsernameSource = ctrl.Profile.Nickname;
                sidePanelOverlay.MyAvatarSource = StreamTools.ToBitmapImage(ctrl.Profile.Avatar);
                sidePanelOverlay.IdSource = ctrl.Profile.UserId.ToString();
            }
            else
            {
                LoginOverlay.Clear();
                LoginOverlay.SpeakLable.Text = "Wrong creditinals, dear User!";
                LoginOverlay.SpeakLable.Foreground = new SolidColorBrush(Color.FromRgb(153, 0, 0));
            }
        }

        void LoginOverlay_RegisterClick(object sender, RoutedEventArgs e)
        {
            RegisterOverlay.Visibility = Visibility.Visible;
            LoginOverlay.Visibility = Visibility.Collapsed;
            LoginOverlay.Clear();
        }

        private void RegisterOverlay_GoBackClick(object sender, RoutedEventArgs e)
        {
            RegisterOverlay.Visibility = Visibility.Collapsed;
            LoginOverlay.Visibility = Visibility.Visible;
            RegisterOverlay.Clear();
        }

        void RegisterOverlay_RegisterClick(object sender, RoutedEventArgs e)
        {
            if (ctrl.TryRegister(RegisterOverlay.tbUsername.Text, RegisterOverlay.tbLogin.Text,
                RegisterOverlay.tbPassword.Password, RegisterOverlay.AvatarImage))
            {
                LoginLayout.Visibility = Visibility.Collapsed;
                MainGrid.Visibility = Visibility.Visible;

                sidePanelOverlay.MyUsernameSource = ctrl.Profile.Nickname;
                sidePanelOverlay.MyAvatarSource = StreamTools.ToBitmapImage(ctrl.Profile.Avatar);
                sidePanelOverlay.IdSource = ctrl.Profile.UserId.ToString();
            }
            else
            {
                RegisterOverlay.Clear();
                RegisterOverlay.SpeakLable.Text = "Wrong creditinals, dear User!";
                RegisterOverlay.SpeakLable.Foreground = new SolidColorBrush(Color.FromRgb(153, 0, 0));
            }
        }
        #endregion


    }
}
