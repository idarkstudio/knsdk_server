using MessagePack;
using FigNet.KernNetz;

namespace KernNetz
{
    [MessagePackObject]
    public class ENStateFrame
    {
        [Key(0)]
        public uint NetworkEntityId;
        [Key(1)]
        public EntangleState State;
    }
}
