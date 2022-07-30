using Client.CustomControls;
using ModelsLibrary;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double toSize;
        Controller ctrl = null!;

        public MainWindow()
        {
            InitializeComponent();

            ctrl = new();

            LoginLayout.Visibility = Visibility.Visible;
            MainGrid.Visibility = Visibility.Collapsed;
            BackgroundOverlayGrid.Visibility = Visibility.Visible;
            SidePanelOverlayGrid.Visibility = Visibility.Visible;
            PrivateOverlayGrid.Visibility = Visibility.Visible;
            GroupOverlayGrid.Visibility = Visibility.Visible;
            DonatePlsGrid.Visibility = Visibility.Visible;
            ChatThumbnailGrid.IsEnabled = false;
            ChatGrid.IsEnabled = false;
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
            {
                rectOverlay.Visibility = Visibility.Collapsed;
            }
            else
            {
                PrivateChatOverlay.Clear();
            }
        }

        private void GroupOverlay_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (GroupChatOverlay.Visibility == Visibility.Collapsed)
            {
                rectOverlay.Visibility = Visibility.Collapsed;
            }
            else
            {
                GroupChatOverlay.Clear();
            }
        }

        private void DonateOverlay_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DonateOverlay.Visibility == Visibility.Collapsed)
            {
                rectOverlay.Visibility = Visibility.Collapsed;
            }
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
                ChatsList.ItemsSource = ctrl.ChatList;
                MessageChat.ItemsSource = ctrl.MessagesList;

                sidePanelOverlay.Name = ctrl.Profile.Nickname;
                sidePanelOverlay.MyAvatarSource = ctrl.Avatar;
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
                ChatThumbnailGrid.IsEnabled = true;
                ChatGrid.IsEnabled = true;

                MessageChat.ItemsSource = ctrl.MessagesList;
                Task.Run(() => ctrl.NewMessagesAdded((ChatsList.SelectedItems as UserCell)!.Nickname));
            }
        }

        private void tbMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(tbMessage.Text))
            {
                Task.Run(() => ctrl.SendMessage(tbMessage.Text.Trim()));
                tbMessage.Clear();
            }
        }

        private void PrivateChatOverlay_AddClick(object sender, RoutedEventArgs e)
        {
            if (Task.Run(() => ctrl.AddPrivateChat(PrivateChatOverlay.tbWhoToAddress.Text)).Result)
                PrivateChatOverlay.Visibility = Visibility.Collapsed;
        }

        private void GroupChatOverlay_AddClick(object sender, RoutedEventArgs e)
        {
            if(Task.Run(() => ctrl.AddGroupChat(GroupChatOverlay.tbGroupName.Text, GroupChatOverlay.ImagePath, GroupChatOverlay.Invites)).Result)
                GroupChatOverlay.Visibility = Visibility.Collapsed;
        }

        private void MessageChat_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            if(ctrl.IsAnyChatSelected())
                Task.Run(() => ctrl.GetOnScroll());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ctrl.CloseServerConnection();
        }

        private void tbSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ctrl.SearchChat(tbSearch.Text);
            }
        }
    }
}
