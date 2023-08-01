namespace FigNetCommon
{
    public enum OperationCode : ushort
    {
        CreateRoom = 60300,
        JoinRoom = 60301,
        GetRoomList = 60302,
        LeaveRoom = 60303,
        AppKey = 60304,

        OnPlayerJoinRoom = 60400,
        OnPlayerLeftRoom = 60401,
        OnEntityState = 60402,
        OnMasterClientChange = 60403,
        InstantiateEntity = 60404,
        DeleteEntity = 60405,
        RoomEvent = 60406,
        RequestOwnerShip = 60407,
        ClearOwnerShip = 60408,
        OnAgentOwnerShipChange = 60409,
        RoomStateChange = 60410,
        PreRoomStateReceived = 60411,
        PostRoomStateReceived = 60412
    }

    public enum RoomResponseCode : byte
    {
        Sucess = 0,
        InvalidPassword = 1,
        RoomLocked = 2,
        RoomFull = 3,
        AlreadyInRoom = 4,
        Failure = 5
    }

    public enum RoomQuery : byte
    {
        All = 0,
        Avaliable = 1
    }

    public enum EntityType : byte
    {
        Player = 0,
        Agent = 1,
        Item = 2
    }
}
