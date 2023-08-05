using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace KernNetzServer.Modules.Lobby.Handlers
{
    public class GetRoomListHandler : IHandler
    {
        public ushort MsgId => (ushort)OperationCode.GetRoomList;
        public void HandleMessage(Message message, uint PeerId)
        {
            var sender = FN.PeerCollection.GetPeerByID(PeerId);
            var operation = message.Payload as OperationData;
            RoomQuery query = (RoomQuery)operation.Parameters[0];

            var lobby = ServiceLocator.GetService<IServer>().GetModule<KNLobby>();
            var rooms = lobby.GetRoomsList(query, PeerId);

            Dictionary<byte, object> parameters = new Dictionary<byte, object>
            {
                { 0, Default_Serializer.Serialize2<List<RoomInfo>>(rooms)}  // MessagePack.MessagePackSerializer.Serialize(rooms, MessagePack.Resolvers.ContractlessStandardResolver.Options)
            };

            var Op = new Message(message.Id, new OperationData(parameters));
            Op.callbackId = message.callbackId;

            FN.Server.SendMessage(sender, Op, DeliveryMethod.Reliable, 0);
        }
    }
}
