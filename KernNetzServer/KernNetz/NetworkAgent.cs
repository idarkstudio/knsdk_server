using KernNetz;
using FigNetCommon;

namespace FigNet.KernNetz
{
    public sealed class NetAgent : NetworkEntity
    {
        public NetAgent() : base()
        {
            EntityType = EntityType.Agent;
        }

        public static NetAgent CreateAgent(EntityType entityType, short entityId, uint networkId)
        {
            NetAgent entity = new NetAgent
            {
                EntityType = entityType,
                EntityId = entityId,
                NetworkId = networkId
            };
            return entity;
        }

        public void UpdateOwner()
        {
            this.IsMine = KN.IsMasterClient;
        }
    }
}
