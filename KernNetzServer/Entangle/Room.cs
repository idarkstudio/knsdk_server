using System;
using KernNetz;
using FigNet.Core;
using System.Numerics;
using System.Collections.Generic;
using FigNetCommon;
using FigNet.KernNetz.Operations;

namespace FigNet.KernNetz
{
	public class Room
	{
		//- roomId: string
		//- roomName: string
		//- clients: Client[]
		//- maxClients: number
		//- patchRate: number
		//- autoDispose: boolean
		//- locked:  (pass protected)
		//- state: T

		//- OnCreate
		//- OnDispose
		//- SetMetaData
		//- Disconnect
		//- OnJoin: peer
		//- OnLeave:peer
		//- OnMessage:IMessage, Peer
		//- send(client, message)
		//- broadcast(type, message)
		//- setSimulationInterval 
		//- setPatchRate

		public uint RoomId { get; set; }
		public string Password { get; set; }
		public string RoomAuthToken { get; set; }
		public string RoomName { get; set; }
		public int MaxPlayer { get; set; }
		public uint MyPlayerId { get; set; }
		public bool IsLocked { get; set; }
		public int RoomState { get; set; }
		public static event Action OnRoomCreate;
		public static event Action OnRoomDispose;
		public static event Action<NetPlayer> OnPlayerJoined;
		public static event Action<NetPlayer> OnPlayerLeft;
		public static event Action<NetworkEntity, Vector3, Vector3, Vector3> OnEntityCreated;
		public static event Action<NetworkEntity> OnEntityDeleted;
		public static event Action<uint, RoomEventData> OnEventReceived;


		public List<NetPlayer> Players = new List<NetPlayer>();

		public List<NetAgent> Agents = new List<NetAgent>();

		public List<NetItem> Items = new List<NetItem>();


		private static IKernNetzRoomListener _listener = null;

		private readonly object entitiesLock = new object();

		public const byte MAX_CHANNEL_LIMIT = 8;

		public static byte ItemChannel = 2;

		public static byte AgentChannel = 1;

		public static byte PlayerChannel = 0;

		private bool IsEnabled;

		private float timer;

		private float targetFrameRate;

		private byte counter;

		public int SyncRate = 20;

		public static void BindListener(IKernNetzRoomListener listener)
		{
			Room._listener = listener;
		}

		private void CreateTimer()
		{
			this.targetFrameRate = 1f / (float)this.SyncRate;
			Room.OnPlayerJoined += delegate (NetPlayer player)
			{
				this.IsEnabled = true;
			};
			Room.OnRoomDispose += delegate ()
			{
				this.IsEnabled = false;
			};
		}

		private void StopTimer()
		{
			this.IsEnabled = false;
		}

		public void Tick(float deltaTime)
		{
			if (!IsEnabled) return;

			this.timer += deltaTime;
			if (this.timer >= this.targetFrameRate)
			{
				this.timer = 0f;
				for (int i = 0; i < this.Players.Count; i++)
				{
					this.Players[i].FlushStateDelta();
				}
				for (int j = 0; j < this.Agents.Count; j++)
				{
					this.Agents[j].FlushStateDelta();
				}
				for (int k = 0; k < this.Items.Count; k++)
				{
					this.Items[k].FlushStateDelta();
				}
			}
		}

		private void SetUpChannelId()
		{
			ItemChannel++;
			if (ItemChannel >= MAX_CHANNEL_LIMIT) ItemChannel = 0;
			AgentChannel++;
			if (AgentChannel >= MAX_CHANNEL_LIMIT) AgentChannel = 0;
			PlayerChannel++;
			if (PlayerChannel >= MAX_CHANNEL_LIMIT) PlayerChannel = 0;
		}

		public void Init()
		{
			this.CreateTimer();
			KN.IsInGame = true;
		}

		public void Deinit()
		{
			this.StopTimer();
			KN.IsInGame = false;

			Players.Clear();
			Agents.Clear();
			Items.Clear();
		}

		public void OnRoomCreated()
		{
			_listener?.OnRoomCreated();
			OnRoomCreate?.Invoke();

			SetUpChannelId();
		}

		public void OnRoomDisposed()
		{
			_listener?.OnRoomDispose();
			OnRoomDispose?.Invoke();
		}

		public void PlayerJoin(NetPlayer networkPlayer)
		{
			Players.Add(networkPlayer);
			_listener?.OnPlayerJoined(networkPlayer);
			OnPlayerJoined?.Invoke(networkPlayer);
			networkPlayer.channelID = PlayerChannel;
		}

		public void PlayerLeft(uint playerId)
		{
			var player = Players.Find(p => p.NetworkId == playerId);
			Players.Remove(player);
			_listener?.OnPlayerLeft(player);
			OnPlayerLeft?.Invoke(player);
		}

		public void OnEvent(uint sender, RoomEventData eventData)
		{
			_listener?.OnEventReceived(sender, eventData);
			OnEventReceived?.Invoke(sender, eventData);
		}

		public void OnAgentOwnershipChange(uint networkId, uint ownerId)
		{
			var agent = Agents.Find(a => a.NetworkId == networkId);
			if (agent != null)
			{
				agent.UpdateProperties(ownerId, false, KN.Room.MyPlayerId);
			}
		}

		public void OnOwnershipRequest(uint networkId, uint ownerId, bool isLock)
		{
			var item = Items.Find(i => i.NetworkId == networkId);
			if (item != null)
			{
				item.UpdateProperties(ownerId, isLock, KN.Room.MyPlayerId);
			}
		}

