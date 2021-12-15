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

            Packet receivedMessage;

            while ((receivedMessage = client.Read()) != null)
            {
                switch (receivedMessage.packetType)
                {
                    case PacketType.PrivateMessage:
                        break;

                    case PacketType.ClientName:
                        break;

                    case PacketType.ChatMessage:

                        ChatMessagePacket chatPacket = (ChatMessagePacket)receivedMessage;


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
