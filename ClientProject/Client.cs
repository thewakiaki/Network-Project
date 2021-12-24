using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using Packets;

namespace ClientProject
{
    public  class Client
    {
        private UdpClient m_udpClient;
        private TcpClient m_tcpClient;
        private NetworkStream m_stream;
        private BinaryWriter m_writer;
        private BinaryReader m_reader;
        private BinaryFormatter m_formatter;
        private MainWindow m_form;

        private RSACryptoServiceProvider m_RSAprovider;
        private RSAParameters m_publicKey;
        private RSAParameters m_privateKey;
        private RSAParameters m_serverKey;
        

        public string userName;
        public string nickName;

        public int RPSLobbyNumber;

        public bool playingRPS;

        public Client()
        {
            m_tcpClient = new TcpClient();
        }

        public bool Connect(string ipAddress, int port)
        {
            try
            {
                m_tcpClient.Connect(ipAddress, port);
                m_udpClient = new UdpClient();
                m_udpClient.Connect(ipAddress, port);
                m_stream = m_tcpClient.GetStream();
                m_formatter = new BinaryFormatter();
                m_writer = new BinaryWriter(m_stream, Encoding.UTF8);
                m_reader = new BinaryReader(m_stream, Encoding.UTF8);
                m_RSAprovider = new RSACryptoServiceProvider(1024);
                m_publicKey = m_RSAprovider.ExportParameters(false);
                m_privateKey = m_RSAprovider.ExportParameters(true);

                Thread UdpThread = new Thread(() => ProcessServerResponseUDP());
                UdpThread.Start();
                Login();
               
                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return false;
            }
        }
        public void Login()
        {
            SendMessageTCP(new LoginPacket((IPEndPoint)m_udpClient.Client.LocalEndPoint, m_publicKey));
        }

        public void Run()
        {
            m_form = new MainWindow(this);
            m_form.Connection.IsChecked = true;

            Thread thread = new Thread(() => ProcessServerResponseTCP());
            thread.Start();

            m_form.ShowDialog();

            m_tcpClient.Close();
            m_udpClient.Close();
        }

        private void ProcessServerResponseTCP()
        {
            while (m_tcpClient.Connected)
            {
                int numberOfBytes;

                if((numberOfBytes = m_reader.ReadInt32()) != -1)
                {
                    byte[] buffer = m_reader.ReadBytes(numberOfBytes);

                    MemoryStream ms = new MemoryStream(buffer);

                    Packet dataPacket = (Packet)m_formatter.Deserialize(ms);

                    switch (dataPacket.packetType)
                    {
                        case PacketType.ChatMessage:
                            ChatMessagePacket message = (ChatMessagePacket)dataPacket;
                            m_form.UpdateChatDisplay(message.message);
                            break;

                        case PacketType.PrivateMessage:
                            break;

                        case PacketType.ClientName:
                            break;
                        case PacketType.EncryptedChatMessage:
                            break;
                        case PacketType.LoginPacket:
                            LoginPacket loginInfo = (LoginPacket)dataPacket;
                            m_serverKey = loginInfo.key;
                            break;
                        case PacketType.NameCheck:
                            
                            NameCheckPacket nameCheck = (NameCheckPacket)dataPacket;

                            switch (nameCheck.type)
                            {
                                case 0:
                                    m_form.ChangeScreen();
                                    break;

                                case 1:
                                    m_form.DisplayErrorMessage("Username already exists! Choose another username!");
                                    break;

                                case 2:
                                    m_form.DisplayErrorMessage("Nickname already exists! Choose another nickname!");
                                    break;
                            }
                            break;

                        case PacketType.PlayingRPS:

                            PlayingRPSPacket playRPS = (PlayingRPSPacket)dataPacket;

                            if(playRPS.playing)
                            {
                                playingRPS = true;
                                m_form.PlayingRockPaperScissors();
                                m_form.UpdateChatDisplay(playRPS.message);
                                RPSLobbyNumber = playRPS.lobbyNo;
                            }
                            break;

                        case PacketType.RPSResult:

                            RPSResultPacket result = (RPSResultPacket)dataPacket;

                            switch(result.player1)
                            {
                                case "rock":
                                    m_form.ShowRockP1(true);
                                    m_form.ShowPaperP1(false);
                                    m_form.ShowScissorsP1(false);
                                    break;
                                case "paper":
                                    m_form.ShowRockP1(false);
                                    m_form.ShowPaperP1(true);
                                    m_form.ShowScissorsP1(false);
                                    break;
                                case "scissors":
                                    m_form.ShowRockP1(false);
                                    m_form.ShowPaperP1(false);
                                    m_form.ShowScissorsP1(true);
                                    break;
                                default:
                                    break;
                            }

                            switch (result.player2)
                            {
                                case "rock":
                                    m_form.ShowRockP2(true);
                                    m_form.ShowPaperP2(false);
                                    m_form.ShowScissorsP2(false);
                                    break;
                                case "paper":
                                    m_form.ShowRockP2(false);
                                    m_form.ShowPaperP2(true);
                                    m_form.ShowScissorsP2(false);
                                    break;
                                case "scissors":
                                    m_form.ShowRockP2(false);
                                    m_form.ShowPaperP2(false);
                                    m_form.ShowScissorsP2(true);
                                    break;
                                default:
                                    break;
                            }
                            break;

                        case PacketType.RPSNextRound:

                            RPSNextRoundPacket nextRoundPacket = (RPSNextRoundPacket)dataPacket;

                            m_form.NewRound();

                            m_form.SetScores(nextRoundPacket.p1Score, nextRoundPacket.p2Score);                            
                            break;

                        case PacketType.RPSGameEnd:

                            RPSGameEndPacket gameEndPacket = (RPSGameEndPacket)dataPacket;

                            if(gameEndPacket.gameEnded)
                            {
                                playingRPS = false;

                                m_form.ShowBackToMenu();
                            }
                            
                            break;

                        case PacketType.PlayerList:
                            PlayerListPacket players = (PlayerListPacket)dataPacket;

                            m_form.SetRPSNames(players.player1, players.player2);
                            break;
                    }
                }

            }
        }

