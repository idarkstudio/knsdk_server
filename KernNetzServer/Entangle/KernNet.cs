using System;
using KernNetz;
using FigNet.Core;
using FigNetCommon;

namespace FigNet.KernNetz
{
    public static class KN
    {
        public static Room Room { private set; get; }
        public static Lobby Lobby { private set; get; }
        public static MatchMaker MatchMaker { private set; get; }
        public static bool IsMasterClient { get; set; }
        public static bool IsConnected { get; private set; }
        public static bool IsInGame { get; set; }

        private static ClientSocketEventListner clientSocketEventListner = new ClientSocketEventListner();

        public static event Action OnConnected;
        public static event Action OnDisconnected;
        public static event Action<PeerStatus> OnNetworkStatusChanged;
        public static IClientSocket ClientSocket { get; private set; }
        private static bool isEventAdded = false;

        public static Action<bool> OnMasterClientUpdate;

        public static int GetPing()
        {
            int ping = -1;
            if (ClientSocket != null)
            {
                ping = ClientSocket.Ping;
            }
            return ping;
        }
        private class ClientSocketEventListner : IClientSocketListener
        {
            public void OnConnected()
            {
                KN.IsConnected = true;
                KN.OnConnected?.Invoke();
            }

            public void OnDisconnected()
            {
                KN.IsConnected = false;
                KN.OnDisconnected?.Invoke();
            }

            public void OnInitilize(IClientSocket clientSocket)
            {
                ClientSocket = clientSocket;
            }
            public void OnConnectionStatusChange(PeerStatus peerStatus)
            {
                OnNetworkStatusChanged?.Invoke(peerStatus);
            }
            public void OnNetworkReceive(Message message) 
            {
                bool handled = FN.HandlerCollection.HandleMessage(message, 0);
                if (!handled)
                {
                    // no handler is registered against coming msg, mannually handle it here
                }
            }

            public void OnNetworkSent(Message message, DeliveryMethod method, byte channelId = 0)
            {
                
            }

            public void OnProcessPayloadException(ExceptionType type, ushort messageId, Exception e)
            {
            }
        }

        public static void Instantiate()
        {
            FN.Logger = new DefaultLogger();
            FigNetCommon.Utils.Serializer = new Default_Serializer();
            FigNetCommon.Utils.Logger = FN.Logger;

            Room = new Room();
            Lobby = new Lobby();
            MatchMaker = new MatchMaker();
            Lobby.Initialize();
        }

        private static KernNetzConfig Config = null;
        public static void SetConfig(KernNetzConfig config)
        {
            Config = config;
        }

        public static void Connect()
        {
            // make peerconfig object with properties
            // add connect & connect 

            if (FN.Connections != null && FN.Connections.Count > 0)
            {
                FN.Connections[0].Reconnect();
            }
            else
            {
                var peerConfig = new PeerConfig()
                {
                    Port = (ushort)Config.Port,
                    PeerIp = Config.ServerIp,
                    Provider = Config.TransportLayer,
                    MaxChannels = (ushort)Config.MaxChannels,
                    DisconnectTimeout = 10000,
                    Name = "EntangleClient"

                };

                FN.AddConnectionAndConnect(peerConfig, clientSocketEventListner);
            }
        }

        /// <summary>
        /// Auto join the room you are in, on Reconnect
        /// </summary>
        public static void ReconnectAndJoin()
        {
            if (!isEventAdded)
            {
                isEventAdded = true;
                OnConnected += RejoinRoom;
            }
            Connect();
        }

        public static void Disconnect()
        {
            if (IsConnected && FN.Connections != null && FN.Connections.Count > 0)
            {
                FN.Connections[0].Disconnect();
            }
        }

        private static void RejoinRoom()
        {
            if (!KN.IsInGame) return;

            Lobby.ReJoinRoom(KN.Room.RoomId, KN.Room.Password, true, KN.Room.RoomAuthToken, (response) => {

                FN.Logger.Info($"Join Room Response {response}");
            });
        }

    }
}
