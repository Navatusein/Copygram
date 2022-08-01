using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Client.CustomControls
{
    /// <summary>
    /// Логика взаимодействия для ContactsOverlay.xaml
    /// </summary>
    public partial class PrivateOverlay : UserControl
    {
        string WhoToAddress
        {
            get { return tbWhoToAddress.Text; }
        }

        public PrivateOverlay()
        {
            InitializeComponent();
            this.DataContext = this;

            CreateBtn.PreviewMouseLeftButtonUp += (sender, args) => OnClick();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        public static readonly RoutedEvent UserClickEvent = EventManager.RegisterRoutedEvent(
            "AddClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PrivateOverlay));

        public event RoutedEventHandler AddClick
        {
            add { AddHandler(UserClickEvent, value); }
            remove { RemoveHandler(UserClickEvent, value); }
        }

        void RaiseClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(PrivateOverlay.UserClickEvent);
            RaiseEvent(newEventArgs);
        }

        void OnClick()
        {
            RaiseClickEvent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
        private void tbGotFocus(object sender, RoutedEventArgs e)
        {
            tbWhoToAddress.Foreground = new SolidColorBrush(Colors.Black);
            tbWhoToAddress.Clear();
        }
        private void tbLostFocus(object sender, RoutedEventArgs e)
        {
            tbWhoToAddress.Foreground = new SolidColorBrush(Colors.LightGray);
        }
        public void Clear()
        {
            tbWhoToAddress.Clear();
        }
    }
}
