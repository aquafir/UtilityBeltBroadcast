using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityBeltBroadcast
{
    public class MessageHeader
    {
        public static readonly int SIZE = sizeof(int) * 5; // header is int type, int flags, int sendingClientId, int targetClientId, int length

        public MessageHeaderType Type;
        public MessageHeaderFlags Flags;
        public int SendingClientId;
        public int TargetClientId;
        public int BodySize;

        public static MessageHeader CreateFrom(BinaryReader reader)
        {
            return new MessageHeader()
            {
                Type = (MessageHeaderType)reader.ReadInt32(),
                Flags = (MessageHeaderFlags)reader.ReadInt32(),
                SendingClientId = reader.ReadInt32(),
                TargetClientId = reader.ReadInt32(),
                BodySize = reader.ReadInt32(),
            };
        }

        public static void ToBytes(out byte[] bytes, MessageHeader header)
        {
            bytes = new byte[SIZE];
            using (var stream = new MemoryStream(bytes))
            using (var writer = new BinaryWriter(stream))
                WriteTo(writer, header);
        }

        public static void WriteTo(BinaryWriter writer, MessageHeader header)
        {
            writer.Write((int)header.Type);
            writer.Write((int)header.Flags);
            writer.Write((int)header.SendingClientId);
            writer.Write((int)header.TargetClientId);
            writer.Write((int)header.BodySize);
        }
    }

    public enum MessageHeaderType : int
    {
        KeepAlive = 1,
        Serialized = 2,
        RemoteClientConnected = 3,
        RemoteClientDisconnected = 4,
        ClientInit = 5,
        DecalString = 6,
    }

    public enum MessageHeaderFlags : int
    {
        None = 0x00000000
    }
}
