using FigNet.Core;
using FigNetCommon;

namespace KernNetzServer.Modules.Lobby.Handlers
{
    public class LeaveRoomHandler : IHandler
    {
        public ushort MsgId => (ushort)OperationCode.LeaveRoom;
        public void HandleMessage(Message message, uint PeerId)
        {
            var sender = FN.PeerCollection.GetPeerByID(PeerId);

            var lobby = ServiceLocator.GetService<IServer>().GetModule<KNLobby>();
            uint roomId = lobby.GetRoomIdWherePlayerBelongs(PeerId);
            bool roomToLeave = lobby.Rooms.TryGetValue(roomId, out KNRoom room);

            FN.Logger.Info($"<< LEAVE ROOM >> User: {PeerId} Leaving Room {roomId}");

            if (roomToLeave)
            {
                room.OnPlayerLeft(PeerId);
                lobby.UpdatePlayerRoomIdHashMap(PeerId, 0, false);

                if (sender != null)
                {
                    var Op = new Message(message.Id, null);
                    Op.callbackId = message.callbackId;
                    FN.Server.SendMessage(sender, Op, DeliveryMethod.Reliable, 0);
                }
            }
            else
            {
                FN.Logger.Warning($"ROOM NOT FOUND: peer {PeerId} is trying to  join room {roomId}");
            }
        }
    }
}
