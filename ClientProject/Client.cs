using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

        public string userName;
        public string nickName;

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
            SendMessageTCP(new LoginPacket((IPEndPoint)m_udpClient.Client.LocalEndPoint));
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

    }
}
