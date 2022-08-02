using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Client.CustomControls
{
    /// <summary>
    /// Логика взаимодействия для SidePanelMenuControl.xaml
    /// </summary>
    public partial class SidePanelMenu : UserControl
    {
        /// <summary>
        /// Attached property
        /// </summary>
        public static readonly DependencyProperty MyAvatarSourceProperty
            = DependencyProperty.Register(
                "MyAvatarSource",
                typeof(ImageSource),
                typeof(SidePanelMenu));

        /// <summary>
        /// Property for attached property
        /// </summary>
        public ImageSource MyAvatarSource
        {
            get => (ImageSource)GetValue(MyAvatarSourceProperty);
            set => SetValue(MyAvatarSourceProperty, value);
        }

        /// <summary>
        /// Attached property
        /// </summary>
        public static readonly DependencyProperty MyUsernameProperty
            = DependencyProperty.Register(
                "MyUsernameSource",
                typeof(string),
                typeof(SidePanelMenu));

        /// <summary>
        /// Property for attached property
        /// </summary>
        public string MyUsernameSource
        {
            get => (string)GetValue(MyUsernameProperty);
            set => SetValue(MyUsernameProperty, value);
        }

        /// <summary>
        /// Attached property
        /// </summary>
        public static readonly DependencyProperty IdSourceProperty
            = DependencyProperty.Register(
                "IdSource",
                typeof(string),
                typeof(SidePanelMenu));

        /// <summary>
        /// Property for attached proprty
        /// </summary>
        public string IdSource
        {
            get => (string)GetValue(IdSourceProperty);
            set => SetValue(IdSourceProperty, value);
        }

        /// <summary>
        /// Routed event
        /// </summary>
        public static readonly RoutedEvent NotImplementedEvent = EventManager.RegisterRoutedEvent(
            "NotImplementedClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SidePanelMenuItem));

        /// <summary>
        /// Property for routed event
        /// </summary>
        public event RoutedEventHandler NotImplementedClick
        {
            add { AddHandler(NotImplementedEvent, value); }
            remove { RemoveHandler(NotImplementedEvent, value); }
        }

        /// <summary>
        /// On routed event
        /// </summary>
        void RaiseNotImplementedEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(SidePanelMenu.NotImplementedEvent);
            RaiseEvent(newEventArgs);
        }

        /// <summary>
        /// Raises routed event
        /// </summary>
        void OnNotImplementedClick()
        {
            RaiseNotImplementedEvent();
        }

        public SidePanelMenu()
        {
            InitializeComponent();
            this.DataContext = this;

            ContactsBtn.PreviewMouseLeftButtonUp += (sender, args) => OnNotImplementedClick();
            GroupBtn.PreviewMouseLeftButtonUp += (sender, args) => OnNotImplementedClick();
            ChannelBtn.PreviewMouseLeftButtonUp += (sender, args) => OnNotImplementedClick();
            SavedBtn.PreviewMouseLeftButtonUp += (sender, args) => OnNotImplementedClick();
            SettingsBtn.PreviewMouseLeftButtonUp += (sender, args) => OnNotImplementedClick();
            NightModeBtn.PreviewMouseLeftButtonUp += (sender, args) => OnNotImplementedClick();
        }
    }
}
