using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Net.Sockets;
using System.IO;
using Packets;


namespace ServerProject
{
    class ConnectedClients
    {
        private Socket m_socket;
        private Stream m_networkStream;
        private BinaryReader m_reader;
        private BinaryWriter m_writer;
        private BinaryFormatter m_formatter;
        private object m_readLock;
        private object m_writeLock;


        public ConnectedClients(Socket socket)
        {
            m_writeLock = new object();
            m_readLock = new object();

            m_socket = socket;

            m_networkStream = new NetworkStream(socket, true);

            m_reader = new BinaryReader(m_networkStream, Encoding.UTF8);
            m_writer = new BinaryWriter(m_networkStream, Encoding.UTF8);

            m_formatter = new BinaryFormatter();
        }

        public void Close()
        {
            m_networkStream.Close();
            m_reader.Close();
            m_writer.Close();
            m_socket.Close();
        }

        public Packet Read()
        {
            lock (m_readLock)
            {
                int dataSize;

                if ((dataSize = m_reader.ReadInt32()) != -1)
                {
                    byte[] buffer = m_reader.ReadBytes(dataSize);
                    MemoryStream ms = new MemoryStream(buffer);

                    return m_formatter.Deserialize(ms) as Packet;
                }
                else
                {
                    return null;
                }

            }
        }

        public void Send(Packet message)
        {
            lock (m_writeLock)
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
}
