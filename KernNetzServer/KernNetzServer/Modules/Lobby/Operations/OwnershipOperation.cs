using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace KernNetzServer.Modules.Lobby.Operations
{
    internal class OwnershipOperation
    {
        public Message GetOperation(uint OwnerId, uint entityNetId, EntityType entityType, bool isLocked)
        {
            Dictionary<byte, object> parameters = new Dictionary<byte, object>
            {
                { 0, OwnerId },
                { 1, entityNetId },
                { 2, (byte)entityType },
                { 3, isLocked },
            };

            Message operation = new Message((ushort)OperationCode.RequestOwnerShip, new OperationData(parameters));
            return operation;
        }

    }
}
