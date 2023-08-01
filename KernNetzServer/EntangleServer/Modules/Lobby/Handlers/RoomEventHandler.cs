using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;

namespace KernNetzServer.Modules.Lobby.Handlers
{
    internal class RoomEventHandler : IHandler
    {
        private static KNLobby lobby;

        public ushort MsgId => (ushort)OperationCode.RoomEvent;
        public void HandleMessage(Message message, uint PeerId)
        {
            lobby ??= ServiceLocator.GetService<IServer>().GetModule<KNLobby>();

            var operation = message.Payload as OperationData;

            uint roomId = (uint)operation.Parameters[0];
            DeliveryMethod delivery = (DeliveryMethod)operation.Parameters[2];
            bool roomToOperate = lobby.Rooms.TryGetValue(roomId, out KNRoom room);

            if (roomToOperate)
            {
                operation.Parameters[0] = PeerId;
                room.SendToAll(message, delivery);
            }
            //FN.Logger.Info($"ROOM_EVENT | Sender {PeerId}");
        }
    }
}
