using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace FigNet.KernNetz.Operations
{
    public class GetRoomListOperation
    {
        public Message GetOperation(RoomQuery query)
        {
            Dictionary<byte, object> parameters = new Dictionary<byte, object>
            {
                { 0, query }
            };

            Message operation = new Message((ushort)OperationCode.GetRoomList, new OperationData(parameters));
            return operation;
        }

    }
}
