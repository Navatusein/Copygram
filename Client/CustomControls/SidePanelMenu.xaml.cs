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
        public static readonly DependencyProperty MyAvatarSourceProperty
            = DependencyProperty.Register(
                "MyAvatarSource",
                typeof(ImageSource),
                typeof(SidePanelMenu));

        public ImageSource MyAvatarSource
        {
            get => (ImageSource)GetValue(MyAvatarSourceProperty);
            set => SetValue(MyAvatarSourceProperty, value);
        }

        public static readonly DependencyProperty MyUsernameProperty
            = DependencyProperty.Register(
                "MyUsernameSource",
                typeof(string),
                typeof(SidePanelMenu));

        public string MyUsernameSource
        {
            get => (string)GetValue(MyUsernameProperty);
            set => SetValue(MyUsernameProperty, value);
        }

        public static readonly DependencyProperty IdSourceProperty
            = DependencyProperty.Register(
                "IdSource",
                typeof(string),
                typeof(SidePanelMenu));

        public string IdSource
        {
            get => (string)GetValue(IdSourceProperty);
            set => SetValue(IdSourceProperty, value);
        }


        public static readonly RoutedEvent NotImplementedEvent = EventManager.RegisterRoutedEvent(
            "NotImplementedClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SidePanelMenuItem));

        public event RoutedEventHandler NotImplementedClick
        {
            add { AddHandler(NotImplementedEvent, value); }
            remove { RemoveHandler(NotImplementedEvent, value); }
        }

        void RaiseNotImplementedEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(SidePanelMenu.NotImplementedEvent);
            RaiseEvent(newEventArgs);
        }

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
