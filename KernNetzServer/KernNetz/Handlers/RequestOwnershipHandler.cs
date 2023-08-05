using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;

namespace KernNetz.Handlers
{
    internal class RequestOwnershipHandler : IHandler
    {
        public ushort MsgId => (ushort)OperationCode.RequestOwnerShip;
        public void HandleMessage(Message message, uint PeerId)
        {
            var operation = message.Payload as OperationData;
            uint ownerId = (uint)operation.Parameters[0];
            uint networkId = (uint)operation.Parameters[1];
            bool isLock = (bool)operation.Parameters[2];

            var room = FigNet.KernNetz.KN.Room;
            room.OnOwnershipRequest(networkId, ownerId, isLock);

            FN.Logger.Info($"OnRequestOwnerSHip {ownerId}|{networkId}|{isLock}");
        }
    }
}
