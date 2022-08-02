using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Client.CustomControls
{
    /// <summary>
    /// Логика взаимодействия для IconButton.xaml
    /// </summary>
    public partial class IconButton : UserControl
    {
        /// <summary>
        /// Attached property
        /// </summary>
        public static readonly DependencyProperty IconImageSourceProperty
            = DependencyProperty.Register(
                "IconImageSource",
                typeof(ImageSource),
                typeof(IconButton));

        /// <summary>
        /// Property for aattached property
        /// </summary>
        public ImageSource IconImageSource
        {
            get => (ImageSource)GetValue(IconImageSourceProperty);
            set => SetValue(IconImageSourceProperty, value);
        }

        /// <summary>
        /// Routed event
        /// </summary>
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
            "Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(IconButton));

        /// <summary>
        /// Property for attached event
        /// </summary>
        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        /// <summary>
        /// On routed event
        /// </summary>
        void RaiseClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(IconButton.ClickEvent);
            RaiseEvent(newEventArgs);
        }

        /// <summary>
        /// When routed event 
        /// </summary>
        void OnClick()
        {
            RaiseClickEvent();
        }

        /// <summary>
        /// Attached property
        /// </summary>
        public static readonly DependencyProperty DesiredHeightProperty
            = DependencyProperty.Register(
        "DesiredHeight",
        typeof(int),
        typeof(IconButton));

        /// <summary>
        /// Property
        /// </summary>
        public int DesiredHeight
        {
            get => (int)GetValue(DesiredHeightProperty);
            set => SetValue(DesiredHeightProperty, value);
        }

        /// <summary>
        /// Attached property
        /// </summary>
        public static readonly DependencyProperty DesiredWidthProperty
            = DependencyProperty.Register(
            "DesiredWidth",
            typeof(int),
            typeof(IconButton));

        /// <summary>
        /// Property for Attached property
        /// </summary>
        public int DesiredWidth
        {
            get => (int)GetValue(DesiredWidthProperty);
            set => SetValue(DesiredWidthProperty, value);
        }

        public IconButton()
        {
            InitializeComponent();
            this.DataContext = this;

            PreviewMouseLeftButtonUp += (sender, args) => OnClick();
        }
    }
}
