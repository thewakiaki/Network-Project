﻿using System;
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
        private ConcurrentDictionary<int, ConnectedClients> m_Clients;

        public Server(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            m_tcpListener = new TcpListener(ip, port);

            Start();
        }

        private void ClientMethod(int index)
        {
            ConnectedClients client = m_Clients[index];

            ChatMessagePacket initialMessage = new ChatMessagePacket("Connected To Server");

            client.Send(initialMessage);

            Packet receivedData;

            while ((receivedData = client.Read()) != null)
            {
                switch (receivedData.packetType)
                {
                    case PacketType.PrivateMessage:

                        PrivateMessagePacket privateChatPacket = (PrivateMessagePacket)receivedData;

                        for (int i = 0; i < m_Clients.Count; ++i)
                        {
                            if(privateChatPacket.targetClient == m_Clients[i].clientUsername || privateChatPacket.targetClient == m_Clients[i].clientNickName)
                            {
                                m_Clients[i].Send(new ChatMessagePacket(GetReturnMessage(privateChatPacket.message)));
                            }
                            else if(privateChatPacket.sendingClient == m_Clients[i].clientUsername || privateChatPacket.sendingClient == m_Clients[i].clientNickName)
                            {
                                m_Clients[i].Send(new ChatMessagePacket(GetReturnMessage(privateChatPacket.message)));
                            }
                            else
                            {
                                continue;
                            }
                        }

                        break;

                    case PacketType.ClientName:

                        ClientNamePacket namePacket = (ClientNamePacket)receivedData;
                        client.clientUsername = namePacket.username;
                        client.clientNickName = namePacket.nickname;

                        break;

                    case PacketType.ChatMessage:

                        ChatMessagePacket chatPacket = (ChatMessagePacket)receivedData;

                        
                        for (int i = 0; i < m_Clients.Count; ++i)
                        {
                            m_Clients[i].Send(new ChatMessagePacket(GetReturnMessage(chatPacket.message)));
                        }

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
                

                Thread thread = new Thread(() => { ClientMethod(index); });
                thread.Start();
            }

        }

        public void Stop()
        {
            m_tcpListener.Stop();
        }

    }
}
