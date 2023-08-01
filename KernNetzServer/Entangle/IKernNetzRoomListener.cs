using FigNetCommon;

namespace FigNet.KernNetz
{
    public interface IKernNetzRoomListener
    {
        void OnRoomCreated();
        void OnRoomDispose();
        void OnRoomStateChange(int status);
        void OnPlayerJoined(NetPlayer player);
        void OnPlayerLeft(NetPlayer player);
        void OnItemCreated(NetItem item, System.Numerics.Vector3 position, System.Numerics.Vector3 rotation, System.Numerics.Vector3 scale);
        void OnItemDeleted(NetItem item);
        void OnAgentCreated(NetAgent agent, System.Numerics.Vector3 position, System.Numerics.Vector3 rotation, System.Numerics.Vector3 scale);
        void OnAgentDeleted(NetAgent agent);
        void OnEventReceived(uint sender, RoomEventData eventData);

    }
}
