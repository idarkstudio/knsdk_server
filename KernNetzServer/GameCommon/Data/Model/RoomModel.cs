using MessagePack;
using FigNet.KernNetz;
using System.Collections.Generic;

namespace FigNetCommon
{
    [MessagePackObject]
    public class RoomInfo
    {
        [Key(0)]
        public uint Id;
        [Key(1)]
        public string RoomName;
        [Key(2)]
        public int MaxPlayer;
        [Key(3)]
        public int ActivePlayer;
        [Key(4)]
        public int State;
    }

    // dummy Class to generate collections of type for AOT
    [MessagePackObject]
    public class RoomList
    {
        [Key(0)]
        public List<RoomInfo> Rooms;
    }

    [MessagePackObject]
    public class EntityDefaultState
    {
        [Key(0)]
        public List<KeyValuePair<byte, EntangleState>> states;
    }

    [MessagePackObject]
    public class RoomEventData
    {
        [Key(0)]
        public ushort EventCode;
        [Key(1)]
        public Dictionary<byte, object> Data;
    }
}
