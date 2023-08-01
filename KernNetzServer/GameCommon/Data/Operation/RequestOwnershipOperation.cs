using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace FigNet.KernNetz.Operations
{
    public class RequestOwnershipOperation
    {
        public Message GetOperation(uint roomId, uint entityNetId, bool isLocked)
        {
            Dictionary<byte, object> parameters = new Dictionary<byte, object>
            {
                { 0, roomId },
                { 1, entityNetId },
                { 2, isLocked }
            };
            
            Message operation = new Message((ushort)OperationCode.RequestOwnerShip, new OperationData(parameters));
            return operation;
        }

    }
}
