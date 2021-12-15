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
        private TcpClient m_tcpClient;
        private NetworkStream m_stream;
        private BinaryWriter m_writer;
        private StreamReader m_reader;
        private BinaryFormatter m_formatter;
        private MainWindow m_form;

        public Client()
        {
            m_tcpClient = new TcpClient();
        }

        public bool Connect(string ipAddress, int port)
        {
            try
            {
                m_tcpClient.Connect(ipAddress, port);
                m_stream = m_tcpClient.GetStream();
                m_writer = new BinaryWriter(m_stream, Encoding.UTF8);
                m_reader = new StreamReader(m_stream, Encoding.UTF8);
                m_formatter = new BinaryFormatter();
                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return false;
            }
        }

        public void Run()
        {
            m_form = new MainWindow(this);
            m_form.Connection.IsChecked = true;

            Thread thread = new Thread(() => ProcessServerResponse());
            thread.Start();

            m_form.ShowDialog();

            m_tcpClient.Close();
        }

        private void ProcessServerResponse()
        {
            while (m_tcpClient.Connected)
            {
                m_form.UpdateChatDisplay(m_reader.ReadLine());
            }
        }

        public void SendMessage(Packet message)
        {
            MemoryStream ms = new MemoryStream();
            m_formatter.Serialize(ms, message);

            byte[] buffer = ms.GetBuffer();

            m_writer.Write(buffer.Length);
            m_writer.Write(buffer);

            m_writer.Flush();
        }

    }
}
