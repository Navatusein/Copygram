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
        /// <summary>
        /// Attachep property
        /// </summary>
        public static readonly DependencyProperty AvatarImageProperty
           = DependencyProperty.Register(
               "AvatarImage",
               typeof(ImageSource),
               typeof(MessageContainer));

        /// <summary>
        /// Property for attached property
        /// </summary>
        public ImageSource AvatartImage
        {
            get => (ImageSource)GetValue(AvatarImageProperty);
            set => SetValue(AvatarImageProperty, value);
        }

        /// <summary>
        /// Attached property
        /// </summary>
        public static readonly DependencyProperty MessageTextProperty
           = DependencyProperty.Register(
               "MessageText",
               typeof(string),
               typeof(MessageContainer));

        /// <summary>
        /// Property for attached property
        /// </summary>
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

        /// <summary>
        /// Parametrized constructor
        /// </summary>
        /// <param name="avatar">Owner Avatar</param>
        /// <param name="text">Message</param>
        public MessageContainer(ImageSource avatar, string text)
        {
            InitializeComponent();
            this.DataContext = this;

            AvatartImage = avatar;
            CalcSize(text);
        }

        /// <summary>
        /// Supposed to be dynamic text rendering
        /// </summary>
        /// <param name="txt">String to parse for text</param>
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

        /// <summary>
        /// Splits string on array of smaller ones
        /// </summary>
        /// <param name="str">String to split</param>
        /// <returns></returns>
        static IEnumerable<string> Split(string str)
        {
            return Enumerable.Range(0, str.Length / 50)
                .Select(i => str.Substring(i * 50, 50));
        }
    }
}
