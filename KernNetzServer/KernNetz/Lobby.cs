using System;
using FigNet.Core;
using FigNetCommon;
using KernNetz.Handlers;
using System.Collections.Generic;
using FigNet.KernNetz.Operations;
using FigNetCommon.Data;

namespace FigNet.KernNetz
{
    public class Lobby
    {
        // get all rooms
        // create room or Join
        // Join room
        // DeleteRoom

        private bool init = false;

        readonly List<IHandler> handlers = new List<IHandler>() {
            new PlayerJoinRoomHandler(),
            new PlayerLeftRoomHandler(),
            new OnEntityStateChangeHandler(),
            new MasterClientChangeHandler(),
            new InstantiateEntityHandler(),
            new DeleteEntityHandler(),
            new RoomEventHandler(),
            new RequestOwnershipHandler(),
            new ClearOwnershipHandler(),
            new AgentOwnershipChangeHandler(),
            new RoomStateChangeHandler()
        };

        public void Initialize()
        {
            if (init) return;
            init = true;
            RegisterHandlers();
        }



        private void RegisterHandlers()
        {
            handlers.ForEach(handle => FN.HandlerCollection.RegisterHandler(handle));
        }
        private void UnRegisterHandlers()
        {
            handlers.ForEach(handle => FN.HandlerCollection.UnRegisterHandler(handle));
            handlers.Clear();
        }

        public static void CreateRoom(string roomName, short maxPlayers, string password, Action<bool, uint> callBack)
        {
            if (FN.Connections[0].IsConnected)
            {
                var createRoomOp = new CreateRoomOperation();
                var op = createRoomOp.GetOperation(roomName, maxPlayers, password);

                FN.Connections[0].SendMessage(op, (response) => {
                    uint RoomId = (uint)((response.Payload as OperationData).Parameters[0]);
                    //Entangle.Room.RoomId = RoomId;
                    //Entangle.Room.RoomName = roomName;
                    //Entangle.Room.MaxPlayer = maxPlayers;
                    KN.Room.OnRoomCreated();
                    callBack?.Invoke(true, RoomId);
                });
            }
            else
            {
                FN.Logger.Error("Client is not connected to Server");
                callBack?.Invoke(false, 0);
            }

        }

        public static void GetRooms(RoomQuery query, Action<List<RoomInfo>> callBack)
        {
            if (FN.Connections[0].IsConnected)
            {
                var getRoomOp = new GetRoomListOperation();
                var op = getRoomOp.GetOperation(query);

                FN.Connections[0].SendMessage(op, (response) => {

                    var data = (byte[])((response.Payload as OperationData).Parameters[0]);
                    var rooms = Default_Serializer.Deserialize2<List<RoomInfo>>(data); // MessagePack.MessagePackSerializer.Deserialize<List<RoomInfo>>(data, MessagePack.Resolvers.ContractlessStandardResolver.Options);

                    callBack?.Invoke(rooms);
                    //FN.Logger.Error($"getRooms SUCESS {rooms?.Count}");
                });
            }
            else
            {
                FN.Logger.Error("Client is not connected to Server");
            }
        }

        public static void JoinRoom(uint roomId, string password, Action<RoomResponseCode> onRoomJoin)
        {
            RoomResponseCode roomResponse = RoomResponseCode.Failure;
            if (FN.Connections[0].IsConnected)
            {
                var joinRoomOp = new JoinRoomOperation();
                var op = joinRoomOp.GetOperation(roomId, password);

                FN.Connections[0].SendMessage(op, (response) => {

                    var _opeartion = response.Payload as OperationData;
                    roomResponse = (RoomResponseCode)_opeartion.Parameters[0];
                    KN.Room.MyPlayerId = (uint)_opeartion.Parameters[1];
                    KN.Room.RoomId = roomId;
                    KN.Room.Password = password;
                    if (_opeartion.Parameters.ContainsKey(2))
                    {
                        KN.Room.RoomAuthToken = _opeartion.Parameters[2] as string;
                    }

                    onRoomJoin?.Invoke(roomResponse);
                });
            }
            else
            {
                FN.Logger.Error("Client is not connected to Server");
                onRoomJoin?.Invoke(RoomResponseCode.Failure);
            }
        }

        public static void ReJoinRoom(uint roomId, string password, bool isRejoining = false, string roomAuthToken = "", Action<RoomResponseCode> onRoomJoin = default)
        {
            RoomResponseCode roomResponse = RoomResponseCode.Failure;
            if (FN.Connections[0].IsConnected)
            {
                var joinRoomOp = new JoinRoomOperation();
                var op = joinRoomOp.GetOperation(roomId, password, isRejoining, roomAuthToken);

                FN.Connections[0].SendMessage(op, (response) => {

                    var _operation = response.Payload as OperationData;
                    roomResponse = (RoomResponseCode)(_operation.Parameters[0]);
                    KN.Room.MyPlayerId = (uint)_operation.Parameters[1];
                    KN.Room.RoomId = roomId;
                    KN.Room.Password = password;
                    onRoomJoin?.Invoke(roomResponse);
                });
            }
            else
            {
                FN.Logger.Error("Client is not connected to Server");
                onRoomJoin?.Invoke(RoomResponseCode.Failure);
            }
        }

        public static void LeaveRoom(Action<RoomResponseCode> onRoomLeft)
        {
            if (FN.Connections[0].IsConnected)
            {
                var operation = new Message((ushort)OperationCode.LeaveRoom, new OperationData(null));

                FN.Connections[0].SendMessage(operation, (response) => {

                    FN.Logger.Info($"On Room {KN.Room.RoomId} Leave {response}");
                    onRoomLeft?.Invoke(RoomResponseCode.Sucess);
                    KN.Room.OnRoomDisposed();
                });
            }
            else
            {
                FN.Logger.Error("Client is not connected to Server");
            }
        }
    }
}
