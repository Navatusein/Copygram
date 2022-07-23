using Client.CustomControls;
using ModelsLibrary;
using System;
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
        Controller ctrl;

        public MainWindow()
        {
            InitializeComponent();
            LoginLayout.Visibility = Visibility.Visible;
            MainGrid.Visibility = Visibility.Collapsed;
            BackgroundOverlayGrid.Visibility = Visibility.Visible;
            SidePanelOverlayGrid.Visibility = Visibility.Visible;
            ContactsOverlayGrid.Visibility = Visibility.Visible;
            DonatePlsGrid.Visibility = Visibility.Visible;
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
                rectOverlay.BeginAnimation(HeightProperty, new DoubleAnimation(this.Height, 0, TimeSpan.FromSeconds(0.3)));
                ContactsOverlay.Visibility = Visibility.Collapsed;
                DonateOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (rectOverlay.Height > 0 || rectOverlay.Width > 0)
            {
                rectOverlay.Width = this.Width;
                rectOverlay.Height = this.Height;
            }
        }

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

        private void IconButton_Tap(object sender, RoutedEventArgs e)
        {
            sidePanelOverlay.Visibility = Visibility.Visible;
            sidePanelOverlay.BeginAnimation(WidthProperty, new DoubleAnimation(0, toSize, TimeSpan.FromSeconds(0.5)));

            rectOverlay.Visibility = Visibility.Visible;
            rectOverlay.BeginAnimation(HeightProperty, new DoubleAnimation(0, this.Height, TimeSpan.FromSeconds(0.5)));
        }

        private void sidePanel_ContactClick(object sender, RoutedEventArgs e)
        {
            sidePanelOverlay.Visibility = Visibility.Collapsed;
            ContactsOverlay.Visibility = Visibility.Visible;
        }

        private void sidePanelOverlay_NotContactClick(object sender, RoutedEventArgs e)
        {
            sidePanelOverlay.Visibility = Visibility.Collapsed;
            DonateOverlay.Visibility = Visibility.Visible;
        }

        private void ContactsOverlay_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ContactsOverlay.Visibility == Visibility.Collapsed)
            {
                rectOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void DonateOverlay_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DonateOverlay.Visibility == Visibility.Collapsed)
            {
                rectOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void tbMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(tbMessage.Text))
            {
                ctrl.SendMessage(tbMessage.Text.Trim());
                MessageChat.Items.Add( new ChatMessage() { MessageText = tbMessage.Text, FromUser = ctrl.profile, Type = MessageType.ChatMessage});
            }
        }

        private void TextChangedEvent(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbUsername.Text) && !string.IsNullOrEmpty(tbPassword.Password))
            {
                btLogin.IsEnabled = true;
                btLogin.Background = HexConverter("#2596be");
            }
        }

        private void tbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbUsername.Text) && !string.IsNullOrEmpty(tbPassword.Password))
            {
                btLogin.IsEnabled = true;
                btLogin.Background = HexConverter("#2596be");
            }
        }

        private Brush HexConverter(string hex)
        {
            BrushConverter bc = new BrushConverter();
            Brush brush = (Brush)bc.ConvertFrom(hex);
            brush.Freeze();
            return brush;
        }

        private void btLogin_Click(object sender, RoutedEventArgs e)
        {
            ctrl = null;//new();
            tbUsername.Text = "test";
            tbPassword.Password = "test";

            if (tbUsername.Text == "test" && tbPassword.Password == "test")
            {
                LoginLayout.Visibility = Visibility.Collapsed;
                MainGrid.Visibility = Visibility.Visible;
                return;
            }

            if (ctrl.TryLogin(tbUsername.Text, tbPassword.Password))
            {
                LoginLayout.Visibility = Visibility.Collapsed;
                MainGrid.Visibility = Visibility.Visible;
                ChatsList.ItemsSource = ctrl.chats;
            }
            else
            {
                tbUsername.Clear();
                tbPassword.Clear();
                SpeakLable.Text = "Wrong creditinals, dear User!";
                SpeakLable.Foreground = new SolidColorBrush(Color.FromRgb(153, 0, 0));
            }
        }

        private void DonateOverlay_CloseClick(object sender, RoutedEventArgs e)
        {
            rectOverlay.Visibility = Visibility.Collapsed;
            DonateOverlay.Visibility = Visibility.Collapsed;
        }
    }
}
