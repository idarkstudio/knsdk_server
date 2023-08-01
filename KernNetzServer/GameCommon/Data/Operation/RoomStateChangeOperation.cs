using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace FigNet.KernNetz.Operations
{
    public class RoomStateChangeOperation
    {
        public Message GetOperation(uint roomId, int state, bool isLocked)
        {

            Dictionary<byte, object> parameters = new Dictionary<byte, object>
            {
                { 0, roomId },
                { 1, state },
                { 2, isLocked }
            };

            Message operation = new Message((ushort)OperationCode.RoomStateChange, new OperationData(parameters));
            return operation;
        }

    }
}
