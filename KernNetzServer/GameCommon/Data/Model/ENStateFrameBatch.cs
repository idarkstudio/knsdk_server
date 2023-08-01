using FigNetCommon;
using MessagePack;
using System.Collections.Generic;

namespace KernNetz
{
    [MessagePackObject]
    public class ENStateFrameBatch
    {
        //[Key(0)]
        //public uint RoomId;
        [Key(0)]
        public EntityType EntityType;
        [Key(1)]
        public List<ENStateFrame> ENStateFrames;
    }
}
