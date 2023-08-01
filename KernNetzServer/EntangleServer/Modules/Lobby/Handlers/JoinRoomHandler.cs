using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace KernNetzServer.Modules.Lobby.Handlers
{
    public class JoinRoomHandler : IHandler
    {
        public ushort MsgId => (ushort)OperationCode.JoinRoom;
        public void HandleMessage(Message message, uint PeerId)
        {
            var sender = FN.PeerCollection.GetPeerByID(PeerId);
            var operation = message.Payload as OperationData;

            uint roomId = (uint)operation.Parameters[0];
            string password = operation.Parameters[1] as string;
            int teamId = 0;
            uint previousId = default;
            bool isRejoin = false;
            string roomAuthToken = "";

            if (operation.Parameters.ContainsKey(2))
            {
                teamId = (int)operation.Parameters[2];
            }

            if (operation.Parameters.ContainsKey(3))
            {
                isRejoin = (bool)operation.Parameters[3];
            }

            if (operation.Parameters.ContainsKey(4))
            {
                roomAuthToken = operation.Parameters[4] as string;
            }

            if (operation.Parameters.ContainsKey(5))
            {
                previousId = (uint)operation.Parameters[5];
            }

            FN.Logger.Info($"<< JOIN ROOM >> UserId: {PeerId} old peerId {previousId} teamId: {teamId}  roomId: {roomId}");

            var lobby = ServiceLocator.GetService<IServer>().GetModule<KNLobby>();

            uint alreadyInARoom = lobby.GetRoomIdWherePlayerBelongs(PeerId, false);
            
            bool roomToJoin = false;

            KNRoom room = default;

            if (alreadyInARoom == uint.MaxValue)
            {
                roomToJoin = lobby.Rooms.TryGetValue(roomId, out room);
            }

            if (roomToJoin)
            {
                if ((room.Password == password && !room.IsLocked && room.MaxPlayers != room.Players.Count) || (isRejoin && roomAuthToken == room.RoomAuthToken))
                {
                    Dictionary<byte, object> parameters = new Dictionary<byte, object>
                    {
                        { 0, RoomResponseCode.Sucess },
                        { 1, PeerId },
                        { 2, room.RoomAuthToken}
                    };

                    var Op = new Message(message.Id, new OperationData(parameters));
                    Op.callbackId = message.callbackId;
                    FN.Server.SendMessage(sender, Op, DeliveryMethod.Reliable, 0);

                    if (isRejoin)
                    {
                        var status = room.OnPlayerReJoin(PeerId, previousId);
                        FN.Logger.Info($"Rejoin Status: {status} peerId: {PeerId}->previousId {previousId}");
                    }
                    else
                    {
                        // todo: send TeamId & SyncRate from client side
                        var netPlayer = FigNet.KernNetz.NetPlayer.CreatePlayer(PeerId, teamId: teamId);
                        netPlayer.IsMasterClient = room.Players.Count < 1;
                        room.OnPlayerJoin(PeerId, netPlayer);
                    }

                    lobby.UpdatePlayerRoomIdHashMap(PeerId, roomId, true);
                    
                }
                else
                {
                    RoomResponseCode reason = RoomResponseCode.Failure;
                    if (room.Password != password)
                    {
                        reason = RoomResponseCode.InvalidPassword;
                    }
                    else if (room.IsLocked)
                    {
                        reason = RoomResponseCode.RoomLocked;
                    }
                    else if (room.MaxPlayers == room.Players.Count)
                    {
                        reason = RoomResponseCode.RoomFull;
                    }

                    Dictionary<byte, object> parameters = new Dictionary<byte, object>
                    {
                        { 0, reason },
                        { 1, PeerId }
                    };

                    var Op = new Message(message.Id, new OperationData(parameters));
                    Op.callbackId = message.callbackId;

                    FN.Server.SendMessage(sender, Op, DeliveryMethod.Reliable, 0);
                }
            }
            else
            {
                // room not found
                RoomResponseCode reason = RoomResponseCode.Failure;

                if (alreadyInARoom != uint.MaxValue)
                {
                    reason = RoomResponseCode.AlreadyInRoom;

                    FN.Logger.Warning($"User {PeerId} is already present in a room {alreadyInARoom}");
                }
                else
                {
                    FN.Logger.Warning($"ROOM NOT FOUND: peer {PeerId} is trying to  join room {roomId}");
                }

                Dictionary<byte, object> parameters = new Dictionary<byte, object>
                    {
                        { 0, reason },
                        { 1, PeerId }
                    };

                var Op = new Message(message.Id, new OperationData(parameters));
                Op.callbackId = message.callbackId;

                FN.Server.SendMessage(sender, Op, DeliveryMethod.Reliable, 0);

            }
        }
    }
}
