using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Client.CustomControls
{
    /// <summary>
    /// Interaction logic for DonatePage.xaml
    /// </summary>
    public partial class DonatePage : UserControl
    {
        public DonatePage()
        {
            InitializeComponent();
            btExit.PreviewMouseLeftButtonDown += (sender, args) => OnCloseClick();
        }

        private void IconButton_MouseEnter(object sender, MouseEventArgs e)
        {
            btExit.Background = new SolidColorBrush(Color.FromRgb(153, 0, 0));
        }

        private void IconButton_MouseLeave(object sender, MouseEventArgs e)
        {
            btExit.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }

        public static readonly RoutedEvent CloseClickEvent = EventManager.RegisterRoutedEvent(
            "CloseClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Button));

        public event RoutedEventHandler CloseClick
        {
            add { AddHandler(CloseClickEvent, value); }
            remove { RemoveHandler(CloseClickEvent, value); }
        }

        void RaiseCloseEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(DonatePage.CloseClickEvent);
            RaiseEvent(newEventArgs);
        }

        void OnCloseClick()
        {
            RaiseCloseEvent();
        }

    }
}
