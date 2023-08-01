using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace FigNet.KernNetz.Operations
{
    public class CreateRoomOperation
    {
        // RoomName
        // MaxClients
        // Password
        public Message GetOperation(string roomName, short maxPlayers, string password = "")
        {
            Dictionary<byte, object> parameters = new Dictionary<byte, object>
            {
                { 0, roomName },
                { 1, maxPlayers },
                { 2, password }
            };

            Message operation = new Message((ushort)OperationCode.CreateRoom, new OperationData(parameters));
            return operation;
        }

    }
}
