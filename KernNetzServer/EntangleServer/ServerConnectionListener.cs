using System;
using FigNet.Core;
using KernNetzServer.Modules.Lobby;

namespace EntangleServer
{
    public class ServerConnectionListener : IServerSocketListener
    {
        private readonly KNLobby lobby;
        public ServerConnectionListener()
        {
            lobby ??= ServiceLocator.GetService<IServer>().GetModule<KNLobby>();
        }
        public void OnError(Exception e, string message)
        {
            FN.Logger.Error(e.StackTrace);
        }

        public void OnInitilize(IServerSocket serverSocket)
        {

        }

        public void OnNetworkReceive(Message message, uint sender)
        {
            bool handled = FN.HandlerCollection.HandleMessage(message, sender);
            if (!handled)
            {
                // no handler is registered against coming msg, mannually handle it here
            }
        }

        public void OnNetworkSent(Message message, DeliveryMethod method, byte channelId = 0)
        {

        }

        public void OnPeerConnected(IPeer peer)
        {
            lobby.OnPlayerConnected(peer);
        }

        public void OnPeerDisconnected(IPeer peer)
        {
            lobby.OnPlayerLeftForceFully(peer);
        }

        public void OnProcessPayloadException(ExceptionType type, ushort messageId, Exception e)
        {
            FN.Logger.Error($"ex type {type} | msg {(FigNetCommon.OperationCode)messageId} | {e.StackTrace}");
        }
    }
}
