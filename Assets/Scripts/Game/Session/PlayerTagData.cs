using Unity.Netcode;

namespace Game.Session
{
    public struct PlayerTagData : INetworkSerializable, System.IEquatable<PlayerTagData>
    {
        public ulong ClientId;
        public float TimeAsTagger;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref TimeAsTagger);
        }

        public bool Equals(PlayerTagData other)
        {
            return ClientId == other.ClientId;
        }
    }
}