using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Client.CustomControls
{
    /// <summary>
    /// Interaction logic for MessageContainer.xaml
    /// </summary>
    public partial class MessageContainer : UserControl
    {
        public static readonly DependencyProperty AvatarImageProperty
           = DependencyProperty.Register(
               "AvatarImage",
               typeof(ImageSource),
               typeof(MessageContainer));

        public ImageSource AvatartImage
        {
            get => (ImageSource)GetValue(AvatarImageProperty);
            set => SetValue(AvatarImageProperty, value);
        }

        public static readonly DependencyProperty MessageTextProperty
           = DependencyProperty.Register(
               "MessageText",
               typeof(string),
               typeof(MessageContainer));

        public string MessageText
        {
            get => (string)GetValue(MessageTextProperty);
            set => SetValue(MessageTextProperty, value);
        }

        public MessageContainer()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public MessageContainer(ImageSource avatar, string text)
        {
            InitializeComponent();
            this.DataContext = this;

            AvatartImage = avatar;
            CalcSize(text);
        }

        void CalcSize(string txt)
        {
            //1 symbol - 8 px width 32 px height
            int rows = txt.Length / 50;

            if (rows > 1)
            {
                foreach (string str in Split(txt))
                {
                    Message.Text += str + '\n';
                }
            }
            else
            {
                Message.Text = txt;
            }
        }

        static IEnumerable<string> Split(string str)
        {
            return Enumerable.Range(0, str.Length / 50)
                .Select(i => str.Substring(i * 50, 50));
        }
    }
}
