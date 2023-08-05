using FigNet.Core;
using FigNetCommon;
using System.Numerics;
using FigNetCommon.Data;

namespace KernNetz.Handlers
{
    internal class InstantiateEntityHandler : IHandler
    {
        public ushort MsgId => (ushort)OperationCode.InstantiateEntity;
        public void HandleMessage(Message message, uint PeerId)
        {
            var operation = message.Payload as OperationData;
            uint networkId = (uint)operation.Parameters[0];
            EntityType entityType = (EntityType)((byte)operation.Parameters[1]);
            short entityId = (short)operation.Parameters[2];
            var room = FigNet.KernNetz.KN.Room;
            uint ownerId = (uint)operation.Parameters[4];

            System.Numerics.Vector3 pos = System.Numerics.Vector3.Zero;
            System.Numerics.Vector3 rot = System.Numerics.Vector3.Zero;
            System.Numerics.Vector3 scale = System.Numerics.Vector3.One;

            if (operation.Parameters.ContainsKey(5))
            {
                var posData = Default_Serializer.Deserialize2<FNVector3>((byte[])operation.Parameters[5]);
                //var posData = operation.Parameters[5] as object[];
                var _vec3Pos = posData.Value; // new FNVec3() { __X__ = System.Convert.ToInt32(posData[0]), __Y__ = System.Convert.ToInt32(posData[1]), __Z__ = System.Convert.ToInt32(posData[2]) };
                pos = new Vector3(_vec3Pos.X, _vec3Pos.Y, _vec3Pos.Z);
            }

            if (operation.Parameters.ContainsKey(6))
            {
                var rotData = Default_Serializer.Deserialize2<FNVector3>((byte[])operation.Parameters[6]); // operation.Parameters[6] as object[];
                var _vec3Pos = rotData.Value; // new FNVec3() { __X__ = System.Convert.ToInt32(rotData[0]), __Y__ = System.Convert.ToInt32(rotData[1]), __Z__ = System.Convert.ToInt32(rotData[2]) };
                rot = new Vector3(_vec3Pos.X, _vec3Pos.Y, _vec3Pos.Z);
            }
            if (operation.Parameters.ContainsKey(7))
            {
                var scaleData = Default_Serializer.Deserialize2<FNVector3>((byte[])operation.Parameters[7]); // operation.Parameters[6] as object[];
                var _vec3Pos = scaleData.Value; //new FNVec3() { __X__ = System.Convert.ToInt32(scaleData[0]), __Y__ = System.Convert.ToInt32(scaleData[1]), __Z__ = System.Convert.ToInt32(scaleData[2]) };
                scale = new Vector3(_vec3Pos.X, _vec3Pos.Y, _vec3Pos.Z);
            }

            if (operation.Parameters.ContainsKey(3))
            {
                room.OnEntityCreate(entityType, entityId, networkId, ownerId, pos, rot, scale, states: (byte[])operation.Parameters[3]);
            }
            else
            {
                room.OnEntityCreate(entityType, entityId, networkId, ownerId, pos, rot, scale);
            }

        }
    }
}
