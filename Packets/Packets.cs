using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packets
{
    public enum PacketType
    {
        ChatMessage,
        PrivateMessage,
        ClientName
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
}
