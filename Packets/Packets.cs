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
        GameChatMessage,
        ClientName,
        NameCheck,
        LoginPacket,
        Key,
        JoinRPSLobby,
        PlayingRPS,
        RPSOption,
        RPSResult,
        RPSNextRound,
        RPSGameEnd,
        JoinedPongLobby,
        PlayingPong,
        PlayerInput,
        PredictedMovement,
        PlayerList
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
    public class GameChatMessagePacket : Packet
    {
        public string messageToSend;
        public int lobbyNumber;

        public GameChatMessagePacket(string message, int number)
        {
            messageToSend = message;
            lobbyNumber = number;

            packetType = PacketType.GameChatMessage;
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

    [Serializable]
    public class JoinedRPSLobbyPacket : Packet
    {
        public bool joinedLobby;

        public JoinedRPSLobbyPacket(bool joinStatus)
        {
            joinedLobby = joinStatus;
            packType = PacketType.JoinRPSLobby;
        }
    }

    [Serializable]
    public class JoinedPongLobbyPacket : Packet
    {
        public bool joinedLobby;

        public JoinedPongLobbyPacket(bool joinStatus)
        {
            joinedLobby = joinStatus;
            packType = PacketType.JoinedPongLobby;
        }
    }

    [Serializable]
    public class PlayingRPSPacket : Packet
    {
        public bool playing;
        public int lobbyNo;
        public string message;

        public PlayingRPSPacket(bool play, int number)
        {
            playing = play;
            lobbyNo = number;
            message = "Joined Lobby " + number + ". Game has started";
            packType = PacketType.PlayingRPS;
        }
    }

    [Serializable]

    public class PlayingPongPacket : Packet
    {
        public bool playing;
        public int lobbyNo;
        public string message;

        public PlayingPongPacket(bool play, int number)
        {
            playing = play;
            lobbyNo = number;
            message = "Joined Lobby " + number + ". Game has started";
            packType = PacketType.PlayingPong;
        }
    }

    [Serializable]
    public class PlayerListPacket : Packet
    {
        public string player1;
        public string player2;

        public PlayerListPacket(string name1, string name2)
        {
            player1 = name1;
            player2 = name2;

            packType = PacketType.PlayerList;
        }
    }

    [Serializable]
    public class RPSOptionPacket : Packet
    {
        public string option;

        public RPSOptionPacket(string choice)
        {
            option = choice;

            packType = PacketType.RPSOption;
        }
    }

    [Serializable]
    public class RPSResultPacket : Packet
    {
        public string player1;
        public string player2;

        public RPSResultPacket(string p1, string p2)
        {
            player1 = p1;
            player2 = p2;

            packType = PacketType.RPSResult;
        }
    }

    [Serializable]
    public class RPSGameEndPacket : Packet
    {
        public bool gameEnded;

        public RPSGameEndPacket(bool state)
        {
            gameEnded = state;

            packType = PacketType.RPSGameEnd;
        }
    }

    [Serializable]
    public class RPSNextRoundPacket : Packet
    {
        public int p1Score;
        public int p2Score;

        public RPSNextRoundPacket(int s1, int s2)
        {
            p1Score = s1;
            p2Score = s2;

            packType = PacketType.RPSNextRound;
        }
    }

    [Serializable]
    public class PlayerInputPacket : Packet
    {
        public bool moveUp;
        public bool moveDown;

        public PlayerInputPacket(bool up, bool down)
        {
            moveUp = up;
            moveDown = down;

            packType = PacketType.PlayerInput;
        }
    }

    [Serializable]
    public class PredictedMovementPacket : Packet
    {
        public int player1;
        public int player2;

        public PredictedMovementPacket(int p1, int p2)
        {
            player1 = p1;
            player2 = p2;

            packType = PacketType.PredictedMovement;
        }
    }


}
