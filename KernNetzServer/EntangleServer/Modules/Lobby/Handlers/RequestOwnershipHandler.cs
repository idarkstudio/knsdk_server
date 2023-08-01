using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;

namespace KernNetzServer.Modules.Lobby.Handlers
{
    internal class RequestOwnershipHandler : IHandler
    {
        private static KNLobby lobby;

        public ushort MsgId => (ushort)OperationCode.RequestOwnerShip;
        public void HandleMessage(Message message, uint PeerId)
        {
            var operation = message.Payload as OperationData;

            uint roomId = (uint)operation.Parameters[0];
            uint entityNetId = (uint)operation.Parameters[1];
            bool isLock = (bool)operation.Parameters[2];

            lobby ??= ServiceLocator.GetService<IServer>().GetModule<KNLobby>();

            bool roomToJoin = lobby.Rooms.TryGetValue(roomId, out KNRoom room);

            if (roomToJoin && room.IsInRoom(PeerId))
            {
                var sucess = room.OnRequestOwnership(PeerId, entityNetId, isLock);
                if (sucess)
                {
                    operation.Parameters[0] = PeerId;

                }
                else
                {
                    var item = room.Items.Find(i => i.NetworkId == entityNetId);
                    if (item != null)
                    {
                        operation.Parameters[0] = item.OwnerId;
                    }
                }

                room.SendToAll(message, DeliveryMethod.Reliable);
                FN.Logger.Info($"OwnerShip Request r {roomId} eI {entityNetId} pI {PeerId} - granted {sucess}");
            }
        }
    }
}
