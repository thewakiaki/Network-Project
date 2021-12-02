using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;


namespace ServerProject
{
    class ConnectedClients
    {
        private Socket m_Socket;
        private Stream m_NetworkStream;
        private StreamReader m_Reader;
        private StreamWriter m_Writer;
        private object m_ReadLock;
        private object m_WriteLock;


        public ConnectedClients(Socket socket)
        {
            m_WriteLock = new object();
            m_ReadLock = new object();

            m_Socket = socket;

            m_NetworkStream = new NetworkStream(socket, true);

            m_Reader = new StreamReader(m_NetworkStream, Encoding.UTF8);
            m_Writer = new StreamWriter(m_NetworkStream, Encoding.UTF8);
        }

        public void Close()
        {
            m_NetworkStream.Close();
            m_Reader.Close();
            m_Writer.Close();
            m_Socket.Close();
        }

        public string Read()
        {
            lock (m_ReadLock)
            {
                return m_Reader.ReadLine();
            }
        }

        public void Send(string message)
        {
            lock (m_WriteLock)
            {
                m_Writer.WriteLine(message);
                m_Writer.Flush();
            }
        }
    }
}
