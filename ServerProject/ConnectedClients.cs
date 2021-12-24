using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
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

        private RSACryptoServiceProvider m_RSAprovider;
        private RSAParameters m_publicKey;
        private RSAParameters m_privateKey;
        private RSAParameters m_clientKey;

        public RSAParameters publicKey
        {
            get { return m_publicKey; }
        }

        public IPEndPoint EndPoint;

        public string clientUsername;
        public string clientNickName;

        public bool isPlayingRPS;

        public string choice;
        public int RPSScore;


        public ConnectedClients(Socket socket)
        {
            m_RSAprovider = new RSACryptoServiceProvider(1024);
            m_publicKey = m_RSAprovider.ExportParameters(false);
            m_privateKey = m_RSAprovider.ExportParameters(true);

            m_writeLock = new object();
            m_readLock = new object();

            m_socket = socket;

            m_networkStream = new NetworkStream(socket, true);

            m_reader = new BinaryReader(m_networkStream, Encoding.UTF8);
            m_writer = new BinaryWriter(m_networkStream, Encoding.UTF8);

            m_formatter = new BinaryFormatter();

            isPlayingRPS = false;
            choice = "";
            RPSScore = 0;
        }

        public void Close()
        {
            m_networkStream.Close();
            m_reader.Close();
            m_writer.Close();
            m_socket.Close();
        }

        public Packet ReadTCP()
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

        public void SendTCP(Packet message)
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

        private byte[] Encrypt(byte[] data)
        {
            lock (m_RSAprovider)
            {
                m_RSAprovider.ImportParameters(m_clientKey);
            }

            return m_RSAprovider.Encrypt(data, true);
        }

        private byte[] Decrypt(byte[] data)
        {
            lock (m_RSAprovider)
            {
                m_RSAprovider.ImportParameters(m_privateKey);
            }

            return m_RSAprovider.Decrypt(data, true);
        }

        internal byte[] EncryptString(string message)
        {

            byte[] messageData = Encoding.UTF8.GetBytes(message);

            byte[] encryptedMessage = Encrypt(messageData);

            return encryptedMessage;
        }

        internal string DecryptString(byte[] message)
        {

            byte[] decryptedMessage = Decrypt(message);
            string sDecryptedMessage = Encoding.UTF8.GetString(decryptedMessage);

            return sDecryptedMessage;
        }

        public void SetClientKey(RSAParameters key)
        {
            m_clientKey = key;
        }
    }
}
