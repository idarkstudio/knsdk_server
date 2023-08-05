using KernNetz;
using FigNet.Core;
using FigNetCommon;
using FigNet.KernNetz;
using System.Threading.Tasks;
using System.Collections.Generic;
using FigNet.KernNetz.Operations;
using System.Runtime.CompilerServices;
using KernNetzServer.Modules.Lobby.Operations;

namespace KernNetzServer.Modules.Lobby
{
    internal sealed class KNRoom
    {
        //    - roomId: string
        //    - roomName: string
        //    - clients: NetworkPlayer[]
        //    - maxClients: number
        //    - autoDispose: boolean
        //    - locked:  (match in progress)
        //    - IsPasswordProtected
        //    - state: int


        //    - OnDispose
        //    - SetMetaData
        //    - Disconnect
        //    - OnJoin: peer
        //    - OnLeave:peer
        //    - OnMessage:IMessage, Peer
        //    - send(client, message)
        //    - broadcast(type, message)
        //    - OnEventReceive

        public uint RoomId;
        public string RoomName;
        public short MaxPlayers;
        /// <summary>
        /// when room is empty auto close the room after X seconds
        /// </summary>
        public bool AutoDispose;
        /// <summary>
        /// if Password is null or empty room is not password protected
        /// </summary>
        public string Password;
        /// <summary>
        /// Room is locked, game has started new players are not allowed
        /// </summary>
        public bool IsLocked;
        /// <summary>
        /// State of the Room/Game, Wating for other player, Lobby, SettingUp, GameInProgress, MatchEnd etc
        /// </summary>
        public int State;

        public string AppId;

        public List<FNEPlayer> Players = new List<FNEPlayer>();
        public List<NetAgent> Agents = new List<NetAgent>();
        public List<NetItem> Items = new List<NetItem>();
        public List<NetPlayer> InactivePlayers = new List<NetPlayer>();

        public class FNEPlayer
        {
            public IPeer peer { get; set; }
            public NetPlayer netPlayer { get; set; }
        }

        private const int AUTO_DISPOSE_TIME = 30000; // 30 sec

        public const float SYNC_RATE = 1.0f / 30;
        public const float CHECK_MASTER_RATE = 21;

        private readonly object entitiesLock = new object();

        uint entityIdGen = 12000;
        private uint GenerateEntityId => entityIdGen++;

        public uint MasterClient = uint.MaxValue;
        private static byte ItemChannel = 2;
        private static byte AgentChannel = 1;
        private static byte PlayerChannel = 0;
        private byte PlayerBatchSize = 4;
        private byte NetworkEntityBatchSize = 8;

        public const byte MAX_CHANNEL_LIMIT = 8;
        public string RoomAuthToken { get; private set; }

        public KNRoom()
        {
            ItemChannel++;
            if (ItemChannel >= MAX_CHANNEL_LIMIT) ItemChannel = 0;
            AgentChannel++;
            if (AgentChannel >= MAX_CHANNEL_LIMIT) AgentChannel = 0;
            PlayerChannel++;
            if (PlayerChannel >= MAX_CHANNEL_LIMIT) PlayerChannel = 0;

            RoomAuthToken = System.Guid.NewGuid().ToString();
        }

        //public FNERoom()
        //{
        //    Timer = new Timer
        //    {
        //        Interval = (1.0 / SyncRate) * 1000
        //    };
        //    Timer.Elapsed += Timer_Elapsed;

        //    Timer.Start();
        //}


        //private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    lock (entitiesLock)
        //    {
        //        FlushStateDelta();
        //    }
        //}
        // todo create a timer class that runs based on simulate functions
        float timer = 0f, timer2 = 0;
        public void Tick(float deltaTime)
        {
            timer += deltaTime;
            if (timer > SYNC_RATE)
            {
                timer = 0;
                FlushStateDelta();
            }

            timer2 += deltaTime;
            if (timer2 > CHECK_MASTER_RATE)
            {
                timer2 = 0;
                CheckAndSetNewMaster();
                //TryDisposeIfEmpty();
            }
        }

