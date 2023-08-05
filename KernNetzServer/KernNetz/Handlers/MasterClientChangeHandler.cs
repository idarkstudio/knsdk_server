using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;

namespace KernNetz.Handlers
{
    internal class MasterClientChangeHandler : IHandler
    {
        public ushort MsgId => (ushort)OperationCode.OnMasterClientChange;
        public void HandleMessage(Message message, uint PeerId)
        {
            var operation = message.Payload as OperationData;
            uint playerId = (uint)operation.Parameters[0];

            FigNet.KernNetz.KN.IsMasterClient = (bool)operation.Parameters[1];
            // todo: here set is kinematic to false if entity is not locked
            FigNet.KernNetz.KN.OnMasterClientUpdate?.Invoke(FigNet.KernNetz.KN.IsMasterClient);
            FN.Logger.Info($"NEW MASTER IN TOWN Client Status of {playerId} is {FigNet.KernNetz.KN.IsMasterClient}");
        }
    }
}
