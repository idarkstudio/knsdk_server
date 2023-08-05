using System;
using System.IO;
using FigNet.Core;
using System.Linq;
using FigNetCommon;
using System.Reflection;
using System.Collections.Generic;
using KernNetzServer.Modules.Lobby.Handlers;

namespace KernNetzServer.Modules.Lobby
{
    internal sealed class KNLobby : IModule
    {
        private static uint s_roomIdCounter = 1;
        private static uint GenarateRoomId => s_roomIdCounter++;
        public Dictionary<uint, KNRoom> Rooms = new Dictionary<uint, KNRoom>();

        private Dictionary<uint, uint> playerRoomIdHashMap = new Dictionary<uint, uint>();
        private Dictionary<uint, PlayerInfo> PlayersInfo = new Dictionary<uint, PlayerInfo>();

        private readonly object roomLock = new object();
        private readonly object hashLock = new object();

        private readonly List<IHandler> handlers = new List<IHandler>() {
            new CreateRoomHandler(),
            new JoinRoomHandler(),
            new LeaveRoomHandler(),
            new GetRoomListHandler(),
            new OnEntityStateChangeHandler(),
            new InstantiateEntityHandler(),
            new DeleteEntityHandler(),
            new RoomEventHandler(),
            new RequestOwnershipHandler(),
            new ClearOwnershipHandler(),
            new RoomStateChangeHandler(),
            new OnAppKeyHandler()
        };

        private AppSecretKey SecretKey;

        private readonly Dictionary<ushort, string> AppKeys = new Dictionary<ushort, string>() {
            { 1, "hw3NufTCIQ83KUXnZKBpvlcherCeb4ZW" },
            { 2, "5e884090e5e80d3b84f8666ef3aa5b92" },
            { 3, "2d982b687b3aeca1f16eb541c33b714d" },
            { 4, "3d9a03fded6ea8d5da7e26a5422ee57e" },
            { 5, "55bc32b5bd4b886421eacb106b0b07c5" },
            { 6, "5d62d25d2709498099bf0448c1a8cfb3" },
            { 7, "dd765569d062c2a226f37062cca967da" },
            { 8, "2f72533eb79621d79c1e4018de1c59dc" },
            { 9, "f6ffd4490d7cc3361faa9416a61728f5" },
            { 10, "fcf015e4b60cf2463fbbf3552f685e5b" },
            { 11, "a23fdf9592701c33a05b04e719f1c5f6" },
            { 12, "6d6b24a5dac07d64991a0513bdaeba7d" },
            { 13, "5568bc0cdd47d4e66f53861f8531dcb9" },
            { 14, "60e62893c27fd7294ad6173f689299fd" },
            { 15, "8d817efd105695f299353d1ec3acc57c" },
            { 16, "a9148987c2447f10a6c0ba346fdb2843" },
            { 17, "5277b187c7c387ca0326216f18b69a55" },
            { 18, "06ca85f1ced4bcaf7456ffdc971b911b" },
            { 19, "5fbe158038bbec251ceaf272208a0686" },
            { 20, "48d868cbe80928edcdcb5d705a7a5304" }
        };

        private class PlayerInfo
        {
            public string AppId;
            public bool IsInRoom;
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

        public static ArraySegment<byte> Serialize<T>(object value)
        {
            //T item = (T)value;
            var bytes = FigNetCommon.Utils.Serializer.Serialize<T>(value);
            return bytes;
        }

        public AppSecretKey keys = null;
        private void Init()
        {
            LoadAppKeys();
        }

        private void LoadAppKeys()
        {
            try
            {
                string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "AppKeys.xml");
                string mapData = File.ReadAllText(path);
                SecretKey = mapData.DeserializeFromXml<AppSecretKey>();
            }
            catch (Exception ex)
            {
                FN.Logger.Exception(ex, ex.Message);
            }
        }

        public void OnPlayerConnected(IPeer peer)
        {
            lock (hashLock)
            {
                if (!PlayersInfo.ContainsKey(peer.Id))
                {
                    PlayersInfo.Add(peer.Id, new PlayerInfo());
                }
            }
        }

