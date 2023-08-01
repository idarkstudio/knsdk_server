using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;

namespace KernNetz.Handlers
{
    internal class AgentOwnershipChangeHandler : IHandler
    {
        public ushort MsgId => (ushort)OperationCode.OnAgentOwnerShipChange;
        public void HandleMessage(Message message, uint PeerId)
        {
            var operation = message.Payload as OperationData;
            uint ownerId = (uint)operation.Parameters[0];
            uint networkId = (uint)operation.Parameters[1];

            var room = FigNet.KernNetz.KN.Room;
            room.OnAgentOwnershipChange(networkId, ownerId);

            FN.Logger.Info($"On AGENT RequestOwnerSHip {ownerId}|{networkId}");
        }
    }
}
