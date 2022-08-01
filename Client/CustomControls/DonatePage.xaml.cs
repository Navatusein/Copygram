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
        //Routed event
        public static readonly RoutedEvent CloseClickEvent = EventManager.RegisterRoutedEvent(
            "CloseClick", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(Button));

        /// <summary>
        /// Property for routed event
        /// </summary>
        public event RoutedEventHandler CloseClick
        {
            add { AddHandler(CloseClickEvent, value); }
            remove { RemoveHandler(CloseClickEvent, value); }
        }

        /// <summary>
        /// Event raising
        /// </summary>
        void RaiseCloseEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(DonatePage.CloseClickEvent);
            RaiseEvent(newEventArgs);
        }

        /// <summary>
        /// Event on which to rase
        /// </summary>
        void OnCloseClick()
        {
            RaiseCloseEvent();
        }

        public DonatePage()
        {
            InitializeComponent();
            btExit.PreviewMouseLeftButtonDown += (sender, args) => OnCloseClick();//Adding routed event
        }

        /// <summary>
        /// Makes exit button glow on hower
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IconButton_MouseEnter(object sender, MouseEventArgs e)
        {
            btExit.Background = new SolidColorBrush(Color.FromRgb(153, 0, 0));
        }

        /// <summary>
        /// Makes exit button unglow on mouse leave
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IconButton_MouseLeave(object sender, MouseEventArgs e)
        {
            btExit.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }
    }
}
