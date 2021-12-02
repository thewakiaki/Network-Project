using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace ClientProject
{
    public  class Client
    {
        private TcpClient m_tcpClient;
        private NetworkStream m_stream;
        private StreamWriter m_writer;
        private StreamReader m_reader;
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
                m_writer = new StreamWriter(m_stream, Encoding.UTF8);
                m_reader = new StreamReader(m_stream, Encoding.UTF8);
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

        public void SendMessage(string message)
        {
            m_writer.WriteLine(message);
            m_writer.Flush();
        }

    }
}
