using KernNetz;
using FigNet.Core;
using FigNetCommon;
using FigNet.KernNetz.Operations;

namespace FigNet.KernNetz
{
    public sealed class NetItem : NetworkEntity
    {
        public NetItem() : base()
        {
            EntityType = EntityType.Item;
        }


        public static NetItem CreateItem(EntityType entityType, short entityId, uint networkId)
        {
            NetItem entity = new NetItem
            {
                EntityType = entityType,
                EntityId = entityId,
                NetworkId = networkId
            };
            return entity;
        }

        public override bool RequestOwnership(bool withLock = false)
        {
            if (IsLocked) return false;
            IsMine = true;
            IsLocked = withLock;
            if (FN.Connections[0].IsConnected)
            {
                var requestOp = new RequestOwnershipOperation().GetOperation(KN.Room.RoomId, this.NetworkId, withLock);
                FN.Connections[0].SendMessage(requestOp, DeliveryMethod.Reliable);
            }

            return IsMine;
        }

        public override void ClearOwnership()
        {
            if (FN.Connections[0].IsConnected)
            {
                var clearOp = new ClearOwnershipOperation().GetOperation(KN.Room.RoomId, this.NetworkId);
                FN.Connections[0].SendMessage(clearOp, DeliveryMethod.Reliable);
            }
        }
    }
}
