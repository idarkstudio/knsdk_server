using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;
using System.Collections.Generic;

namespace FigNet.KernNetz.Operations
{
	public class JoinRoomOperation
	{
		public Message GetOperation(uint roomId, string password = "", bool rejoin = false, string roomAuthToken = "")
		{
			Dictionary<byte, object> parameters = new Dictionary<byte, object>
			{
				{
					0,
					roomId
				},
				{
					1,
					password
				},
				{
					2,
					rejoin
				},
				{
					3,
					roomAuthToken
				}
			};
			return new Message((ushort)OperationCode.JoinRoom, new OperationData(parameters));
		}

		public Message GetOperation(uint roomId, string password = "")
		{
			Dictionary<byte, object> parameters = new Dictionary<byte, object>
			{
				{
					0,
					roomId
				},
				{
					1,
					password
				}
			};
			return new Message((ushort)OperationCode.JoinRoom, new OperationData(parameters));
		}
	}
}
