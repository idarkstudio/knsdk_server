using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;

namespace KernNetzServer.Modules.Lobby.Handlers
{
    internal class RoomStateChangeHandler : IHandler
    {
        private static KNLobby lobby;

        public ushort MsgId => (ushort)OperationCode.RoomStateChange;
        public void HandleMessage(Message message, uint PeerId)
        {
            lobby ??= ServiceLocator.GetService<IServer>().GetModule<KNLobby>();

            var operation = message.Payload as OperationData;
            uint roomId = (uint)operation.Parameters[0];
            int state = (int)operation.Parameters[1];
            bool isLock = (bool)operation.Parameters[2];

            bool roomToOperate = lobby.Rooms.TryGetValue(roomId, out KNRoom room);

            if (roomToOperate)
            {
                operation.Parameters[0] = PeerId;
                room.UpdateRoomState(state, isLock);
                room.SendToAll(message, DeliveryMethod.Reliable);
            }
            FN.Logger.Info($"RoomState Change | Sender {PeerId}");
        }
    }
}
