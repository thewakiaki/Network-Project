using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using Packets;

namespace ServerProject
{
    class Server
    {
        private TcpListener m_tcpListener;
        private UdpClient m_udpListener;
        private ConcurrentDictionary<int, ConnectedClients> m_Clients;

        private ConcurrentDictionary<int, ConnectedClients[]> m_RPSLobbies;
        private int m_NumberOfLobbies;
        private List<ConnectedClients> m_ClientForRPS;

        private ConcurrentDictionary<int, ConnectedClients[]> m_PongLobbies;
        private List<ConnectedClients> m_ClientForPong;

        public Server(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            m_tcpListener = new TcpListener(ip, port);

            m_udpListener = new UdpClient(port);
            Thread udpThread = new Thread(() => UDPListen());
            udpThread.Start();

            Start();

            
        }

        private void ClientMethodTCP(int index)
        {
            ConnectedClients client = m_Clients[index];

            ChatMessagePacket initialMessage = new ChatMessagePacket("Connected To Server");

            client.SendTCP(initialMessage);

            Packet receivedData;

            while ((receivedData = client.ReadTCP()) != null)
            {
                switch (receivedData.packetType)
                {
                    case PacketType.PrivateMessage:

                        PrivateMessagePacket privateChatPacket = (PrivateMessagePacket)receivedData;

                        int targetUserIndex = 0;

                        if (m_Clients.Count <= 1)
                        {
                            m_Clients[index].SendTCP(new ChatMessagePacket("No Other Users On The Server"));
                        }
                        else
                        {
                            for (int i = 0; i < m_Clients.Count; ++i)
                            {
                                if (privateChatPacket.targetClient == m_Clients[i].clientUsername || privateChatPacket.targetClient == m_Clients[i].clientNickName)
                                {
                                    targetUserIndex = i;
                                }
                                else
                                {
                                    targetUserIndex = 0;
                                    continue;
                                }
                            }

                            if (targetUserIndex == 0)
                            {
                                m_Clients[index].SendTCP(new ChatMessagePacket("User Does Not Exist On The Server"));
                            }
                            else
                            {
                                m_Clients[index].SendTCP(new ChatMessagePacket(privateChatPacket.message));
                                m_Clients[targetUserIndex].SendTCP(new ChatMessagePacket(privateChatPacket.message));
                            }
                        }                        

                        break;

                    case PacketType.ClientName:

                        ClientNamePacket namePacket = (ClientNamePacket)receivedData;

                        int nameCheck = 0;

                       
                        for (int i = 0; i < m_Clients.Count; ++i)
                        {
                            if (namePacket.username == m_Clients[i].clientUsername)
                            {
                                nameCheck = 1;
                                break;
                            }
                            else if(namePacket.nickname == m_Clients[i].clientNickName)
                            {
                                nameCheck = 2;
                                break;
                            }
                            else
                            {
                                nameCheck = 0;
                            }
                        }
                     

                        switch(nameCheck)
                        {
                            case 0:
                                client.clientUsername = namePacket.username;
                                client.clientNickName = namePacket.nickname;

                                NameCheckPacket checkedName = new NameCheckPacket(0);
                                client.SendTCP(checkedName);
                                break;

                            case 1:
                                NameCheckPacket badUsername = new NameCheckPacket(1);
                                client.SendTCP(badUsername);
                                break;

                            case 2:
                                NameCheckPacket badNickname = new NameCheckPacket(2);
                                client.SendTCP(badNickname);
                                break;
                        }
                        

                        

                        break;

                    case PacketType.ChatMessage:

                        ChatMessagePacket chatPacket = (ChatMessagePacket)receivedData;


                        for (int i = 0; i < m_Clients.Count; ++i)
                        {
                            m_Clients[i].SendTCP(new ChatMessagePacket(chatPacket.message));
                        }

                        break;
                    case PacketType.EncryptedChatMessage:

                        EncryptedChatMessagePacket eChatPacket = (EncryptedChatMessagePacket)receivedData;

                        string decryptedMessage = m_Clients[index].DecryptString(eChatPacket.message);

                        for(int i = 0; i < m_Clients.Count; ++i)
                        {
                            EncryptedChatMessagePacket eChatPackToSend = new EncryptedChatMessagePacket(m_Clients[i].EncryptString(decryptedMessage));
                            m_Clients[i].SendTCP(eChatPackToSend);
                        }

                        break;

                    case PacketType.GameChatMessage:

                        GameChatMessagePacket gameChatMessage = (GameChatMessagePacket)receivedData;

                        ChatMessagePacket message = new ChatMessagePacket(gameChatMessage.messageToSend);

                        if (m_Clients[index].isPlayingRPS)
                        {
                            m_RPSLobbies[gameChatMessage.lobbyNumber][0].SendTCP(message);
                            m_RPSLobbies[gameChatMessage.lobbyNumber][1].SendTCP(message);
                        }
                        else
                        {
                            m_PongLobbies[gameChatMessage.lobbyNumber][0].SendTCP(message);
                            m_PongLobbies[gameChatMessage.lobbyNumber][1].SendTCP(message);
                        }

                        break;
                    case PacketType.LoginPacket:

                        LoginPacket loginPacket = (LoginPacket)receivedData;
                        m_Clients[index].EndPoint = loginPacket.EndPoint;
                        m_Clients[index].SetClientKey(loginPacket.key);
                        m_Clients[index].SendTCP(new KeyPacket(m_Clients[index].publicKey));
                        break;

                    case PacketType.JoinRPSLobby:

                        AddPlayersToLobby(m_ClientForRPS, m_Clients[index]);

                        if(m_ClientForRPS.Count == 2)
                        {
                            ConnectedClients[] playersToAdd = new ConnectedClients[2];

                            playersToAdd = PrepareToStartGame(m_ClientForRPS, m_RPSLobbies);

                            Thread RPSThread = new Thread(() => RockPaperScissors(playersToAdd[0], playersToAdd[1]));
                            RPSThread.Start();
                        }
                        
                        break;

                    case PacketType.JoinedPongLobby:

                        AddPlayersToLobby(m_ClientForPong, m_Clients[index]);

                        if(m_ClientForPong.Count == 2)
                        {
                            ConnectedClients[] playersToAdd = new ConnectedClients[2];

                            playersToAdd = PrepareToStartGame(m_ClientForPong, m_PongLobbies);

                            Thread PongThread = new Thread(() => Pong(playersToAdd[0], playersToAdd[1]));
                            PongThread.Start();
                        }
                       
                        break;

                    case PacketType.RPSOption:

                        RPSOptionPacket choice = (RPSOptionPacket)receivedData;

                        m_Clients[index].choice = choice.option;

                        break;
                }
            }

            m_Clients[index].Close();
            ConnectedClients c;
            m_Clients.TryRemove(index, out c);

        }