        private void CheckAndSetNewMaster()
        {
            if (Players.Count > 0)
            {
                var _players = Players;
                uint newMaster = SelectMasterClientBasedOnLowPing();
                if (newMaster == MasterClient) return;
                for (int i = 0; i < _players.Count; i++)
                {
                    bool isMaster = newMaster == _players[i].netPlayer.NetworkId;
                    if (isMaster)
                    {
                        MasterClient = _players[i].netPlayer.NetworkId;
                    }
                    var masterOp = new MasterClientChangeOperation().GetOperation(_players[i].netPlayer.NetworkId, isMaster);
                    Send(_players[i].peer, masterOp, DeliveryMethod.Reliable);
                }
            }
        }

        private uint SelectMasterClientBasedOnLowPing()
        {
            int lowestPing = int.MaxValue;
            uint newMaster = Players[0].netPlayer.NetworkId;
            try
            {
                for (int i = 0; i < Players.Count; i++)
                {
                    var peer = FN.PeerCollection.GetPeerByID(Players[i].netPlayer.NetworkId);
                    if (peer == null) continue;

                    if (peer.Ping < lowestPing)
                    {
                        lowestPing = peer.Ping;
                    }
                }
            }
            catch (System.Exception ex)
            {
                FN.Logger.Exception(ex, ex.Message);
            }


            return newMaster;
        }

        private void UpdateEntitiesOwnershipOnPlayerLeft(uint playerId)
        {
            var items = Players.Count == 1 ? Items : Items?.FindAll(i => i.OwnerId == playerId);
            foreach (var item in items)
            {
                item.OwnerId = MasterClient;
                var requestOp = new RequestOwnershipOperation().GetOperation(MasterClient, item.NetworkId, false);
                SendToAll(requestOp, DeliveryMethod.Reliable);
            }

            foreach (var agent in Agents)
            {
                agent.OwnerId = MasterClient;
                var requestOp = new AgentOwnershipChangeOperation().GetOperation(MasterClient, agent.NetworkId);
                SendToAll(requestOp, DeliveryMethod.Reliable);
            }
        }


        ENStateFrameBatch _frameBatch = new ENStateFrameBatch()
        {
            ENStateFrames = new List<ENStateFrame>(),
            EntityType = EntityType.Player
        };

        public class ENStateFramebatchHelper
        {
            public bool IsDirty;
            public ENStateFrameBatch ENStateFrames = new ENStateFrameBatch() { ENStateFrames = new List<ENStateFrame>() };

            public void AddStateFrame(ENStateFrame frame, EntityType entityType)
            {
                IsDirty = true;
                ENStateFrames.ENStateFrames.Add(frame);
                ENStateFrames.EntityType = entityType;
            }

            public void Clear()
            {
                ENStateFrames.ENStateFrames.Clear();
                IsDirty = false;
            }
        }

        Dictionary<DeliveryMethod, ENStateFramebatchHelper> frameBatches = new Dictionary<DeliveryMethod, ENStateFramebatchHelper>()
        {
            { DeliveryMethod.Reliable, new ENStateFramebatchHelper() },
            { DeliveryMethod.ReliableUnordered, new ENStateFramebatchHelper() },
            { DeliveryMethod.Sequenced, new ENStateFramebatchHelper() },
            { DeliveryMethod.Unreliable, new ENStateFramebatchHelper() },
        };

        private void ClearBatchFrames()
        {
            foreach (var batch in frameBatches)
            {
                batch.Value.Clear();
            }
        }

