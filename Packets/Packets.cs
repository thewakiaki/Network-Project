using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;

namespace Packets
{
    public enum PacketType
    {
        ChatMessage,
        EncryptedChatMessage,
        PrivateMessage,
        ClientName,
        NameCheck,
        LoginPacket,
        Key
    }

    [Serializable]
    public class Packet
    {
        protected PacketType packType;

        public PacketType packetType
        {
            get { return packType; }
            set { packType = value; }
        }
    }

    [Serializable]
    public class ChatMessagePacket : Packet
    {
        public string message;

        public ChatMessagePacket(string messages)
        {
            message = messages;
            packType = PacketType.ChatMessage;
        }
    }

    [Serializable]
    public class EncryptedChatMessagePacket : Packet
    {
        public byte[] message;

        public EncryptedChatMessagePacket(byte[] messages)
        {
            message = messages;
            packType = PacketType.EncryptedChatMessage;
        }
    }

    [Serializable]
    public class ClientNamePacket : Packet
    {
        public string username;
        public string nickname;

        public ClientNamePacket(string user, string nick)
        {
            username = user;
            nickname = nick;

            packType = PacketType.ClientName;
        }
    }

    [Serializable]
    public class NameCheckPacket : Packet
    {

        public int type;

        public NameCheckPacket(int check)
        {
            type = check;
            packetType = PacketType.NameCheck;
        }
    }

    [Serializable]
    public class PrivateMessagePacket : Packet
    {
        public string message;
        public string targetClient;
        public string sendingClient;

        public PrivateMessagePacket(string messages, string target, string sender)
        {
            message = messages;
            targetClient = target;
            sendingClient = sender;
            packType = PacketType.PrivateMessage;
        }
    }

    [Serializable]
    public class LoginPacket : Packet
    {
        public IPEndPoint EndPoint;
        private RSAParameters m_key;

        public RSAParameters key
        {
            get { return m_key; }
        }


        public LoginPacket(IPEndPoint IpEndPoint, RSAParameters key)
        {
            EndPoint = IpEndPoint;
            m_key = key;
            packType = PacketType.LoginPacket;
        }
    }

    [Serializable]
    public class KeyPacket : Packet
    {
        private RSAParameters m_serverKey;

        public RSAParameters serverKey
        {
            get { return m_serverKey; }
        }

        public KeyPacket(RSAParameters key)
        {
            m_serverKey = key;

            packType = PacketType.Key;
        }
    }
}
