using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Net.Sockets;
using System.IO;


namespace ServerProject
{
    class ConnectedClients
    {
        private Socket m_Socket;
        private Stream m_NetworkStream;
        private BinaryReader m_Reader;
        private BinaryWriter m_Writer;
        private BinaryFormatter formatter;
        private object m_ReadLock;
        private object m_WriteLock;


        public ConnectedClients(Socket socket)
        {
            m_WriteLock = new object();
            m_ReadLock = new object();

            m_Socket = socket;

            m_NetworkStream = new NetworkStream(socket, true);

            m_Reader = new BinaryReader();
            m_Writer = new BinaryWriter();

            formatter = new BinaryFormatter();
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
