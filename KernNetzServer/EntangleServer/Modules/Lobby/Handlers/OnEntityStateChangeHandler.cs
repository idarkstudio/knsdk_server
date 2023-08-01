using FigNet.Core;
using FigNetCommon;
using FigNet.KernNetz;
using FigNetCommon.Data;

namespace KernNetzServer.Modules.Lobby.Handlers
{
    public class OnEntityStateChangeHandler : IHandler
    {
        public ushort MsgId => (ushort)OperationCode.OnEntityState;
        KNLobby lobby;

        public void HandleMessage(Message message, uint PeerId)
        {
            //var sender = FN.PeerCollection.GetPeerByID(PeerId);
            lobby ??= ServiceLocator.GetService<IServer>().GetModule<KNLobby>();
            var operation = message.Payload as OperationData;
            try
            {
                EntityType type = (EntityType)((byte)operation.Parameters[2]);
                uint entityId = (uint)operation.Parameters[0];
                uint roomId = (uint)operation.Parameters[1];
                var bytes = (byte[])operation.Parameters[3];

                EntangleState stateDelta = Default_Serializer.Deserialize2<EntangleState>(bytes);

                //if (stateDelta != null) stateDelta.COUNT();

                bool roomToOperate = lobby.Rooms.TryGetValue(roomId, out KNRoom room);
                //FN.Logger.Info($"{bytes.Length} bytes received");
                // FN.Logger.Warning($"EntityStateHandle: peer {PeerId} room {roomId} Type {type}");

                if (roomToOperate)
                {
                    if (stateDelta != null)
                    {
                        room.OnEntityStateChange(PeerId, type, entityId, stateDelta);
                    }
                    else
                    {
                        FN.Logger.Warning("@EntityStateChangeHandle coming state state is null");
                    }
                    //if (room.Players.ContainsKey(entityId))
                    //{
                    //    room.Players[entityId].ApplyStateDelta(stateDelta);
                    //}
                    //else
                    //{
                    //    FN.Logger.Error($"Entity# {entityId} not fount in Room# {roomId} ##Sendby {PeerId}");
                    //}
                    // room.SendToAll(message, DeliveryMethod.Reliable);    
                }
            }
            catch (System.Exception e)
            {
                FN.Logger.Exception(e, e.Message);
            }

        }
    }
}
