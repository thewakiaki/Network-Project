using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packets
{ 
    [Serializable]
    public class Packets
    {
        protected enum PacketType
        {
            ChatMessage,
            PrivateMessage,
            ClientName
        }

        protected PacketType packType;

        PacketType packetType
        {
            get { return packType; }
            set { packType = value; }
        }
    }

    [Serializable]
    public class ChatMessagePacket : Packets
    {
        public string message;

        ChatMessagePacket(string messages)
        {
            message = messages;
            packType = PacketType.ChatMessage;
        }
    }
}
