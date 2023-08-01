using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace FigNet.KernNetz.Operations
{
    public class MasterClientChangeOperation
    {
        // RoomName
        // isMaster
        public Message GetOperation(uint playerId, bool isMaster)
        {
            Dictionary<byte, object> parameters = new Dictionary<byte, object>
            {
                { 0, playerId },
                { 1, isMaster }
            };

            Message operation = new Message((ushort)OperationCode.OnMasterClientChange, new OperationData(parameters));
            return operation;
        }

    }
}
