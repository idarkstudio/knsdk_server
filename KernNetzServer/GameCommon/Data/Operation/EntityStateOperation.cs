using KernNetz;
using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace FigNet.KernNetz.Operations
{
    // todo: UpDate the operation remove entityId & EntityType Either from operation or ENStatebatch
    public class EntityStateOperation
    {
        public static Message GetOperation(ENStateFrameBatch state)
        {
            Dictionary<byte, object> parameters = new Dictionary<byte, object>
            {
                { 3, Default_Serializer.Serialize2<ENStateFrameBatch>(state) }
            };

            Message operation = new Message((ushort)OperationCode.OnEntityState, new OperationData(parameters));
            return operation;
        }

        public static Message GetOperation(uint entityId, uint roomId, EntityType type, EntangleState state)
        {
            Dictionary<byte, object> parameters = new Dictionary<byte, object>
            {
                { 0, entityId },
                { 1, roomId },
                { 2, type },
                { 3, Default_Serializer.Serialize2<EntangleState>(state) }
            };

            Message operation = new Message((ushort)OperationCode.OnEntityState, new OperationData(parameters));
            return operation;
        }
    }
}
