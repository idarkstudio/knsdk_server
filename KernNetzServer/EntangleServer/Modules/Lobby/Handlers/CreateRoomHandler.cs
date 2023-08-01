using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace KernNetzServer.Modules.Lobby.Handlers
{
    public class CreateRoomHandler : IHandler
    {
        public ushort MsgId => (ushort)OperationCode.CreateRoom;
        public void HandleMessage(Message message, uint PeerId)
        {
            var sender = FN.PeerCollection.GetPeerByID(PeerId);
            var operation = message.Payload as OperationData;
            string roomName = operation.Parameters[0] as string;
            short maxPlayers = (short)operation.Parameters[1];
            string password = operation.Parameters[2] as string;

            var lobby = ServiceLocator.GetService<IServer>().GetModule<KNLobby>();
            var room = lobby.CreateRoom(roomName, maxPlayers, password, PeerId);

            Dictionary<byte, object> parameters = new Dictionary<byte, object>
            {
                { 0, room.RoomId }  // roomId
            };

            var Op = new Message(message.Id, new OperationData(parameters));
            Op.callbackId = message.callbackId;

            FN.Server.SendMessage(sender, Op, DeliveryMethod.Reliable, 0);
        }
    }
}
