using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace KernNetzServer.Modules.Lobby.Operations
{
    internal class AgentOwnershipChangeOperation
    {
        public Message GetOperation(uint OwnerId, uint entityNetId)
        {
            Dictionary<byte, object> parameters = new Dictionary<byte, object>
            {
                { 0, OwnerId },
                { 1, entityNetId },
            };

            Message operation = new Message((ushort)OperationCode.OnAgentOwnerShipChange, new OperationData(parameters));
            return operation;
        }

    }
}
