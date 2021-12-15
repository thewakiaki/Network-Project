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
}