        private void FlushStateDelta()
        {
            int batchCount = Players.Count / PlayerBatchSize;
            int additionalBatch = Players.Count % PlayerBatchSize;
            //frameBatch.RoomId = this.RoomId;
            ClearBatchFrames();
            #region PlayerBatches

            for (int i = 0; i < batchCount; i++)
            {
                for (int j = 0; j < ((i * PlayerBatchSize) + PlayerBatchSize); j++)
                {
                    foreach (var state in Players[j].netPlayer.States)
                    {
                        if (state.Value.IsDirty)
                        {
                            var delta = Players[j].netPlayer.GetStateDelta(state.Key);
                            ENStateFrame frame = new ENStateFrame()
                            {
                                NetworkEntityId = Players[j].netPlayer.NetworkId,
                                State = delta
                            };
                            frameBatches[(DeliveryMethod)delta.DeliveryMethod].AddStateFrame(frame, EntityType.Player);
                            state.Value.IsDirty = false;
                        }
                    }
                }

                // dispatch batch & 
                // clear batches
                foreach (var batch in frameBatches)
                {
                    if (batch.Value.IsDirty)
                    {
                        var op = EntityStateOperation.GetOperation(batch.Value.ENStateFrames);
                        this.SendToAll(op, batch.Key, PlayerChannel);
                    }
                }
                ClearBatchFrames();
            }


            // send additional batch
            for (int i = Players.Count - additionalBatch - 1; i < Players.Count; i++)
            {
                if (i < 0) continue;

                foreach (var state in Players[i].netPlayer.States)
                {
                    if (state.Value.IsDirty)
                    {
                        var delta = Players[i].netPlayer.GetStateDelta(state.Key);
                        ENStateFrame frame = new ENStateFrame()
                        {
                            NetworkEntityId = Players[i].netPlayer.NetworkId,
                            State = delta
                        };
                        frameBatches[(DeliveryMethod)delta.DeliveryMethod].AddStateFrame(frame, EntityType.Player);
                        state.Value.IsDirty = false;
                    }
                }
            }

            // dispatch batch & 
            // clear batches
            foreach (var batch in frameBatches)
            {
                if (batch.Value.IsDirty)
                {
                    var op = EntityStateOperation.GetOperation(batch.Value.ENStateFrames);
                    this.SendToAll(op, batch.Key, PlayerChannel);
                }
            }
            ClearBatchFrames();
            #endregion

            #region ItemBatches

            batchCount = Items.Count / NetworkEntityBatchSize;
            additionalBatch = Items.Count % NetworkEntityBatchSize;

            for (int i = 0; i < batchCount; i++)
            {
                for (int j = 0; j < ((i * NetworkEntityBatchSize) + NetworkEntityBatchSize); j++)
                {
                    foreach (var state in Items[j].States)
                    {
                        if (state.Value.IsDirty)
                        {
                            var delta = Items[j].GetStateDelta(state.Key);
                            ENStateFrame frame = new ENStateFrame()
                            {
                                NetworkEntityId = Items[j].NetworkId,
                                State = delta
                            };
                            frameBatches[(DeliveryMethod)delta.DeliveryMethod].AddStateFrame(frame, EntityType.Item);
                            state.Value.IsDirty = false;
                        }
                    }


                }
                // dispatch batch & 
                // clear batches
                foreach (var batch in frameBatches)
                {
                    if (batch.Value.IsDirty)
                    {
                        var op = EntityStateOperation.GetOperation(batch.Value.ENStateFrames);
                        this.SendToAll(op, batch.Key, ItemChannel);
                    }

                }
                ClearBatchFrames();
            }


            // send additional batch
            for (int i = Items.Count - additionalBatch - 1; i < Items.Count; i++)
            {
                if (i < 0) continue;

                foreach (var state in Items[i].States)
                {
                    if (state.Value.IsDirty)
                    {
                        var delta = Items[i].GetStateDelta(state.Key);
                        ENStateFrame frame = new ENStateFrame()
                        {
                            NetworkEntityId = Items[i].NetworkId,
                            State = delta
                        };

                        frameBatches[(DeliveryMethod)delta.DeliveryMethod].AddStateFrame(frame, EntityType.Item);
                        state.Value.IsDirty = false;
                    }
                }
            }

            // dispatch batch & 
            // clear batches
            foreach (var batch in frameBatches)
            {
                if (batch.Value.IsDirty)
                {
                    var op = EntityStateOperation.GetOperation(batch.Value.ENStateFrames);
                    this.SendToAll(op, batch.Key, ItemChannel);
                }

            }
            ClearBatchFrames();
            #endregion

            #region AgentBatches

            batchCount = Agents.Count / NetworkEntityBatchSize;
            additionalBatch = Agents.Count % NetworkEntityBatchSize;

            for (int i = 0; i < batchCount; i++)
            {
                for (int j = 0; j < ((i * NetworkEntityBatchSize) + NetworkEntityBatchSize); j++)
                {

                    foreach (var state in Agents[j].States)
                    {
                        if (state.Value.IsDirty)
                        {
                            var delta = Agents[j].GetStateDelta(state.Key);
                            ENStateFrame frame = new ENStateFrame()
                            {
                                NetworkEntityId = Agents[j].NetworkId,
                                State = delta
                            };
                            frameBatches[(DeliveryMethod)delta.DeliveryMethod].AddStateFrame(frame, EntityType.Agent);
                            state.Value.IsDirty = false;
                        }
                    }

                }
                // dispatch batch & 
                // clear batches
                foreach (var batch in frameBatches)
                {
                    if (batch.Value.IsDirty)
                    {
                        var op = EntityStateOperation.GetOperation(batch.Value.ENStateFrames);
                        this.SendToAll(op, batch.Key, AgentChannel);
                    }

                }
                ClearBatchFrames();
            }


            // send additional batch
            for (int i = Agents.Count - additionalBatch - 1; i < Agents.Count; i++)
            {
                if (i < 0) continue;

                foreach (var state in Agents[i].States)
                {
                    if (state.Value.IsDirty)
                    {
                        var delta = Agents[i].GetStateDelta(state.Key);
                        ENStateFrame frame = new ENStateFrame()
                        {
                            NetworkEntityId = Agents[i].NetworkId,
                            State = delta
                        };
                        frameBatches[(DeliveryMethod)delta.DeliveryMethod].AddStateFrame(frame, EntityType.Agent);
                        state.Value.IsDirty = false;
                    }
                }
            }
            // dispatch batch & 
            // clear batches
            foreach (var batch in frameBatches)
            {
                if (batch.Value.IsDirty)
                {
                    var op = EntityStateOperation.GetOperation(batch.Value.ENStateFrames);
                    this.SendToAll(op, batch.Key, AgentChannel);
                }
            }
            ClearBatchFrames();
            #endregion

        }

