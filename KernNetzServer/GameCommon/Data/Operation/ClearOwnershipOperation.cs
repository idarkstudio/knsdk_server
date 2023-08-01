using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace FigNet.KernNetz.Operations
{
    public class ClearOwnershipOperation
    {
        public Message GetOperation(uint roomId, uint entityNetId)
        {
            Dictionary<byte, object> parameters = new Dictionary<byte, object>
            {
                { 0, roomId },
                { 1, entityNetId },
            };

            Message operation = new Message((ushort)OperationCode.ClearOwnerShip, new OperationData(parameters));
            return operation;
        }

    }
}
