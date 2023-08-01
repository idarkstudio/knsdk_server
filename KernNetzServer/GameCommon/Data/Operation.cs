using MessagePack;
using System.Collections.Generic;

namespace FigNetCommon
{
    //[MessagePackObject]
    //public sealed class Operation : IMessage
    //{
    //    [Key(0)]
    //    public readonly ushort code;
    //    [Key(1)]
    //    public readonly Dictionary<byte, object> parameters;
    //    [Key(2)]
    //    public byte? CallbackCode { get; set; }
    //    public Operation(ushort code, Dictionary<byte, object> parameters, byte? callbackCode = null)
    //    {
    //        this.code = code;
    //        this.parameters = parameters;
    //        this.CallbackCode = callbackCode;
    //    }

    //    [IgnoreMember]
    //    public ushort OpCode => code;
    //    [IgnoreMember]
    //    public Dictionary<byte, object> Parameters => parameters;
    //}
}
