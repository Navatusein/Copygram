using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double toSize;

        public MainWindow()
        {
            InitializeComponent();
            toSize = Width / 4;
        }

        private void rectOverlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                sidePanel.BeginAnimation(WidthProperty, new DoubleAnimation(toSize, 0, TimeSpan.FromSeconds(0.3)));
                rectOverlay.BeginAnimation(HeightProperty, new DoubleAnimation(this.Height, 0, TimeSpan.FromSeconds(0.3)));
                ContactsOverlay.Visibility = Visibility.Collapsed;
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

            sidePanel.Visibility = Visibility.Visible;
            sidePanel.BeginAnimation(WidthProperty, new DoubleAnimation(0, toSize, TimeSpan.FromSeconds(0.5)));

            rectOverlay.Visibility = Visibility.Visible;
            rectOverlay.BeginAnimation(HeightProperty, new DoubleAnimation(0, this.Height, TimeSpan.FromSeconds(0.5)));
        }

        private void sidePanel_ContactClick(object sender, RoutedEventArgs e)
        {
            sidePanel.Visibility = Visibility.Collapsed;
            ContactsOverlay.Visibility = Visibility.Visible;
        }

        private void ContactsOverlay_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ContactsOverlay.Visibility == Visibility.Collapsed)
            {
                rectOverlay.Visibility = Visibility.Collapsed;
            }
        }
    }
}
