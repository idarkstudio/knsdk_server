using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;

namespace KernNetz.Handlers
{
    internal class PlayerLeftRoomHandler : IHandler
    {
        public ushort MsgId => (ushort)OperationCode.OnPlayerLeftRoom;
        public void HandleMessage(Message message, uint PeerId)
        {
            var operation = message.Payload as OperationData;
            uint playerId = (uint)operation.Parameters[0];

            var room = FigNet.KernNetz.KN.Room;

            FN.Logger.Info($"OnPlayer Left HANDLER {playerId}");
            room.PlayerLeft(playerId);
        }
    }
}
