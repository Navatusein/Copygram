using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public static readonly RoutedEvent ContactClickEvent = EventManager.RegisterRoutedEvent(
            "ContactClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SidePanelMenuItem));

        public event RoutedEventHandler ContactClick
        {
            add { AddHandler(ContactClickEvent, value); }
            remove { RemoveHandler(ContactClickEvent, value); }
        }

        void RaiseContactEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(SidePanelMenu.ContactClickEvent);
            RaiseEvent(newEventArgs);
        }

        void OnContactClick()
        {
            RaiseContactEvent();
        }

        public static readonly RoutedEvent NotContactClickEvent = EventManager.RegisterRoutedEvent(
            "NotContactClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SidePanelMenuItem));

        public event RoutedEventHandler NotContactClick
        {
            add { AddHandler(NotContactClickEvent, value); }
            remove { RemoveHandler(NotContactClickEvent, value); }
        }

        void RaiseNotContactEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(SidePanelMenu.NotContactClickEvent);
            RaiseEvent(newEventArgs);
        }

        void OnNotContactClick()
        {
            RaiseNotContactEvent();
        }

        public SidePanelMenu()
        {
            InitializeComponent();
            this.DataContext = this;

            //ContactsBtn.PreviewMouseLeftButtonUp += (sender, args) => OnContactClick();
            //GroupBtn.PreviewMouseLeftButtonUp += (sender, args) => OnNotContactClick();
            //ChannelBtn.PreviewMouseLeftButtonUp += (sender, args) => OnNotContactClick();
            //SavedBtn.PreviewMouseLeftButtonUp += (sender, args) => OnNotContactClick();
            //SettingsBtn.PreviewMouseLeftButtonUp += (sender, args) => OnNotContactClick();
            //NightModeBtn.PreviewMouseLeftButtonUp += (sender, args) => OnNotContactClick();
        }
    }
}
