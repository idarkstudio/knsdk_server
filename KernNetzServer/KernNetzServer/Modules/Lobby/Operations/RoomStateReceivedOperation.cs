using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace KernNetzServer.Modules.Lobby.Operations
{
    internal class PreRoomStateReceivedOperation
    {
        public Message GetOperation()
        {
            Dictionary<byte, object> parameters = new Dictionary<byte, object>();

            Message operation = new Message((ushort)OperationCode.PreRoomStateReceived, new OperationData(parameters));
            return operation;
        }

    }


    internal class PostRoomStateReceivedOperation
    {
        public Message GetOperation()
        {
            Dictionary<byte, object> parameters = new Dictionary<byte, object>();

            Message operation = new Message((ushort)OperationCode.PostRoomStateReceived, new OperationData(parameters));
            return operation;
        }

    }
}
