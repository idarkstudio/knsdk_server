using System;
using FigNet.Core;
using FigNetCommon.Data;

namespace FigNetCommon
{
    public static class Utils
    {
        public static FigNet.Core.ILogger Logger { get; set; }
        public static FigNet.Core.ISerializer Serializer { get; set; }

        public static void RegisterPayloads()
        {
            // here register payloads 
            FN.RegisterPayload((ushort)OperationCode.ClearOwnerShip, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.CreateRoom, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.DeleteEntity, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.OnEntityState, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.GetRoomList, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.InstantiateEntity, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.JoinRoom, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.OnMasterClientChange, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.AppKey, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.RequestOwnerShip, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.RoomEvent, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.RoomStateChange, Serialize<OperationData>, Deserialize<OperationData>);
        }

        public static object Deserialize<T>(ArraySegment<byte> buffer)
        {
            return Serializer.Deserialize<T>(buffer);
        }

        public static ArraySegment<byte> Serialize<T>(object value)
        {
            //T item = (T)value;
            var bytes = Serializer.Serialize<T>(value);
            return bytes;
        }
    }
}
