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
using System.Threading;
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

                        

                        //EncryptedChatMessagePacket eChatPacket = new EncryptedChatMessagePacket(m_client.EncryptString(chatPacket.message));
                        //m_client.SendMessageTCP(eChatPacket);

                        if (m_client.playingRPS || m_client.playingPong)
                        {
                            if (splitMessage[0] == "/e")
                            {
                                string gameMessage = "";

                                for (int i = 1; i < splitMessage.Length; ++i)
                                {
                                    gameMessage += splitMessage[i] + " ";
                                }

                                ChatMessagePacket chatPacket = new ChatMessagePacket(m_client.nickName + ": " + gameMessage);
                                m_client.SendMessageTCP(chatPacket);
                            }
                            else
                            {
                                string option = message.ToLower();

                                if (option == "rock" || option == "paper" || option == "scissors")
                                {
                                    RPSOptionPacket choice = new RPSOptionPacket(option);

                                    m_client.SendMessageTCP(choice);
                                }
                                else
                                {
                                    GameChatMessagePacket gameChatMessage = new GameChatMessagePacket((m_client.nickName + ": " + message), m_client.RPSLobbyNumber);
                                    m_client.SendMessageTCP(gameChatMessage);
                                }
                            }
                        }
                        else
                        {
                            ChatMessagePacket chatPacket = new ChatMessagePacket(m_client.nickName + ": " + message);
                            m_client.SendMessageTCP(chatPacket);
                        }
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

        private void PlayPong_Click(object sender, RoutedEventArgs e)
        {
            ChooseGame.Visibility = Visibility.Hidden;
            WaitingInLobby.Visibility = Visibility.Visible;

            JoinedPongLobbyPacket joinedLobbyPacket = new JoinedPongLobbyPacket(true);

            m_client.SendMessageTCP(joinedLobbyPacket);
        }

        private void PlayRPS_Click(object sender, RoutedEventArgs e)
        {
            ChooseGame.Visibility = Visibility.Hidden;
            WaitingInLobby.Visibility = Visibility.Visible;

            JoinedRPSLobbyPacket joinedLobbyPacket = new JoinedRPSLobbyPacket(true);

            m_client.SendMessageTCP(joinedLobbyPacket);
        }

        public void PlayingRockPaperScissors()
        {
            WaitingInLobby.Dispatcher.Invoke(() =>
            {
                WaitingInLobby.Visibility = Visibility.Hidden;
                RockPaperScissors.Visibility = Visibility.Visible;
            });
        }

        public void PlayingPongGame()
        {
            WaitingInLobby.Dispatcher.Invoke(() =>
            {
                WaitingInLobby.Visibility = Visibility.Hidden;
                Pong.Visibility = Visibility.Visible;
            });

            m_client.playingPong = true;
        }

        public void SetRPSNames(string player1, string player2)
        {
            RockPaperScissors.Dispatcher.Invoke(() =>
            {
                Player1.Text = player1;
                Player2.Text = player2;
            });
        }

        public void SetScores(int s1, int s2)
        {
            RockPaperScissors.Dispatcher.Invoke(() =>
            {
                P1Score.Text = s1.ToString();
                P2Score.Text = s2.ToString();
            });
        }

        private void BackToMenu1_Click(object sender, RoutedEventArgs e)
        {
            RockPaperScissors.Visibility = Visibility.Hidden;
            ChooseGame.Visibility = Visibility.Visible;

            P1Score.Text = "0";
            P2Score.Text = "0";

            BackToMenu1.Visibility = Visibility.Hidden;
        }

        public void ShowBackToMenu()
        {

            RockPaperScissors.Dispatcher.Invoke(() =>
            {
                BackToMenu1.Visibility = Visibility.Visible;
            });
        }

        #region Show Rock Paper Scissor Results

        public void ShowRockP1(bool show)
        {
            
            RockPaperScissors.Dispatcher.Invoke(() =>
            {
                if (show)
                {
                    P1Rock.Visibility = Visibility.Visible;
                }
                else
                {
                    P1Rock.Visibility = Visibility.Hidden;
                }
            });
            

        }

        public void ShowPaperP1(bool show)
        {
            RockPaperScissors.Dispatcher.Invoke(() =>
            {
                if (show)
                {
                    P1Paper.Visibility = Visibility.Visible;
                }
                else
                {
                    P1Paper.Visibility = Visibility.Hidden;
                }
            });
        }

        public void ShowScissorsP1(bool show)
        {
            RockPaperScissors.Dispatcher.Invoke(() =>
            {
                if (show)
                {
                    P1Scissors.Visibility = Visibility.Visible;
                }
                else
                {
                    P1Scissors.Visibility = Visibility.Hidden;
                }
            });
        }

        public void ShowRockP2(bool show)
        {
            RockPaperScissors.Dispatcher.Invoke(() =>
            {
                if (show)
                {
                    P2Rock.Visibility = Visibility.Visible;
                }
                else
                {
                    P2Rock.Visibility = Visibility.Hidden;
                }
            });
        }

        public void ShowPaperP2(bool show)
        {
            RockPaperScissors.Dispatcher.Invoke(() =>
            {
                if (show)
                {
                    P2Paper.Visibility = Visibility.Visible;
                }
                else
                {
                    P2Paper.Visibility = Visibility.Hidden;
                }
            });
        }

        public void ShowScissorsP2(bool show)
        {
            RockPaperScissors.Dispatcher.Invoke(() =>
            {
                if (show)
                {
                    P2Scissors.Visibility = Visibility.Visible;
                }
                else
                {
                    P2Scissors.Visibility = Visibility.Hidden;
                }
            });
        }

        #endregion

        public void NewRound()
        {
            RockPaperScissors.Dispatcher.Invoke(() =>
            {
                P1Rock.Visibility = Visibility.Hidden;
                P1Paper.Visibility = Visibility.Hidden;
                P1Scissors.Visibility = Visibility.Hidden;

                P2Rock.Visibility = Visibility.Hidden;
                P2Paper.Visibility = Visibility.Hidden;
                P2Scissors.Visibility = Visibility.Hidden;
            });
        }

        public void MoveRackets(int p1, int p2)
        {
            Pong.Dispatcher.Invoke(() =>
            {
                Canvas.SetBottom(Player1Racket, Canvas.GetBottom(Player1Racket) + p1);
                Canvas.SetBottom(Player2Racket, Canvas.GetBottom(Player1Racket) + p2);
            });
        }

        private void PongCanvas_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.W)
            {
                Console.WriteLine("MoveUP");
            }
        }
    }
}

