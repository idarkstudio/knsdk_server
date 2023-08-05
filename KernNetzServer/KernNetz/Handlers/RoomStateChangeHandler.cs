using FigNet.Core;
using FigNetCommon;
using FigNetCommon.Data;

namespace KernNetz.Handlers
{
    internal class RoomStateChangeHandler : IHandler
    {
        public ushort MsgId => (ushort)OperationCode.RoomStateChange;
        public void HandleMessage(Message message, uint PeerId)
        {
            var operation = message.Payload as OperationData;
            uint sender = (uint)operation.Parameters[0];
            int state = (int)operation.Parameters[1];
            bool isLock = (bool)operation.Parameters[2];

            var room = FigNet.KernNetz.KN.Room;
            room.OnStateChange(sender, state, isLock);

        }
    }
}
