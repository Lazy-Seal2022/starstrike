using System;
using Unity.Collections;
using Unity.Netcode;

namespace StarStrike.Network
{
    [Serializable]
    public struct ArenaScoreEntry : INetworkSerializable, IEquatable<ArenaScoreEntry>
    {
        public ulong ClientId;
        public FixedString32Bytes PlayerName;
        public FixedString32Bytes ShipId;
        public int Kills;
        public int Deaths;
        public int Score;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref ShipId);
            serializer.SerializeValue(ref Kills);
            serializer.SerializeValue(ref Deaths);
            serializer.SerializeValue(ref Score);
        }

        public bool Equals(ArenaScoreEntry other)
        {
            return ClientId == other.ClientId;
        }

        public override bool Equals(object obj)
        {
            return obj is ArenaScoreEntry other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ClientId.GetHashCode();
        }
    }
}
