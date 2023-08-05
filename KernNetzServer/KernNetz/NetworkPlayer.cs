using KernNetz;
using FigNetCommon;
namespace FigNet.KernNetz
{
    public sealed class NetPlayer : NetworkEntity
    {
        public bool IsMasterClient;
        public int TeamId;

        public NetPlayer() : base()
        {
            EntityType = EntityType.Player;
        }

        public static NetPlayer CreatePlayer(uint networkId, float sycnRate = 20f, int teamId = -1, bool isMasterClient = false, bool isMine = false)
        {
            var netPlayer = new FigNet.KernNetz.NetPlayer()
            {
                NetworkId = networkId,
                TeamId = teamId,
                SyncRate = sycnRate, // 20 ticks per sec
                IsMasterClient = isMasterClient,
                IsMine = isMine,
                EntityType = EntityType.Player,
                OwnerId = networkId

            };
            return netPlayer;
        }
    }
}
