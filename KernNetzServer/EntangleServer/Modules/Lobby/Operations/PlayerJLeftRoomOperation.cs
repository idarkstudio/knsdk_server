using KernNetz;
using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace KernNetzServer.Modules.Lobby.Operations
{
    internal class PlayerJLeftRoomOperation
    {
        public Message GetOperation(uint playerId)
        {
            Dictionary<byte, object> parameters = new Dictionary<byte, object>
            {
                { 0, playerId }
            };

            Message operation = new Message((ushort)OperationCode.OnPlayerLeftRoom, new OperationData(parameters));
            return operation;
        }

    }
}