        public void OnPlayerLeftForceFully(IPeer peer)
        {
            try
            {
                lock (hashLock)
                {
                    if (playerRoomIdHashMap.ContainsKey(peer.Id))
                    {
                        var roomId = playerRoomIdHashMap[peer.Id];
                        var room = Rooms[roomId];
                        room.OnPlayerLeft(peer.Id);
                        playerRoomIdHashMap.Remove(peer.Id);
                    }

                    if (PlayersInfo.ContainsKey(peer.Id))
                    {
                        PlayersInfo.Remove(peer.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                FN.Logger.Exception(ex, ex.StackTrace);
            }
        }

        public bool AppKeyValidation(string key, uint peerId)
        {

            bool isValid = SecretKey.ContainsValue(key);

            lock (hashLock)
            {
                if (PlayersInfo.ContainsKey(peerId) && !string.IsNullOrEmpty(key))
                {
                    PlayersInfo[peerId].AppId = key;
                }
            }

            return isValid;
        }

        public uint GetRoomIdWherePlayerBelongs(uint playerId, bool logError = true)
        {
            lock (hashLock)
            {
                try
                {
                    if (!playerRoomIdHashMap.ContainsKey(playerId))
                    {
                        if (logError)
                            FN.Logger.Error($"playerId:: {playerId} not found in playerRoomHashMap");
                        return uint.MaxValue;
                    }
                    return playerRoomIdHashMap[playerId];
                }
                catch (Exception ex)
                {
                    FN.Logger.Exception(ex, ex.Message);
                    return uint.MaxValue;
                }
                
            }
        }

        public bool UpdatePlayerRoomIdHashMap(uint playerId, uint roomId, bool isAdd)
        {
            bool result = false;
            lock (hashLock)
            {
                if (isAdd)
                {
                    if (!playerRoomIdHashMap.ContainsKey(playerId))
                    {
                        playerRoomIdHashMap.Add(playerId, roomId);
                        result = true;
                    }
                }
                else
                {
                    if (playerRoomIdHashMap.ContainsKey(playerId))
                    {
                        playerRoomIdHashMap.Remove(playerId);
                        result = true;
                    }
                }
            }
            return result;
        }

        public KNRoom CreateRoom(string roomName, short maxPlayers, string password, uint peerId)
        {
            var newRoom = KNRoom.CreateRoom(GenarateRoomId, roomName, maxPlayers, password);

            lock (hashLock)
            {
                Rooms.Add(newRoom.RoomId, newRoom);

                FN.Logger.Info($"Active Rooms Count {Rooms.Count} : Room Create with ID {newRoom.RoomId}");
                newRoom.AppId = PlayersInfo[peerId].AppId;
            }

            newRoom.IsEmpty(() => {

                DeleteRoom(newRoom.RoomId);
            });

            return newRoom;
        }

        public bool DeleteRoom(uint roomId)
        {
            bool result = false;

            lock (roomLock)
            {
                if (Rooms.ContainsKey(roomId))
                {
                    result = true;
                    Rooms[roomId].OnDispose();
                    Rooms.Remove(roomId);

                    FN.Logger.Info($"{roomId} Disposed SucessFully : Active Rooms {Rooms.Count}");
                }
            }

            return result;
        }

        public List<RoomInfo> GetRoomsList(RoomQuery query, uint peerId)
        {
            List<RoomInfo> rooms = null;
            if (query == RoomQuery.All)
            {
                rooms = Rooms.Values.ToList().Where(p => p.AppId == PlayersInfo[peerId].AppId).Select(r => r.GetRoomInfo()).ToList();
            }
            else if (query == RoomQuery.Avaliable)
            {
                rooms = Rooms.Values.ToList().Where(p => p.AppId == PlayersInfo[peerId].AppId).Where(r => r.MaxPlayers != r.Players.Count).Select(r => r.GetRoomInfo()).ToList();
            }
            return rooms;
        }

        public void Load(IServer server)
        {
            FN.Logger.Info("FNELobby Module loaded successfully...");
            //ServiceLocator.Bind(typeof(FNELobby), this);
            RegisterHandlers();
            Init();
        }

        public void Process(float deltaTime)
        {
            foreach (var room in Rooms)
            {
                room.Value.Tick(deltaTime);
            }
        }

        public void UnLoad()
        {
            UnRegisterHandlers();
        }
    }
}
