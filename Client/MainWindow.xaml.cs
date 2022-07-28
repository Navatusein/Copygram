using Client.CustomControls;
using ModelsLibrary;
using System;
using System.Collections.Generic;
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


            //UserCell cell = null;
            //int count = 1;
            //while (count != 5)
            //{
            //    cell = new("test" + count, "Hello, World!", (count > 2 ? count : 0), new BitmapImage(new Uri("../../../Resources/Icons/user.png", UriKind.Relative)));
            //    count++;
            //    ChatsList.Items.Add(cell);
            //}
        }

        private void rectOverlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                sidePanelOverlay.BeginAnimation(WidthProperty, new DoubleAnimation(toSize, 0, TimeSpan.FromSeconds(0.3)));
                PrivateChatOverlay.Visibility = Visibility.Collapsed;
                GroupChatOverlay.Visibility = Visibility.Collapsed;
                DonateOverlay.Visibility = Visibility.Collapsed;
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
            ctrl = new();
            LoginOverlay.tbUsername.Text = "test";
            LoginOverlay.tbPassword.Password = "test";

            if (LoginOverlay.tbUsername.Text == "test" && LoginOverlay.tbPassword.Password == "test")
            {
                LoginLayout.Visibility = Visibility.Collapsed;
                MainGrid.Visibility = Visibility.Visible;
                return;
            }

            if (ctrl.TryLogin(LoginOverlay.tbUsername.Text, LoginOverlay.tbPassword.Password))
            {
                LoginLayout.Visibility = Visibility.Collapsed;
                MainGrid.Visibility = Visibility.Visible;
                ChatsList.ItemsSource = ctrl.ChatList;

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

                foreach (ChatMessage message in ctrl.GetChatOnUser((ChatsList.SelectedItem as UserCell)!.Nickname))
                {
                    MessageChat.Items.Add(new MessageContainer((BitmapImage)ctrl.Deserialize(message.FromUser.Avatar), message.MessageText));
                }
            }
        }

        private void tbMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(tbMessage.Text))
            {
                if (ctrl.SendMessage(tbMessage.Text.Trim()))
                    MessageChat.Items.Add(new ChatMessage() { MessageText = tbMessage.Text.Trim(), FromUser = ctrl.Profile });
                else
                    MessageChat.Items.Add(new ChatMessage() { MessageText = "X Something went wrong! X", FromUser = ctrl.Profile });
            }
        }

        private void PrivateChatOverlay_AddClick(object sender, RoutedEventArgs e)
        {
            ctrl.AddPrivateChat(PrivateChatOverlay.tbWhoToAddress.Text);
            PrivateChatOverlay.Visibility = Visibility.Collapsed;
        }

        private void GroupChatOverlay_AddClick(object sender, RoutedEventArgs e)
        {
            ctrl.AddGroupChat(GroupChatOverlay.tbGroupName.Text, GroupChatOverlay.ImagePath);
            GroupChatOverlay.Visibility = Visibility.Collapsed;
        }
    }
}