        public void Start()
        {
            m_Clients = new ConcurrentDictionary<int, ConnectedClients>();

            m_RPSLobbies = new ConcurrentDictionary<int, ConnectedClients[]>();
            m_ClientForRPS = new List<ConnectedClients>();

            m_PongLobbies = new ConcurrentDictionary<int, ConnectedClients[]>();
            m_ClientForPong = new List<ConnectedClients>();

            int clientIndex = 0;

            m_tcpListener.Start();

            while (clientIndex <= 4)
            {

                Console.WriteLine("Listening...");

                Socket socket = m_tcpListener.AcceptSocket();

                Console.WriteLine("Connection made");

                ConnectedClients client = new ConnectedClients(socket);

                int index = clientIndex;
                clientIndex++;

                m_Clients.TryAdd(index, client);
                

                Thread thread = new Thread(() => { ClientMethodTCP(index); });
                thread.Start();
            }

        }

        public void Stop()
        {
            m_tcpListener.Stop();
        }

        private void UDPListen()
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

                while (true)
                {
                    byte[] buffer = m_udpListener.Receive(ref endPoint);

                    for(int i = 0; i < m_Clients.Count; ++i)
                    {
                        if(endPoint.ToString() == m_Clients[i].EndPoint.ToString())
                        {
                            m_udpListener.Send(buffer, buffer.Length, m_Clients[i].EndPoint);
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("Client UDP Read Method exception: " + e.Message);
            }
        }

        private void RockPaperScissors(ConnectedClients player1, ConnectedClients player2)
        {
            player1.isPlayingRPS = true;
            player2.isPlayingRPS = true;
            
            PlayingRPSPacket playingRPS = new PlayingRPSPacket(true, m_NumberOfLobbies);

            player1.SendTCP(new PlayerListPacket(player1.clientNickName, player2.clientNickName));
            player2.SendTCP(new PlayerListPacket(player1.clientNickName, player2.clientNickName));

            player1.SendTCP(playingRPS);
            player2.SendTCP(playingRPS);

            int lobbyNumber = m_NumberOfLobbies;

            m_NumberOfLobbies++;

            OpponentJoinedLobbyMessage(player1, player2);

            bool playing = true;
            
            while(playing)
            {
                if(player1.choice != "" && player2.choice != "")
                {
                    Thread.Sleep(1000);

                    player1.SendTCP(new RPSResultPacket(player1.choice, player2.choice));
                    player2.SendTCP(new RPSResultPacket(player1.choice, player2.choice));

                    switch(player1.choice)
                    {
                        case "rock":

                            if(player2.choice == "rock")
                            {
                                player1.SendTCP(new ChatMessagePacket("It's a draw"));
                                player2.SendTCP(new ChatMessagePacket("It's a draw"));
                            }
                            else if(player2.choice == "paper")
                            {
                                player1.SendTCP(new ChatMessagePacket(player2.clientNickName + " won this round"));
                                player2.SendTCP(new ChatMessagePacket(player2.clientNickName + " won this round"));

                                player2.RPSScore++;
                            }
                            else
                            {
                                player1.SendTCP(new ChatMessagePacket(player1.clientNickName + " won this round"));
                                player2.SendTCP(new ChatMessagePacket(player1.clientNickName + " won this round"));

                                player1.RPSScore++;
                            }

                            break;

                        case "paper":

                            if (player2.choice == "rock")
                            {
                                player1.SendTCP(new ChatMessagePacket(player1.clientNickName + " won this round"));
                                player2.SendTCP(new ChatMessagePacket(player1.clientNickName + " won this round"));

                                player1.RPSScore++;
                            }
                            else if (player2.choice == "paper")
                            {
                                player1.SendTCP(new ChatMessagePacket("It's a draw"));
                                player2.SendTCP(new ChatMessagePacket("It's a draw"));
                              
                            }
                            else
                            {
                                player1.SendTCP(new ChatMessagePacket(player2.clientNickName + " won this round"));
                                player2.SendTCP(new ChatMessagePacket(player2.clientNickName + " won this round"));

                                player2.RPSScore++;
                            }
                            break;

                        case "scissors":

                            if (player2.choice == "rock")
                            {
                                player1.SendTCP(new ChatMessagePacket(player2.clientNickName + " won this round"));
                                player2.SendTCP(new ChatMessagePacket(player2.clientNickName + " won this round"));

                                player2.RPSScore++;
                            }
                            else if (player2.choice == "paper")
                            {
                                player1.SendTCP(new ChatMessagePacket(player1.clientNickName + " won this round"));
                                player2.SendTCP(new ChatMessagePacket(player1.clientNickName + " won this round"));

                                player1.RPSScore++;
                            }
                            else
                            {
                                player1.SendTCP(new ChatMessagePacket("It's a draw"));
                                player2.SendTCP(new ChatMessagePacket("It's a draw"));
                            }

                            break;
                    }

                    if(player1.RPSScore == 3 || player2.RPSScore == 3)
                    {
                        playing = false;
                    }

                    player1.choice = "";
                    player2.choice = "";

                    Thread.Sleep(3000);

                    player1.SendTCP(new RPSNextRoundPacket(player1.RPSScore, player2.RPSScore));
                    player2.SendTCP(new RPSNextRoundPacket(player1.RPSScore, player2.RPSScore));

                }
            }

            if(player1.RPSScore == 3)
            {
                player1.SendTCP(new ChatMessagePacket(player1.clientNickName + " Has won"));
                player2.SendTCP(new ChatMessagePacket(player1.clientNickName + " Has won"));

                player1.SendTCP(new RPSGameEndPacket(true));
                player2.SendTCP(new RPSGameEndPacket(true));

                player1.RPSScore = 0;
                player2.RPSScore = 0;
            }
            else
            {
                player1.SendTCP(new ChatMessagePacket(player2.clientNickName + " Has won"));
                player2.SendTCP(new ChatMessagePacket(player2.clientNickName + " Has won"));

                player1.SendTCP(new RPSGameEndPacket(true));
                player2.SendTCP(new RPSGameEndPacket(true));

                player1.RPSScore = 0;
                player2.RPSScore = 0;
            }

            ConnectedClients[] lobbyPlayers = new ConnectedClients[2];

            m_RPSLobbies.TryRemove(lobbyNumber, out lobbyPlayers);

            lobbyPlayers = null;
        }

        private void Pong(ConnectedClients player1, ConnectedClients player2)
        {
            player1.isPlayingPong = true;
            player2.isPlayingPong = true;

            PlayingPongPacket playingPong = new PlayingPongPacket(true, m_NumberOfLobbies);

            player1.SendTCP(playingPong);
            player2.SendTCP(playingPong);

            int lobbyNumber = m_NumberOfLobbies;

            OpponentJoinedLobbyMessage(player1, player2);

            m_NumberOfLobbies++;
        }

        private void AddPlayersToLobby(List<ConnectedClients> playersInLobby, ConnectedClients client)
        {
            if(playersInLobby.Count < 2)
            {
                playersInLobby.Add(client);
            }
        }

        private ConnectedClients[] PrepareToStartGame(List<ConnectedClients> readyPlayers , ConcurrentDictionary<int, ConnectedClients[]> gameLobbies)
        {
            ConnectedClients[] playersToAdd = new ConnectedClients[2];

            playersToAdd[0] = readyPlayers[0];
            playersToAdd[1] = readyPlayers[1];

            gameLobbies.TryAdd(m_NumberOfLobbies, playersToAdd);
            readyPlayers.Clear();

            return playersToAdd;
        }

        private void OpponentJoinedLobbyMessage(ConnectedClients client1, ConnectedClients client2)
        {
            ChatMessagePacket player1Opponent = new ChatMessagePacket(client2.clientNickName + " has joined your game");
            ChatMessagePacket player2Opponent = new ChatMessagePacket(client1.clientNickName + " has joined your game");

            client1.SendTCP(player1Opponent);
            client2.SendTCP(player2Opponent);
        }
    }
}
