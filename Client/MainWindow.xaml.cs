using Client.CustomControls;
using Client.Resources.Tools;
using System;
using System.Collections.ObjectModel;
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

        private void rectOverlay_MouseDown(object sender, MouseButtonEventArgs e)
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

        #region Focus events
        private void tbSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            tbSearch.Clear();
        }
        private void tbSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            tbSearch.Text = "Search";
        }
        private void tbMessage_GotFocus(object sender, RoutedEventArgs e)
        {
            tbMessage.Clear();
        }
        private void tbMessage_LostFocus(object sender, RoutedEventArgs e)
        {
            tbMessage.Text = "Write a message...";
        }
        #endregion

        #region Overlays functions
        private void IconButton_Tap(object sender, RoutedEventArgs e)
        {
            sidePanelOverlay.Visibility = Visibility.Visible;
            sidePanelOverlay.BeginAnimation(WidthProperty, new DoubleAnimation(0, toSize, TimeSpan.FromSeconds(0.5)));

            rectOverlay.Visibility = Visibility.Visible;
            rectOverlay.BeginAnimation(HeightProperty, new DoubleAnimation(0, this.Height, TimeSpan.FromSeconds(0.5)));
        }

        private void NotImplementedClick(object sender, RoutedEventArgs e)
        {
            sidePanelOverlay.Visibility = Visibility.Collapsed;

            if (rectOverlay.Visibility == Visibility.Collapsed)
                rectOverlay.Visibility = Visibility.Visible;

            DonateOverlay.Visibility = Visibility.Visible;
        }

        private void PrivateOverlay_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (PrivateChatOverlay.Visibility == Visibility.Collapsed)
                rectOverlay.Visibility = Visibility.Collapsed;
            else
                PrivateChatOverlay.Clear();
        }

        private void GroupOverlay_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (GroupChatOverlay.Visibility == Visibility.Collapsed)
                rectOverlay.Visibility = Visibility.Collapsed;
            else
                GroupChatOverlay.Clear();
        }

        private void DonateOverlay_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DonateOverlay.Visibility == Visibility.Collapsed)
                rectOverlay.Visibility = Visibility.Collapsed;
        }

        private void DonateOverlay_CloseClick(object sender, RoutedEventArgs e)
        {
            rectOverlay.Visibility = Visibility.Collapsed;
            DonateOverlay.Visibility = Visibility.Collapsed;
        }

        private void AddPrivate_Click(object sender, RoutedEventArgs e)
        {
            PrivateChatOverlay.Visibility = Visibility.Visible;
            rectOverlay.Visibility = Visibility.Visible;
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            GroupChatOverlay.Visibility = Visibility.Visible;
            rectOverlay.Visibility = Visibility.Visible;
        }

        #endregion
        private void btLogin_Click(object sender, RoutedEventArgs e)
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

        private void ChatsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ChatsList.SelectedIndex != -1 || ChatsList.SelectedItem != null)
            {
                int chatId = (int)(ChatsList.SelectedItem as UserCell)!.Tag;

                if (ctrl.GetActiveChat != null && ctrl.GetActiveChat!.ChatId == chatId)
                    return;
                else
                    ctrl.IsLast = false;

                ChatThumbnailGrid.Visibility = Visibility.Visible;
                MessageSets.IsEnabled = true;

                ctrl.NewMessagesAdded(chatId);

                if (Chat.Items.Count > 1)
                    Chat.ScrollIntoView(Chat.Items[Chat.Items.Count - 1]);

                Username.Text = ctrl.GetActiveChat!.ChatName;
            }
        }

        private void tbMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(tbMessage.Text))
            {
                ctrl.SendMessage(tbMessage.Text.Trim());
                ctrl.NewChatsAdded();
                tbMessage.Clear();
            }
        }

        private void PrivateChatOverlay_AddClick(object sender, RoutedEventArgs e)
        {
            if (ctrl.AddPrivateChat(PrivateChatOverlay.tbWhoToAddress.Text))
                PrivateChatOverlay.Visibility = Visibility.Collapsed;

        }

        private void GroupChatOverlay_AddClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(GroupChatOverlay.tbGroupName.Text))
                GroupChatOverlay.tbGroupName.Text = "HERE must be a name!";

            if (string.IsNullOrEmpty(GroupChatOverlay.tbGroupUsers.Text))
                GroupChatOverlay.tbGroupUsers.Text = "HERE must at least one user";

            if (ctrl.AddGroupChat(GroupChatOverlay.tbGroupName.Text,
                GroupChatOverlay.Image, GroupChatOverlay.tbGroupUsers.Text))
                GroupChatOverlay.Visibility = Visibility.Collapsed;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(LoginLayout.Visibility == Visibility.Collapsed &&
                RegisterOverlay.Visibility == Visibility.Collapsed)
                ctrl.CloseServerConnection();
        }

        private void tbSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ctrl.SearchChat(tbSearch.Text);
        }

        private void Chat_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ctrl.GetActiveChat != null && e.Delta > 5 && !ctrl.IsLast)
                ctrl.NewMessagesAdded((int)(ChatsList.SelectedItem as UserCell)!.Tag);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
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

        private void LoginOverlay_RegisterClick(object sender, RoutedEventArgs e)
        {
            RegisterOverlay.Visibility = Visibility.Visible;
            LoginOverlay.Visibility = Visibility.Collapsed;
        }

        private void RegisterOverlay_RegisterClick(object sender, RoutedEventArgs e)
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
    }
}