		public void OnClearOwnership(uint networkId)
		{
			var item = Items.Find(i => i.NetworkId == networkId);
			if (item != null)
			{
				item.IsLocked = false;
			}
		}

		public void OnStateChange(uint sender, int state, bool isLocked)
		{
			RoomState = state;
			_listener?.OnRoomStateChange(state);
		}

		public void SetRoomState(int state, bool isLock)
		{
			if (FN.Connections[0].IsConnected)
			{
				Message operation = new RoomStateChangeOperation().GetOperation(this.RoomId, state, isLock);
				FN.Connections[0].SendMessage(operation, DeliveryMethod.Reliable, 0);
			}
		}

		public void SendEvent(RoomEventData eventData, DeliveryMethod deliveryMethod)
		{
			if (FN.Connections[0].IsConnected)
			{
				counter++;
				byte channelId = (byte)(counter % MAX_CHANNEL_LIMIT);
				var eventROomOp = new RoomEventOperation().GetOperation(KN.Room.RoomId, eventData, (int)deliveryMethod);
				FN.Connections[0].SendMessage(eventROomOp, deliveryMethod, channelId);
			}
		}

		public void OnEntityDelete(EntityType entityType, uint networkId)
		{
			switch (entityType)
			{
				case EntityType.Player:

					break;
				case EntityType.Agent:
					var agent = Agents.Find(a => a.NetworkId == networkId);
					if (agent != null)
					{
						OnEntityDeleted?.Invoke(agent);
						_listener?.OnAgentDeleted(agent);
						Agents.Remove(agent);
					}
					break;
				case EntityType.Item:
					var item = Items.Find(a => a.NetworkId == networkId);
					if (item != null)
					{
						OnEntityDeleted?.Invoke(item);
						_listener?.OnItemDeleted(item);
						Items.Remove(item);
					}
					break;
			}

		}

		public void OnEntityCreate(EntityType entityType, short entityId, uint networkId, uint ownerId, System.Numerics.Vector3 position, System.Numerics.Vector3 rotation, System.Numerics.Vector3 scale, byte[] states = null)
		{
			switch (entityType)
			{
				case EntityType.Player:
					FN.Logger.Error($"{entityType} creation is not supported via Instantiate Function");
					break;
				case EntityType.Agent:
					var entity = NetAgent.CreateAgent(entityType, entityId, networkId);
					entity.channelID = AgentChannel;
					if (states != null)
					{
						var __state = Default_Serializer.Deserialize2<EntityDefaultState>(states); //FN.Serializer.Deserialize<Dictionary<DeliveryMethod, EntangleState>>(new ArraySegment<byte>(states));

						Dictionary<DeliveryMethod, EntangleState> compatibleState = new Dictionary<DeliveryMethod, EntangleState>();
						foreach (var item in __state.states)
						{
							compatibleState.Add((DeliveryMethod)item.Key, item.Value);
						}

						entity.States = compatibleState;

						foreach (var state in entity.States)
						{
							state.Value.BindOnValueChangeProperties();
						}
					}
					entity.OwnerId = ownerId;
					entity.UpdateOwner();
					OnEntityCreated?.Invoke(entity, position, rotation, scale);
					_listener?.OnAgentCreated(entity, position, rotation, scale);
					Agents.Add(entity);
					break;
				case EntityType.Item:
					var _entity = NetItem.CreateItem(entityType, entityId, networkId);
					_entity.channelID = ItemChannel;
					if (states != null)
					{
						var __state = Default_Serializer.Deserialize2<EntityDefaultState>(states); // FN.Serializer.Deserialize<Dictionary<DeliveryMethod, EntangleState>>(new ArraySegment<byte>(states));
						Dictionary<DeliveryMethod, EntangleState> compatibleState = new Dictionary<DeliveryMethod, EntangleState>();
						foreach (var item in __state.states)
						{
							compatibleState.Add((DeliveryMethod)item.Key, item.Value);
						}

						_entity.States = compatibleState;

						foreach (var state in _entity.States)
						{
							state.Value.BindOnValueChangeProperties();
						}
					}
					//_entity.OwnerId = ownerId;
					_entity.UpdateProperties(ownerId, false, MyPlayerId);
					OnEntityCreated?.Invoke(_entity, position, rotation, scale);
					_listener?.OnItemCreated(_entity, position, rotation, scale);
					Items.Add(_entity);
					break;
			}

		}

		// todo: implement custom find to avoid gc pressure
		public void OnEntityStateChangeReceive(EntityType type, uint entityId, EntangleState state)
		{
			switch (type)
			{
				case EntityType.Player:
					FindPlayer(entityId)?.ApplyStateDelta(state);
					break;
				case EntityType.Agent:
					FindAgent(entityId)?.ApplyStateDelta(state);
					break;
				case EntityType.Item:
					FindItem(entityId)?.ApplyStateDelta(state);
					break;
			}
		}


		private NetPlayer FindPlayer(uint networkId)
		{
			NetPlayer player = null;
			for (int i = 0; i < Players.Count; i++)
			{
				if (Players[i].NetworkId == networkId)
				{
					player = Players[i];
					break;
				}
			}
			return player;
		}

		private NetItem FindItem(uint networkId)
		{
			NetItem item = null;
			for (int i = 0; i < Items.Count; i++)
			{
				if (Items[i].NetworkId == networkId)
				{
					item = Items[i];
					break;
				}
			}
			return item;
		}

		private NetAgent FindAgent(uint networkId)
		{
			NetAgent agent = null;
			for (int i = 0; i < Agents.Count; i++)
			{
				if (Agents[i].NetworkId == networkId)
				{
					agent = Agents[i];
					break;
				}
			}
			return agent;
		}

	}
}
