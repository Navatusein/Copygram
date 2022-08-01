using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Client.CustomControls
{
    /// <summary>
    /// Interaction logic for SidePanelMenuItem.xaml
    /// </summary>
    public partial class SidePanelMenuItem : UserControl
    {
        /// <summary>
        /// Attached property
        /// </summary>
        public static readonly DependencyProperty IconImageSourceProperty
            = DependencyProperty.Register(
                "IconImageSource",
                typeof(ImageSource),
                typeof(SidePanelMenuItem));

        /// <summary>
        /// Property fopr attached property
        /// </summary>
        public ImageSource IconImageSource
        {
            get => (ImageSource)GetValue(IconImageSourceProperty);
            set => SetValue(IconImageSourceProperty, value);
        }

        /// <summary>
        /// Attached property
        /// </summary>
        public static readonly DependencyProperty ItemTextSourceProperty
            = DependencyProperty.Register(
                "TextSource",
                typeof(string),
                typeof(SidePanelMenuItem));

        /// <summary>
        /// Property for attached property
        /// </summary>
        public string TextSource
        {
            get => (string)GetValue(ItemTextSourceProperty);
            set => SetValue(ItemTextSourceProperty, value);
        }

        public SidePanelMenuItem()
        {
            InitializeComponent();
            this.DataContext = this;
        }
    }
}
