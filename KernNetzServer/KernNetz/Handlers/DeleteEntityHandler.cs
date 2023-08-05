using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;

namespace KernNetz.Handlers
{
    internal class DeleteEntityHandler : IHandler
    {
        public ushort MsgId => (ushort)OperationCode.DeleteEntity;
        public void HandleMessage(Message message, uint PeerId)
        {
            var operation = message.Payload as OperationData;
            uint networkId = (uint)operation.Parameters[1];
            EntityType entityType = (EntityType)((byte)operation.Parameters[2]);

            var room = FigNet.KernNetz.KN.Room;
            room.OnEntityDelete(entityType, networkId);

        }
    }
}
