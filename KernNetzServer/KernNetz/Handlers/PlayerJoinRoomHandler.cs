using FigNet.Core;
using FigNetCommon;
using FigNet.KernNetz;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace KernNetz.Handlers
{
    internal class PlayerJoinRoomHandler : IHandler
    {
        public ushort MsgId => (ushort)OperationCode.OnPlayerJoinRoom;
        public void HandleMessage(Message message, uint PeerId)
        {
            var operation = message.Payload as OperationData;
            bool isMine = (bool)operation.Parameters[0];
            uint playerId = (uint)operation.Parameters[1];
            EntityDefaultState state = null;

            var room = FigNet.KernNetz.KN.Room;

            //FN.Logger.Error($"OnPlayer JOIN HANDLER {isMine}");

            var netPlayer = FigNet.KernNetz.NetPlayer.CreatePlayer(playerId, isMine: isMine);
            if (!isMine)
            {
                // todo global memoy rent pool
                state = Default_Serializer.Deserialize2<EntityDefaultState>((byte[])operation.Parameters[2]);// FN.Serializer.Deserialize<Dictionary<DeliveryMethod, EntangleState>>(new System.ArraySegment<byte>((byte[])operation.Parameters[2]));
                Dictionary<DeliveryMethod, EntangleState> compatibleState = new Dictionary<DeliveryMethod, EntangleState>();
                foreach (var _state in state.states)
                {
                    compatibleState.Add((DeliveryMethod)_state.Key, _state.Value);
                }
                netPlayer.States = compatibleState;
                netPlayer.GetNetProperty<FNVector3>(255,DeliveryMethod.Unreliable, (pos) => {

                    netPlayer.Position = pos;
                });
                foreach (var _state in compatibleState)
                {
                    _state.Value.BindOnValueChangeProperties();
                }
            }

            if (netPlayer.Position == default)
            {
                netPlayer.Position = new FNVector3();
                netPlayer.SetNetProperty<FNVector3>(255, netPlayer.Position, DeliveryMethod.Unreliable);
            }
            room.PlayerJoin(netPlayer);

        }
    }
}
