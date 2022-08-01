using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for SignUp.xaml
    /// </summary>
    public partial class SignUp : Window
    {
        public SignUp()
        {
            InitializeComponent();
        }
        private void selectImageAction(object sender, RoutedEventArgs e)
        {
            string imagePath;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                imagePath = File.ReadAllText(openFileDialog.FileName);
                profileImage.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));
            }
        }

        private void signupAction(object sender, RoutedEventArgs e)
        {
            try
            {
                // Send data
            }
            catch (Exception ex)
            {
                // Create window with error
                Console.WriteLine(ex.Message);
            }
            /*
            if (usernameTb.Text.ToString() != String.Empty && passwordTb.Text.ToString() != String.Empty)
            {
                // TODO
                // Sign up process. Send request to server.
            } else
            {

            }
            */
        }

        // Delete
        private void passwordAction(object sender, TextChangedEventArgs e)
        {
            
        }

        private void usernameAction(object sender, TextChangedEventArgs e)
        {
           
        }
    }
}
