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
using Packets;

namespace ClientProject
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Client m_client;

        public MainWindow(Client client)
        {
            InitializeComponent();
            m_client = client;
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Send_Message_Button(object sender, RoutedEventArgs e)
        {
            #region Send message Version 1
            if (Connection.IsChecked == true)
            {
                if (EnterMessage.Text == "")
                {
                    MessageBox.Show("No message to send!", "Warning");
                }
                else
                {
                    string message = EnterMessage.Text;
                    EnterMessage.Text = "";

                    if (Username.Text == "")
                    {
                        MessageBox.Show("Enter a user name first!", "Warning");
                        EnterMessage.Text = message;
                    }
                    else
                    {
                        string username = Username.Text;

                        ChatMessagePacket chatPacket = new ChatMessagePacket((username + ": " + message));
                         
                        m_client.SendMessage(chatPacket);
                    }

                }
            }
            else
            {
                MessageBox.Show("Not connected to server", "Warning");
            }
            #endregion
        }

        private void Input_Message_Box(object sender, TextChangedEventArgs e)
        {

        }

        private void Chat_Display(object sender, TextChangedEventArgs e)
        {

        }

        private void Username_TextBox(object sender, TextChangedEventArgs e)
        {

        }

        private void Server_Check_Box(object sender, RoutedEventArgs e)
        {

        }

        private void Encryption_Check_Box(object sender, RoutedEventArgs e)
        {

        }

        public void UpdateChatDisplay(string message)
        {
            ChatDisplay.Dispatcher.Invoke(() =>
            {
                ChatDisplay.Text += message + Environment.NewLine;
                ChatDisplay.ScrollToEnd();
            });
        }
    }
}

