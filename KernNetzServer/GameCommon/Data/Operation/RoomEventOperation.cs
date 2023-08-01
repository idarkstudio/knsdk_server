using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace FigNet.KernNetz.Operations
{
    public class RoomEventOperation
    {
        public Message GetOperation(uint roomId, RoomEventData eventData, int deliveryMethod)
        {
            Dictionary<byte, object> parameters = new Dictionary<byte, object>
            {
                { 0, roomId },
                { 1, Default_Serializer.Serialize2<RoomEventData>(eventData) },
                { 2, deliveryMethod }
            };
            
            Message operation = new Message((ushort)OperationCode.RoomEvent, new OperationData(parameters));
            return operation;
        }

    }
}
