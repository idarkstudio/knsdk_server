using FigNet.Core;
using FigNetCommon;
using System.Numerics;
using FigNetCommon.Data;
using System;

namespace KernNetzServer.Modules.Lobby.Handlers
{
    internal class InstantiateEntityHandler : IHandler
    {
        public ushort MsgId => (ushort)OperationCode.InstantiateEntity;

        private Vector4 ToQuaternion(Vector3 v)
        {

            v.X = v.X * 0.01745f;
            v.Y = v.Y * 0.01745f;
            v.Z = v.Z * 0.01745f;

            float cy = (float)Math.Cos(v.Z * 0.5f);
            float sy = (float)Math.Sin(v.Z * 0.5f);
            float cp = (float)Math.Cos(v.Y * 0.5f);
            float sp = (float)Math.Sin(v.Y * 0.5f);
            float cr = (float)Math.Cos(v.X * 0.5f);
            float sr = (float)Math.Sin(v.X * 0.5f);

            return new Vector4
            {
                W = (cr * cp * cy + sr * sp * sy),
                X = (sr * cp * cy - cr * sp * sy),
                Y = (cr * sp * cy + sr * cp * sy),
                Z = (cr * cp * sy - sr * sp * cy)
            };

        }

        public void HandleMessage(Message message, uint PeerId)
        {
            try
            {
                var operation = message.Payload as OperationData;
                uint roomId = (uint)operation.Parameters[0];
                EntityType entityType = (EntityType)((byte)operation.Parameters[1]);
                short entityId = (short)operation.Parameters[2];

                var lobby = ServiceLocator.GetService<IServer>().GetModule<KNLobby>();
                bool roomToJoin = lobby.Rooms.TryGetValue(roomId, out KNRoom room);

                if (roomToJoin && room.IsInRoom(PeerId))
                {
                    // in case of agent owner is Master client

                    Vector3 pos = default;
                    Vector3 rot = default;
                    Vector3 scale = default;

                    if (operation.Parameters.ContainsKey(5))
                    {
                        var posData = Default_Serializer.Deserialize2<FNVector3>((byte[])operation.Parameters[5]); // operation.Parameters[5] as object[];
                        var _vec3Pos = posData.Value; // new FNVec3() { __X__ = System.Convert.ToInt32(posData[0]), __Y__ = System.Convert.ToInt32(posData[1]), __Z__ = System.Convert.ToInt32(posData[2]) };
                        pos = new Vector3(_vec3Pos.X, _vec3Pos.Y, _vec3Pos.Z);
                    }
                    if (operation.Parameters.ContainsKey(6))
                    {
                        var rotData = Default_Serializer.Deserialize2<FNVector3>((byte[])operation.Parameters[6]);//operation.Parameters[6] as object[];
                        var _vec3Pos = rotData.Value; //new FNVec3() { __X__ = System.Convert.ToInt32(rotData[0]), __Y__ = System.Convert.ToInt32(rotData[1]), __Z__ = System.Convert.ToInt32(rotData[2]) };
                        rot = new Vector3(_vec3Pos.X, _vec3Pos.Y, _vec3Pos.Z);

                        Vector4 quat = ToQuaternion(rot); 

                        FNVector4 quatRot = new FNVector4();
                        quatRot.Value = new FNVec4(quat.X, quat.Y, quat.Z, quat.W);

                        operation.Parameters[6] = Default_Serializer.Serialize2<FNVector4>(quatRot);
                    }
                    if (operation.Parameters.ContainsKey(7))
                    {
                        var scaleData = Default_Serializer.Deserialize2<FNVector3>((byte[])operation.Parameters[7]);// operation.Parameters[6] as object[];
                        var _vec3Pos = scaleData.Value; //new FNVec3() { __X__ = System.Convert.ToInt32(scaleData[0]), __Y__ = System.Convert.ToInt32(scaleData[1]), __Z__ = System.Convert.ToInt32(scaleData[2]) };
                        scale = new Vector3(_vec3Pos.X, _vec3Pos.Y, _vec3Pos.Z);
                    }

                    uint networkId = room.OnInstantiateEntity(PeerId, entityId, entityType, pos);
                    operation.Parameters.Add(4, entityType == EntityType.Item ? PeerId : room.MasterClient);
                    operation.Parameters[0] = networkId;
                    // add ownerId
                    //counter++;
                    //byte channelID = (byte)(counter % FNERoom.MAX_CHANNEL_LIMIT);
                    room.SendToAll(message, DeliveryMethod.Reliable, 0);
                    //if (entityType == EntityType.Agent)
                    //{
                    //    room.SendAgentOwnerToAll();
                    //}
                    //  FN.Logger.Error($"<<INSTANTIATE REQUEST>> by user: {PeerId} room id: {roomId} Entity Type {entityType} | Entity Created WithID {networkId} pos {pos} rot {rot}");
                }
            }
            catch (System.Exception e)
            {
                FN.Logger.Exception(e, e.Message);
            }


        }
    }
}
