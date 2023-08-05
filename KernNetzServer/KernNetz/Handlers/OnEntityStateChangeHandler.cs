using System;
using FigNet.Core;
using FigNetCommon;
using FigNet.KernNetz;
using FigNetCommon.Data;

namespace KernNetz.Handlers
{
    internal class OnEntityStateChangeHandler : IHandler
    {
        public ushort MsgId => (ushort)OperationCode.OnEntityState;
        public void HandleMessage(Message message, uint PeerId)
        {
            var operation = message.Payload as OperationData;
            // todo: rent memory pool
            try
            {
                //uint entityId = (uint)operation.Parameters[0];
                //uint roomId = (uint)operation.Parameters[1];
                //EntityType type = (EntityType)((byte)operation.Parameters[2]);
                ENStateFrameBatch batch = Default_Serializer.Deserialize2<ENStateFrameBatch>((byte[])operation.Parameters[3]); // FN.Serializer.Deserialize<ENStateFrameBatch>(new ArraySegment<byte>((byte[])operation.Parameters[3]));

                EntityType type = batch.EntityType;

                foreach (var state in batch.ENStateFrames)
                {
                    uint entityId = state.NetworkEntityId;
                    var room = FigNet.KernNetz.KN.Room;
                    room.OnEntityStateChangeReceive(type, entityId, state.State);
                }


            }
            catch (Exception e)
            {
                FN.Logger.Exception(e, e.Message);
            }


        }
    }
}
