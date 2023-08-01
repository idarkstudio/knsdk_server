using MessagePack;
using System.Collections.Generic;

namespace FigNetCommon.Data
{
    [MessagePackObject]
    public sealed class OperationData
    {
        [Key(0)]
        public readonly Dictionary<byte, object> parameters;

        [IgnoreMember]
        public Dictionary<byte, object> Parameters => parameters;

        public OperationData(Dictionary<byte, object> parameters)
        {
            this.parameters = parameters;
        }
    }
}
