using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;

namespace KernNetz.Handlers
{
    internal class RoomEventHandler : IHandler
    {
        public ushort MsgId => (ushort)OperationCode.RoomEvent;
        public void HandleMessage(Message message, uint PeerId)
        {
            var operation = message.Payload as OperationData;
            // sender
            uint networkId = (uint)operation.Parameters[0];
            RoomEventData data = Default_Serializer.Deserialize2<RoomEventData>((byte[])operation.Parameters[1]);//FN.Serializer.Deserialize<RoomEventData>(new System.ArraySegment<byte>((byte[])operation.Parameters[1]));
            var room = FigNet.KernNetz.KN.Room;

            room.OnEvent(networkId, data);

            FN.Logger.Info($"ROOM_EVENT {data.EventCode} | Sender {networkId}");
        }
    }
}
