using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace FigNet.KernNetz.Operations
{
    public class InstantiateOperation
    {
        public Message GetOperation(uint roomId, EntityType entityType, short entityId, FNVec3 position = default, FNVec3 rotation = default, FNVec3 scale = default)
        {
            Dictionary<byte, object> parameters = new Dictionary<byte, object>
            {
                { 0, roomId },
                { 1, (byte)entityType },
                { 2, entityId }
            };
            if (position != default)
            {
                parameters.Add(5, Default_Serializer.Serialize2<FNVector3>(new FNVector3() { Value = position })); //FigNetCommon.Utils.Serializer.Serialize<FNVector3>(new FNVector3() { Value = position })); ; ;
            }
            if (rotation != default)
            {
                parameters.Add(6, Default_Serializer.Serialize2<FNVector3>(new FNVector3() { Value = rotation })); //FigNetCommon.Utils.Serializer.Serialize<FNVector3>(new FNVector3() { Value = rotation }));
            }
            if (scale != default)
            {
                parameters.Add(7, Default_Serializer.Serialize2<FNVector3>(new FNVector3() { Value = scale }));//FigNetCommon.Utils.Serializer.Serialize<FNVector3>(new FNVector3() { Value = scale }));
            }
            Message operation = new Message((ushort)OperationCode.InstantiateEntity, new OperationData(parameters));
            return operation;
        }

    }
}
