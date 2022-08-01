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
        /// <summary>
        /// Routed event
        /// </summary>
        public static readonly RoutedEvent UserClickEvent = EventManager.RegisterRoutedEvent(
            "AddClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PrivateOverlay));

        /// <summary>
        /// Routed event property
        /// </summary>
        public event RoutedEventHandler AddClick
        {
            add { AddHandler(UserClickEvent, value); }
            remove { RemoveHandler(UserClickEvent, value); }
        }

        /// <summary>
        /// On routed event
        /// </summary>
        void RaiseClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(PrivateOverlay.UserClickEvent);
            RaiseEvent(newEventArgs);
        }

        /// <summary>
        /// Raises routed event
        /// </summary>
        void OnClick()
        {
            RaiseClickEvent();
        }

        public PrivateOverlay()
        {
            InitializeComponent();
            this.DataContext = this;

            CreateBtn.PreviewMouseLeftButtonUp += (sender, args) => OnClick();
        }

        /// <summary>
        /// Collapse on finish
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// Clear on focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbGotFocus(object sender, RoutedEventArgs e)
        {
            tbWhoToAddress.Foreground = new SolidColorBrush(Colors.Black);
            tbWhoToAddress.Clear();
        }
        /// <summary>
        /// Return default text on lost focus if any wasnt there
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbLostFocus(object sender, RoutedEventArgs e)
        {
            tbWhoToAddress.Foreground = new SolidColorBrush(Colors.LightGray);
        }

        /// <summary>
        /// Clears input text
        /// </summary>
        public void Clear()
        {
            tbWhoToAddress.Clear();
        }
    }
}
