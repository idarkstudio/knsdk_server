using System.Collections.Generic;

namespace KernNetz
{
    [System.Serializable]
    public class KernNetzConfig
    {
        public string AppId;
        public string ServerIp;
        public int Port;
        public string TransportLayer;
        public int MaxConnections; // optional I guess
        public int MaxChannels;
        public int DisconnectTimeout;
        public List<NetworkLOD> LODs;
    }

    [System.Serializable]
    public class NetworkLOD
    {
        public LODLevel level;
        public byte SyncPercent; // 0-100
    }

    public enum LODLevel : byte
    {
        Level0,
        Level1,
        Level2
    }
}
