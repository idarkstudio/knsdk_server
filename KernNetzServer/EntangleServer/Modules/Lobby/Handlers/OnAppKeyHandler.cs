using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;

namespace KernNetzServer.Modules.Lobby.Handlers
{
    internal class OnAppKeyHandler : IHandler
    {
        private static KNLobby lobby;

        public ushort MsgId => (ushort)OperationCode.AppKey;
        public void HandleMessage(Message message, uint PeerId)
        {
            lobby ??= ServiceLocator.GetService<IServer>().GetModule<KNLobby>();
            var operation = message.Payload as OperationData;
            string appId = (string)operation.Parameters[0];

            bool isValid = lobby.AppKeyValidation(appId, PeerId);
            operation.Parameters[0] = isValid;

            var peer = FN.PeerCollection.GetPeerByID(PeerId);
            message.Payload = operation;
            FN.Server.SendMessage(peer, message, DeliveryMethod.Reliable, 0);
            FN.Logger.Info($"OnAppKey IsValid {isValid} sent by {PeerId}");
            // TODO: infuture server should call the peer disconnect if AppId is not valid

        }
    }
}
