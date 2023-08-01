using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;

namespace KernNetzServer.Modules.Lobby.Handlers
{
    internal class DeleteEntityHandler : IHandler
    {
        public ushort MsgId => (ushort)OperationCode.DeleteEntity;
        public void HandleMessage(Message message, uint PeerId)
        {
            var operation = message.Payload as OperationData;

            uint roomId = (uint)operation.Parameters[0];
            uint networkId = (uint)operation.Parameters[1];
            EntityType entityType = (EntityType)((byte)operation.Parameters[2]);

            var lobby = ServiceLocator.GetService<IServer>().GetModule<KNLobby>();
            bool roomToJoin = lobby.Rooms.TryGetValue(roomId, out KNRoom room);

            if (roomToJoin)
            {
                bool sucess = room.OnDeleteEntity(networkId, entityType);
                operation.Parameters.Remove(0);

                if (sucess) room.SendToAll(message, DeliveryMethod.Reliable);

                FN.Logger.Info($"Delete Request {entityType} result {sucess}");
            }

        }
    }
}
