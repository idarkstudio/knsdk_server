using FigNet.Core;
using FigNetCommon;
using FigNet.KernNetz;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace KernNetzServer.Modules.Lobby.Operations
{
    internal class InstantiateEntityOperation
    {
        public Message GetOperation(uint networkId, EntityType entityType, short entityId, Dictionary<DeliveryMethod, EntangleState> states, uint ownerId)
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
                { 0, networkId },
                { 1, (byte)entityType },
                { 2, entityId },
                { 3, Default_Serializer.Serialize2<EntityDefaultState>(defaultState)}, // FN.Serializer.Serialize<Dictionary<DeliveryMethod, EntangleState>>(states)
                { 4, ownerId}
            };

            Message operation = new Message((ushort)OperationCode.InstantiateEntity, new OperationData(parameters));
            return operation;
        }

    }
}