        private void ProcessServerResponseUDP()
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

                while(true)
                {
                    byte[] buffer = m_udpClient.Receive(ref endPoint);

                    MemoryStream ms = new MemoryStream(buffer);
                    Packet dataPacket = (Packet)m_formatter.Deserialize(ms);

                    switch (dataPacket.packetType)
                    {
                        case PacketType.ChatMessage:

                            ChatMessagePacket message = (ChatMessagePacket)dataPacket;
                            m_form.UpdateChatDisplay(message.message);
                            break;

                        case PacketType.EncryptedChatMessage:

                            EncryptedChatMessagePacket receivedPacket = (EncryptedChatMessagePacket)dataPacket;
                            string decryptedMessage = DecryptString(receivedPacket.message);
                            m_form.UpdateChatDisplay(decryptedMessage);
                            break;

                        case PacketType.PrivateMessage:
                            break;

                        case PacketType.ClientName:
                            break;
                    }

                }
            }
            catch(SocketException e)
            {
                Console.WriteLine("Client UDP Read Method exception: " + e.Message);
            }

        }

        public void SendMessageTCP(Packet packet)
        {
            MemoryStream ms = new MemoryStream();
            m_formatter.Serialize(ms, packet);

            byte[] buffer = ms.GetBuffer();

            m_writer.Write(buffer.Length);
            m_writer.Write(buffer);

            m_writer.Flush();
        }

        public void SendMessageUDP(Packet packet)
        {
            MemoryStream ms = new MemoryStream();
            m_formatter.Serialize(ms, packet);

            byte[] buffer = ms.GetBuffer();

            m_udpClient.Send(buffer, buffer.Length);
        }

        private byte[] Encrypt(byte[] data)
        {
            lock (m_RSAprovider)
            {
                m_RSAprovider.ImportParameters(m_serverKey);
            }

            return m_RSAprovider.Encrypt(data, true);
        }

        private byte[] Decrypt(byte[] data)
        {
            lock(m_RSAprovider)
            {
                m_RSAprovider.ImportParameters(m_privateKey);
            }

            return m_RSAprovider.Decrypt(data, true);
        }

        internal byte[] EncryptString(string message)
        { 
            byte[] messageData = Encoding.UTF8.GetBytes(message);

            byte[] encryptedMessage = Encrypt(messageData);

            return encryptedMessage;
        }

        internal string DecryptString(byte[] message)
        {

            byte[] decryptedMessage = Decrypt(message);
            string sDecryptedMessage = Encoding.UTF8.GetString(decryptedMessage);

            return sDecryptedMessage;
        }
    }
}
