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
                            m_Clients[index].SendTCP(new ChatMessagePacket(GetReturnMessage("No Other Users On The Server")));
                        }
                        else
                        {
                            for (int i = 0; i < m_Clients.Count; ++i)
                            {
                                if (privateChatPacket.targetClient == m_Clients[i].clientUsername || privateChatPacket.targetClient == m_Clients[i].clientNickName)
                                {
                                    targetUserIndex = i;
                                    //m_Clients[i].SendTCP(new ChatMessagePacket(GetReturnMessage(privateChatPacket.message)));
                                }
                                else
                                {
                                    targetUserIndex = 0;
                                    continue;
                                }
                            }

                            if (targetUserIndex == 0)
                            {
                                m_Clients[index].SendTCP(new ChatMessagePacket(GetReturnMessage("User Does Not Exist On The Server")));
                            }
                            else
                            {
                                m_Clients[index].SendTCP(new ChatMessagePacket(GetReturnMessage(privateChatPacket.message)));
                                m_Clients[targetUserIndex].SendTCP(new ChatMessagePacket(GetReturnMessage(privateChatPacket.message)));
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
                            m_Clients[i].SendTCP(new ChatMessagePacket(GetReturnMessage(chatPacket.message)));
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
                    case PacketType.LoginPacket:

                        LoginPacket loginPacket = (LoginPacket)receivedData;
                        m_Clients[index].EndPoint = loginPacket.EndPoint;
                        m_Clients[index].SetClientKey(loginPacket.key);
                        m_Clients[index].SendTCP(new KeyPacket(m_Clients[index].publicKey));
                        break;
                }
            }

            m_Clients[index].Close();
            ConnectedClients c;
            m_Clients.TryRemove(index, out c);

        }

        private string GetReturnMessage(string code)
        {
            return code;
        }

        public void Start()
        {
            m_Clients = new ConcurrentDictionary<int, ConnectedClients>();
            

            int clientIndex = 0;

            m_tcpListener.Start();

            while (clientIndex <= 3)
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
    }
}