        public void UpdateRoomState(int state, bool isLock)
        {
            State = state;
            IsLocked = isLock;
        }

        // TODO: check integrity
        public bool IsInRoom(uint peerId) => Players.Find(p => p.netPlayer.NetworkId == peerId) != null;

        public static KNRoom CreateRoom(uint roomId, string roomName, short maxPlayers, string password, bool autoDispose = true)
        {
            KNRoom newRoom = new KNRoom()
            {
                RoomId = roomId,
                RoomName = roomName,
                MaxPlayers = maxPlayers,
                Password = password,
                AutoDispose = autoDispose
            };

            return newRoom;
        }

        public RoomInfo GetRoomInfo()
        {
            RoomInfo info = new RoomInfo()
            {
                Id = this.RoomId,
                RoomName = this.RoomName,
                ActivePlayer = this.Players.Count,
                MaxPlayer = this.MaxPlayers,
                State = this.State
            };

            return info;
        }

        public async void IsEmpty(System.Action delete)
        {
            await Task.Delay(AUTO_DISPOSE_TIME);
            bool IsEmpty = Players.Count < 1;
            if (IsEmpty)
            {
                delete?.Invoke();
                //Timer.Stop();
            }
        }

        private void SendInstantiateEntities(IPeer peer)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var op = new InstantiateEntityOperation().GetOperation(Items[i].NetworkId, Items[i].EntityType, Items[i].EntityId, Items[i].States, Items[i].OwnerId);
                Send(peer, op, DeliveryMethod.Reliable, ItemChannel);
                //FN.Logger.Warning($" <<SPAWN ITEM>> {Items[i].NetworkId} for USer {peerId}");
            }

