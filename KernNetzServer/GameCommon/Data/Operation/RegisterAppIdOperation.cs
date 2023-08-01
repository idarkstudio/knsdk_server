using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace FigNet.KernNetz.Operations
{
    public class RegisterAppIdOperation
    {
        public Message GetOperation(string appId)
        {
            Dictionary<byte, object> parameters = new Dictionary<byte, object>
            {
                { 0, appId }
            };

            Message operation = new Message((ushort)OperationCode.AppKey, new OperationData(parameters));
            return operation;
        }

    }
}
