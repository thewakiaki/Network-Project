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
           
            if (Connection.IsChecked == true)
            {
                if (EnterMessage.Text == "")
                {
                    MessageBox.Show("No message to send!", "Warning");
                }
                else
                {
                    string[] splitMessage = EnterMessage.Text.Split();

                    string message = "";
                    string targetClientName;

                    if (splitMessage[0] == "/w")
                    {

                        targetClientName = splitMessage[1];
                        
                        for(int i = 2; i < splitMessage.Length; ++i)
                        {
                            if (i == splitMessage.Length - 1)
                            {
                                message += splitMessage[i];
                            }
                            else
                            {
                                message += splitMessage[i] + " ";
                            }
                        }

                        PrivateMessagePacket privateMessage = new PrivateMessagePacket(m_client.userName + ": " + message, targetClientName, m_client.userName);
                        EnterMessage.Text = "";

                        m_client.SendMessageTCP(privateMessage);
                    }
                    else
                    {
                        message = EnterMessage.Text;
                        EnterMessage.Text = "";

                        ChatMessagePacket chatPacket = new ChatMessagePacket(m_client.nickName + ": " + message);

                        //EncryptedChatMessagePacket eChatPacket = new EncryptedChatMessagePacket(m_client.EncryptString(chatPacket.message));
                        //m_client.SendMessageTCP(eChatPacket);

                        m_client.SendMessageTCP(chatPacket);
                    }
                    

                }
            }
            else
            {
                MessageBox.Show("Not connected to server", "Warning");
            }
           
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(ClientUserame.Text == "" || ClientNickname.Text == "")
            {
                MessageBox.Show("Fill in both the username and nickname then click submit", "Warning");
            }
            else
            {
                if (ClientUserame.Text.Length > 14 || NicknameText.Text.Length > 14)
                {
                    MessageBox.Show("Username and Nickname can't be longer than 14 characters");
                }
                else
                {
                    m_client.userName = ClientUserame.Text;
                    m_client.nickName = ClientNickname.Text;

                    UsernameText.Text = m_client.userName;
                    NicknameText.Text = m_client.nickName;

                    ClientNamePacket clientName = new ClientNamePacket(m_client.userName, m_client.nickName);
                    m_client.SendMessageTCP(clientName);
                }
            }           
            
        }

        public void ChangeScreen()
        {
            Login.Dispatcher.Invoke(() =>
            {
                Login.Visibility = Visibility.Hidden;
                ChatDisplayPanel.Visibility = Visibility.Visible;
            });
        }

        public void DisplayErrorMessage(string message)
        {
            Login.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, "Warning");
            });
        }
    }
}