            for (int i = 0; i < Agents.Count; i++)
            {
                var op = new InstantiateEntityOperation().GetOperation(Agents[i].NetworkId, Agents[i].EntityType, Agents[i].EntityId, Agents[i].States, Agents[i].OwnerId);
                Send(peer, op, DeliveryMethod.Reliable, ItemChannel);

                //FN.Logger.Warning($" <<SPAWN AGENT>> {Agents[i].NetworkId} for USer {peer.Id}");
            }
        }

        public RoomResponseCode OnPlayerReJoin(uint peerId, uint previousPeerId)
        {
            try
            {
                var peer = FN.PeerCollection.GetPeerByID(peerId);
                NetPlayer networkPlayer = InactivePlayers.Find(p => p.NetworkId == previousPeerId);

                if(networkPlayer == null) Players.Find(p => p.netPlayer.NetworkId == previousPeerId);

                if(networkPlayer == null) return RoomResponseCode.Failure;

                var op = new PreRoomStateReceivedOperation().GetOperation();
                Send(peer, op, DeliveryMethod.Reliable, 0);

                var playerJoinOp = new PlayerJoinRoomOperation().GetOperation(peerId, true, networkPlayer.TeamId);
                Send(peer, playerJoinOp, DeliveryMethod.Reliable);

                for (int i = 0; i < Players.Count; i++)
                {
                    var _playerJoinOp = new PlayerJoinRoomOperation().GetOperation(Players[i].netPlayer.NetworkId, false, networkPlayer.TeamId, Players[i].netPlayer.States);
                    Send(peer, _playerJoinOp, DeliveryMethod.Reliable);
                }

                var __playerJoinOp = new PlayerJoinRoomOperation().GetOperation(peerId, false, networkPlayer.TeamId, networkPlayer.States);

                for (int i = 0; i < Players.Count; i++)
                {
                    Send(Players[i].netPlayer.NetworkId, __playerJoinOp, DeliveryMethod.Reliable);
                }

                Players.Add(new FNEPlayer()
                {
                    peer = FN.PeerCollection.GetPeerByID(peerId),
                    netPlayer = networkPlayer

                });

                InactivePlayers.Remove(networkPlayer);

                networkPlayer.NetworkId = peerId;
                networkPlayer.OwnerId = peerId;
                CheckAndSetNewMaster();

                SendRoomStateToPlayer(peer);
                // here spawn agents & items to new player
                lock (entitiesLock)
                {
                    SendInstantiateEntities(peer);
                    if (Players.Count == 1) UpdateEntitiesOwnershipOnPlayerLeft(peerId);
                }

                var _op = new PostRoomStateReceivedOperation().GetOperation();
                Send(peer, _op, DeliveryMethod.Reliable, 0);

                FN.Logger.Info($"User {peerId} ReJoined Room {RoomId} : users count {Players.Count}");
            }
            catch (System.Exception ex)
            {
                FN.Logger.Exception(ex, ex.ToString());
                return RoomResponseCode.Failure;
            }

            return RoomResponseCode.Sucess;
        }

        public RoomResponseCode OnPlayerJoin(uint peerId, NetPlayer networkPlayer)
        {
            // tell new player you entered room {send}
            // tell new player to spawn existing players in room {multiple send}
            // tell existing players to spawn new player { literate : send }

            var peer = FN.PeerCollection.GetPeerByID(peerId);

            networkPlayer.Position = new FNVector3();
            networkPlayer.SetNetProperty<FNVector3>(255, networkPlayer.Position, DeliveryMethod.Unreliable);

            var op = new PreRoomStateReceivedOperation().GetOperation();
            Send(peer, op, DeliveryMethod.Reliable, 0);

            var playerJoinOp = new PlayerJoinRoomOperation().GetOperation(peerId, true, networkPlayer.TeamId);
            Send(peer, playerJoinOp, DeliveryMethod.Reliable);

            for (int i = 0; i < Players.Count; i++)
            {
                var _playerJoinOp = new PlayerJoinRoomOperation().GetOperation(Players[i].netPlayer.NetworkId, false, networkPlayer.TeamId, Players[i].netPlayer.States);
                Send(peer, _playerJoinOp, DeliveryMethod.Reliable);
            }

            var __playerJoinOp = new PlayerJoinRoomOperation().GetOperation(peerId, false, networkPlayer.TeamId, networkPlayer.States);

            for (int i = 0; i < Players.Count; i++)
            {
                Send(Players[i].netPlayer.NetworkId, __playerJoinOp, DeliveryMethod.Reliable);
            }


            Players.Add(new FNEPlayer()
            {
                peer = FN.PeerCollection.GetPeerByID(peerId),
                netPlayer = networkPlayer

            });

            CheckAndSetNewMaster();

            SendRoomStateToPlayer(peer);
            // here spawn agents & items to new player
            lock (entitiesLock)
            {
                SendInstantiateEntities(peer);
                if (Players.Count == 1) UpdateEntitiesOwnershipOnPlayerLeft(peerId);
            }

            var _op = new PostRoomStateReceivedOperation().GetOperation();
            Send(peer, _op, DeliveryMethod.Reliable, 0);

            FN.Logger.Info($"User {peerId} Joined Room {RoomId} : users count {Players.Count}");
            return RoomResponseCode.Sucess;
        }

        public void OnPlayerLeft(uint peerId)
        {
            var toRemove = Players.Find(p => p.netPlayer.NetworkId == peerId);
            if (toRemove != null)
            {
                InactivePlayers.Add(toRemove.netPlayer);
                Players.Remove(toRemove);
                FN.Logger.Info($"<< PLAYER LEFT ROOM >> User: {peerId} : RoomID {RoomId} | {RoomName}  PC : {Players.Count} ");
            }

            var lobby = ServiceLocator.GetService<IServer>().GetModule<KNLobby>();
            lobby.UpdatePlayerRoomIdHashMap(peerId, 0, false);

            CheckAndSetNewMaster();

            var pLeftOp = new PlayerJLeftRoomOperation().GetOperation(peerId);
            SendToAll(pLeftOp, DeliveryMethod.Reliable);

            if (Players.Count < 1)
            {
                lobby.DeleteRoom(RoomId);
            }
            else
            {
                UpdateEntitiesOwnershipOnPlayerLeft(peerId);
            }
        }

        private void TryDisposeIfEmpty()
        {
            if (Players.Count < 1)
            {
                var lobby = ServiceLocator.GetService<IServer>().GetModule<KNLobby>();
                lobby.DeleteRoom(RoomId);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NetPlayer FindPlayer(uint networkId, uint sender)
        {
            NetPlayer player = null;
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].netPlayer.NetworkId == networkId && Players[i].netPlayer.OwnerId == sender)
                {
                    player = Players[i].netPlayer;
                    break;
                }
            }
            return player;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NetItem FindItem(uint networkId, uint sender)
        {
            NetItem item = null;
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].NetworkId == networkId && Items[i].OwnerId == sender)
                {
                    item = Items[i];
                    break;
                }
            }
            return item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NetAgent FindAgent(uint networkId, uint sender)
        {
            NetAgent agent = null;
            for (int i = 0; i < Agents.Count; i++)
            {
                if (Agents[i].NetworkId == networkId && Agents[i].OwnerId == sender)
                {
                    agent = Agents[i];
                    break;
                }
            }
            return agent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnEntityStateChange(uint sender, EntityType type, uint networkId, EntangleState state)
        {

            switch (type)
            {
                case EntityType.Player:
                    FindPlayer(networkId, sender)?.ApplyStateDelta(state);
                    //Players.Find(p => p.NetworkId == entityId && p.OwnerId == sender)?.ApplyStateDelta(state);
                    break;
                case EntityType.Agent:
                    FindAgent(networkId, sender)?.ApplyStateDelta(state);
                    //Agents.Find(p => p.NetworkId == entityId && p.OwnerId == sender)?.ApplyStateDelta(state);
                    break;
                case EntityType.Item:
                    FindItem(networkId, sender)?.ApplyStateDelta(state);
                    //Items.Find(p => p.NetworkId == entityId && p.OwnerId == sender)?.ApplyStateDelta(state);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(uint peerId, Message message, DeliveryMethod method, byte channelId = 0)
        {
            var sender = FN.PeerCollection.GetPeerByID(peerId);
            FN.Server.SendMessage(sender, message, method, channelId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(IPeer peer, Message message, DeliveryMethod method, byte channelId = 0)
        {
            FN.Server.SendMessage(peer, message, method, channelId);
        }

        List<IPeer> _peers = new List<IPeer>();
        public void SendToAll(Message message, DeliveryMethod method, byte channelId = 0, uint peerToExclude = uint.MaxValue)
        {
            _peers.Clear();
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].netPlayer.NetworkId == peerToExclude) continue;
                var sender = Players[i].peer; 
                if (sender != null)
                {
                    _peers.Add(sender);
                }
            }
            FN.Server.SendMessage(_peers, message, method, channelId);
        }


        public void OnEventReceive() { }
        public void Disconnect() { }
        public void OnDispose()
        {
            Items.Clear();
            Agents.Clear();
            Players.Clear();
            //Timer.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SendRoomStateToPlayer(IPeer player)
        {
            var roomSate = new RoomStateChangeOperation().GetOperation(this.RoomId, State, IsLocked);
            Send(player, roomSate, DeliveryMethod.Reliable);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool OnClearOwnership(uint ownerId, uint entityId)
        {
            bool sucess = false;
            var networkItem = Items.Find(i => i.NetworkId == entityId);
            if (networkItem != null)
            {
                if (networkItem.OwnerId == ownerId)
                {
                    //networkItem.OwnerId = uint.MaxValue;
                    networkItem.IsLocked = false;
                    sucess = true;
                }
            }
            return sucess;
        }

        public bool OnRequestOwnership(uint ownerId, uint entityId, bool isLock)
        {
            bool sucess = false;
            var networkItem = Items.Find(i => i.NetworkId == entityId);
            if (networkItem != null)
            {
                if (!networkItem.IsLocked)
                {
                    networkItem.OwnerId = ownerId;
                    networkItem.IsLocked = isLock;
                    sucess = true;
                }
            }
            return sucess;
        }

        private uint lastCreatedAgentID = 0;
        public void SendAgentOwnerToAll()
        {
            var agentOwnerOp = new AgentOwnershipChangeOperation().GetOperation(MasterClient, lastCreatedAgentID);
            SendToAll(agentOwnerOp, DeliveryMethod.Reliable);
            FN.Logger.Info($"Master {MasterClient} : Agent ID {lastCreatedAgentID} ");
        }

        public uint OnInstantiateEntity(uint sender, short entityId, EntityType entityType, System.Numerics.Vector3 position)
        {
            uint networkId = 0;
            switch (entityType)
            {
                case EntityType.Player:
                    FN.Logger.Error($"{entityType} creation is not supported via Instantiate Function");
                    break;
                case EntityType.Agent:

                    networkId = GenerateEntityId;
                    var entity = NetAgent.CreateAgent(entityType, entityId, networkId);
                    entity.OwnerId = MasterClient;
                    lastCreatedAgentID = networkId;

                    entity.Position = new FNVector3();
                    entity.Position.Value = new FNVec3(position.X, position.Y, position.Z);
                    entity.SetNetProperty<FNVector3>(255, entity.Position, DeliveryMethod.Unreliable);

                    Agents.Add(entity);
                    break;
                case EntityType.Item:
                    networkId = GenerateEntityId;
                    var _entity = NetItem.CreateItem(entityType, entityId, networkId);
                    _entity.OwnerId = sender;

                    _entity.Position = new FNVector3();
                    _entity.Position.Value = new FNVec3(position.X, position.Y, position.Z);
                    _entity.SetNetProperty<FNVector3>(255, _entity.Position, DeliveryMethod.Unreliable);

                    Items.Add(_entity);
                    break;
            }
            return networkId;
            // create Item add to dict & broadcast item to players
        }

        public bool OnDeleteEntity(uint networkId, EntityType entityType)
        {
            bool result = false;
            switch (entityType)
            {
                case EntityType.Player:
                    FN.Logger.Error($"{entityType} deletion is not supported via DeleteEntity Function");
                    break;
                case EntityType.Agent:
                    var agent = Agents.Find(a => a.NetworkId == networkId);
                    if (agent != null)
                    {
                        Agents.Remove(agent);
                        result = true;
                    }
                    break;
                case EntityType.Item:
                    var item = Items.Find(i => i.NetworkId == networkId);
                    if (item != null)
                    {
                        Items.Remove(item);
                        result = true;
                    }
                    break;
            }
            return result;
        }

    }
}
