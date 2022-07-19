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

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
            "Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SidePanelMenuItem));

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        void RaiseClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(SidePanelMenuItem.TapEvent);
            RaiseEvent(newEventArgs);
        }

        void OnClick()
        {
            RaiseClickEvent();
        }


        public SidePanelMenu()
        {
            InitializeComponent();
            this.DataContext = this;

            ContactsBtn.PreviewMouseLeftButtonUp += (sender, args) => OnClick();
        }
    }
}
