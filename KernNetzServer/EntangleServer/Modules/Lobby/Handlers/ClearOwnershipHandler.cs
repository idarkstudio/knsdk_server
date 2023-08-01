using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;

namespace KernNetzServer.Modules.Lobby.Handlers
{
    internal class ClearOwnershipHandler : IHandler
    {
        private static KNLobby lobby;

        public ushort MsgId => (ushort)OperationCode.ClearOwnerShip;
        public void HandleMessage(Message message, uint PeerId)
        {
            var operation = message.Payload as OperationData;
            uint roomId = (uint)operation.Parameters[0];
            uint entityNetId = (uint)operation.Parameters[1];

            lobby ??= ServiceLocator.GetService<IServer>().GetModule<KNLobby>();

            bool roomToJoin = lobby.Rooms.TryGetValue(roomId, out KNRoom room);

            if (roomToJoin && room.IsInRoom(PeerId))
            {
                var sucess = room.OnClearOwnership(PeerId, entityNetId);
                if (sucess)
                {
                    operation.Parameters[0] = PeerId;
                    room.SendToAll(message, DeliveryMethod.Reliable);
                }

                FN.Logger.Info($"OwnerShip CLEAR r {roomId} eI {entityNetId} pI {PeerId} - granted {sucess}");
            }
        }
    }
}
