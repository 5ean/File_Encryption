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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileSafety
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string fileName = string.Empty;
        public MainWindow()
        {
            InitializeComponent();
            passwordbox1.PasswordChanged += (sender, args) =>
            {
                encryptBtn.IsEnabled = !String.IsNullOrEmpty(passwordbox1.Password) && File.Exists(fileName);
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                fileName = dialog.FileName;
                decryptBtn.IsEnabled = File.Exists(fileName) && dialog.SafeFileName.Contains(".cryp");
                (sender as Button).Content = dialog.SafeFileName;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var target = new FileInfo(fileName);
            if (passwordbox1.Password != passwordbox2.Password) MessageBox.Show("The passwords are not same! Please try again.","Error");
            else if (target.Exists && !string.IsNullOrEmpty(passwordbox1.Password))
            {
                prgGrid.Visibility = System.Windows.Visibility.Visible;
                mainGrid.Opacity = 0.3;
                mainGrid.IsEnabled = false;
                Task.Factory.StartNew(() => 
                {
                    try
                    {
                        foreach (var percentage in target.EncryptFile(passwordbox1.Password))
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                prgbar.Value = percentage;
                                prgText.Text = string.Format("Encrypeted {0}%", percentage);
                            }), null);
                        }
                        Dispatcher.BeginInvoke(new Action(
                            () => prgGrid.Visibility = System.Windows.Visibility.Collapsed
                            ));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("{0}/n/n{1}/{2}", "Encryption failed!", ex.Message, ex.StackTrace)
                        , "Error");
                    }
                });
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var target = new FileInfo(fileName);
            if (target.Exists && target.Name.Contains(".cryp") && !string.IsNullOrEmpty(passwordbox1.Password))
            {
                prgGrid.Visibility = System.Windows.Visibility.Visible;
                mainGrid.Opacity = 0.3;
                mainGrid.IsEnabled = false;
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        foreach (var percentage in target.DecryptFile(passwordbox1.Password))
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                prgbar.Value = percentage;
                                prgText.Text = string.Format("Encrypeted {0}%", percentage);
                            }), null);
                        }
                        Dispatcher.BeginInvoke(new Action(
                            () => prgGrid.Visibility = System.Windows.Visibility.Collapsed
                            ));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("{0}/n/n{1}/{2}", "Decryption failed!", ex.Message, ex.StackTrace)
                        , "Error");
                    }
                });
            }
        }
    }
}
