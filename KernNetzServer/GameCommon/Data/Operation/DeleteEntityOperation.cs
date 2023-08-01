using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace FigNet.KernNetz.Operations
{
    public class DeleteEntityOperation
    {
        public Message GetOperation(uint roomId, uint networkId, EntityType entityType)
        {
            Dictionary<byte, object> parameters = new Dictionary<byte, object>
            {
                { 0, roomId },
                { 1, networkId },
                { 2, (byte)entityType}
            };

            Message operation = new Message((ushort)OperationCode.DeleteEntity, new OperationData(parameters));
            return operation;
        }

    }
}
