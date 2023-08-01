using System;
using FigNet.Core;
using MessagePack;
using FigNetCommon.Data;

namespace FigNetCommon
{
    public class Default_Serializer : FigNet.Core.ISerializer
    {
        private static MessagePackSerializerOptions Options;
        private readonly ArrayBufferWriter bufferWriter = new ArrayBufferWriter();
        public Default_Serializer()
        {
            var resolver = MessagePack.Resolvers.CompositeResolver.Create(
                MessagePack.Resolvers.GeneratedResolver.Instance,
                MessagePack.Resolvers.StandardResolver.Instance,
                MessagePack.Resolvers.PrimitiveObjectResolver.Instance
               );

            Options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
           
            MessagePackSerializer.DefaultOptions = Options;

        }

        public T Deserialize<T>(ArraySegment<byte> buffer)
        {
            return MessagePackSerializer.Deserialize<T>(buffer, Options);
        }

        public ArraySegment<byte> Serialize<T>(object value)
        {
            return new ArraySegment<byte>(MessagePackSerializer.Serialize<T>((T)value, Options));
            //lock (bufferWriter)
            //{

            //    bufferWriter.Clear();
            //    bufferWriter.RecycleWrittenBytes();
            //    MessagePackSerializer.Serialize<T>(bufferWriter, (T)value, Options);
            //    bufferWriter.PrepareWrittenBytes();
            //    return bufferWriter.GetAsArraySegment();
            //}
        }

        public static T Deserialize2<T>(byte[] buffer)
        {
            return MessagePackSerializer.Deserialize<T>(buffer, Options);
        }

        public static byte[] Serialize2<T>(object value)
        {
            return MessagePackSerializer.Serialize<T>((T)value, Options);
        }

        public void RegisterPayloads()
        {
            FN.RegisterPayload((ushort)OperationCode.OnAgentOwnerShipChange, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.OnPlayerLeftRoom, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.OnPlayerJoinRoom, Serialize<OperationData>, Deserialize<OperationData>);

            FN.RegisterPayload((ushort)OperationCode.ClearOwnerShip, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.CreateRoom, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.DeleteEntity, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.GetRoomList, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.InstantiateEntity, Serialize<OperationData>, Deserialize<OperationData>);

            FN.RegisterPayload((ushort)OperationCode.JoinRoom, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.LeaveRoom, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.AppKey, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.OnEntityState, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.RequestOwnerShip, Serialize<OperationData>, Deserialize<OperationData>);

            FN.RegisterPayload((ushort)OperationCode.RoomEvent, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.RoomStateChange, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.OnMasterClientChange, Serialize<OperationData>, Deserialize<OperationData>);

            FN.RegisterPayload((ushort)OperationCode.PreRoomStateReceived, Serialize<OperationData>, Deserialize<OperationData>);
            FN.RegisterPayload((ushort)OperationCode.PostRoomStateReceived, Serialize<OperationData>, Deserialize<OperationData>);
        }

    }
}
