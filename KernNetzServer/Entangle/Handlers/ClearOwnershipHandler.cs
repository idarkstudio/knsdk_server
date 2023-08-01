using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;

namespace KernNetz.Handlers
{
    internal class ClearOwnershipHandler : IHandler
    {

        public ushort MsgId => (ushort)OperationCode.ClearOwnerShip;
        public void HandleMessage(Message message, uint PeerId)
        {
            var operation = message.Payload as OperationData;
            uint ownerId = (uint)operation.Parameters[0];
            uint networkId = (uint)operation.Parameters[1];

            var room = FigNet.KernNetz.KN.Room;
            room.OnClearOwnership(networkId);
        }
    }
}
