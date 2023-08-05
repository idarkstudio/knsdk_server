using FigNet.Core;
using FigNetCommon;
using FigNet.KernNetz;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace KernNetzServer.Modules.Lobby.Operations
{
    internal class PlayerJoinRoomOperation
    {
        public Message GetOperation(uint playerId, bool isMine, int teamId, Dictionary<DeliveryMethod, EntangleState> states = null)
        {
            EntityDefaultState defaultState = new EntityDefaultState() { states = new List<KeyValuePair<byte, EntangleState>>() };
            if (states != null)
            {
                foreach (var item in states)
                {
                    defaultState.states.Add(new KeyValuePair<byte, EntangleState>((byte)item.Key, item.Value));
                }
            }

            Dictionary<byte, object> parameters = new Dictionary<byte, object>
            {
                { 0, isMine },
                { 1, playerId },
                { 2, teamId},
                { 3, Default_Serializer.Serialize2<EntityDefaultState>(defaultState)}  //  FN.Serializer.Serialize<Dictionary<DeliveryMethod, EntangleState>>(states)
            };

            Message operation = new Message((ushort)OperationCode.OnPlayerJoinRoom, new OperationData(parameters));
            return operation;
        }

    }
}
