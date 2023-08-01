using System;
using FigNet.Core;
using FigNetCommon;
using FigNet.KernNetz;
using System.Collections.Generic;
using FigNet.KernNetz.Operations;
using System.Runtime.CompilerServices;

namespace KernNetz
{
    public abstract class NetworkEntity
    {
        public uint NetworkId;
        public float SyncRate = 10;

        public short EntityId;
        public uint OwnerId;
        //{ 
        //    get => ownerId;
        //    set
        //    {
        //        ownerId = value;
        //        IsMine = EN.Room.MyPlayerId == ownerId;
        //    }
        //}
        // uint ownerId = uint.MaxValue;
        public bool IsLocked { get; set; }
        public bool IsMine
        {
            get { return isMine; }
            protected set
            {
                isMine = value;
                OnOWnershipChange?.Invoke(isMine);
            }
        }
        public EntityType EntityType { get; protected set; }

        public Dictionary<DeliveryMethod, EntangleState> States = null;

        public FNVector3 Position;
        public byte channelID = 0;
        public event Action<bool> OnOWnershipChange;

        private bool isMine;
        public NetworkEntity()
        {
            OwnerId = uint.MaxValue;
            States = new Dictionary<DeliveryMethod, EntangleState>();
        }

        public virtual bool RequestOwnership(bool withLock = false) { return false; }

        public virtual void ClearOwnership() { }

        public void ClearStates()
        {
            foreach (var state in States)
            {
                state.Value?.ClearProperties();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateProperties(uint owner, bool isLock, uint myId)
        {
            OwnerId = owner;
            IsLocked = isLock;

            IsMine = myId == OwnerId;
        }

        public static void Instantiate(EntityType entityType, short entityId, FNVec3 position = default, FNVec3 rotation = default, FNVec3 scale = default)
        {
            if (FN.Connections[0].IsConnected && entityType != EntityType.Player)
            {
                var instantiateOp = new InstantiateOperation().GetOperation(FigNet.KernNetz.KN.Room.RoomId, entityType, entityId, position, rotation, scale);
                FN.Connections[0].SendMessage(instantiateOp, DeliveryMethod.Reliable);
            }
        }

        public static void Delete(EntityType entityType, uint networkId)
        {
            if (FN.Connections[0].IsConnected && entityType != EntityType.Player)
            {
                var deleteOp = new DeleteEntityOperation().GetOperation(FigNet.KernNetz.KN.Room.RoomId, networkId, entityType);
                FN.Connections[0].SendMessage(deleteOp, DeliveryMethod.Reliable);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PropertyType GetPropertyType<T>(T property)
        {
            PropertyType type = PropertyType.NotSupported;

            if (typeof(T) == typeof(FNShort)) type = PropertyType.FNShort;
            else if (typeof(T) == typeof(FNInt)) type = PropertyType.FNInt;
            else if (typeof(T) == typeof(FNFloat)) type = PropertyType.FNFloat;
            else if (typeof(T) == typeof(FNVector2)) type = PropertyType.FNVector2;
            else if (typeof(T) == typeof(FNVector3)) type = PropertyType.FNVector3;
            else if (typeof(T) == typeof(FNVector4)) type = PropertyType.FNVector4;
            else if (typeof(T) == typeof(FNString)) type = PropertyType.FNString;

            return type;
        }

        // todo: in future replace State with scriptable objects
        public bool SetNetProperty<T>(byte index, T property, DeliveryMethod deliveryMethod, SyncType syncType = SyncType.OnValueChange) where T : IFNProperty
        {
            bool result = false;
            if (!States.ContainsKey(deliveryMethod))
            {
                States.Add(deliveryMethod, new EntangleState() { DeliveryMethod = (int)deliveryMethod });
            }

            var type = GetPropertyType<T>(property);

            switch (type)
            {
                case PropertyType.FNShort:
                    var _short = property as FNShort;
                    _short.SyncType = syncType;
                    result = States[deliveryMethod].SetShort(index, _short);
                    break;
                case PropertyType.FNInt:
                    var _int = property as FNInt;
                    _int.SyncType = syncType;
                    result = States[deliveryMethod].SetInt(index, _int);
                    break;
                case PropertyType.FNFloat:
                    var _float = property as FNFloat;
                    _float.SyncType = syncType;
                    result = States[deliveryMethod].SetFloat(index, _float);
                    break;
                case PropertyType.FNVector2:
                    var _vector2 = property as FNVector2;
                    _vector2.SyncType = syncType;
                    States[deliveryMethod].SetVector2(index, _vector2);
                    break;
                case PropertyType.FNVector3:
                    var _vector3 = property as FNVector3;
                    _vector3.SyncType = syncType;
                    result = States[deliveryMethod].SetVector3(index, _vector3);
                    break;
                case PropertyType.FNVector4:
                    var _vector4 = property as FNVector4;
                    _vector4.SyncType = syncType;
                    result = States[deliveryMethod].SetVector4(index, _vector4);
                    break;
                case PropertyType.FNString:
                    var _string = property as FNString;
                    _string.SyncType = syncType;
                    result = States[deliveryMethod].SetString(index, _string);
                    break;
                default:
                    FN.Logger.Error($"Undefined Set property {type} | {typeof(T)}");
                    break;
            }
            return result;
        }

        public void GetNetProperty<T>(byte index, DeliveryMethod deliveryMethod, Action<T> callBack) where T : class
        {
            if (!States.ContainsKey(deliveryMethod))
            {
                States.Add(deliveryMethod, new EntangleState() { DeliveryMethod = (int)deliveryMethod });
            }

            var type = GetPropertyType<T>(null);

            switch (type)
            {
                case PropertyType.FNShort:
                    States[deliveryMethod].GetShort(index, (obj) => {

                        callBack.Invoke(obj as T);
                    });
                    break;
                case PropertyType.FNInt:
                    States[deliveryMethod].GetInt(index, (obj) => {

                        callBack.Invoke(obj as T);
                    });
                    break;
                case PropertyType.FNFloat:
                    States[deliveryMethod].GetFloat(index, (obj) => {

                        callBack.Invoke(obj as T);
                    });
                    break;
                case PropertyType.FNVector2:
                    States[deliveryMethod].GetVector2(index, (obj) => {

                        callBack.Invoke(obj as T);
                    });
                    break;
                case PropertyType.FNVector3:
                    States[deliveryMethod].GetVector3(index, (obj) => {

                        callBack.Invoke(obj as T);
                    });

                    break;
                case PropertyType.FNVector4:
                    States[deliveryMethod].GetVector4(index, (obj) => {

                        callBack.Invoke(obj as T);
                    });
                    break;
                case PropertyType.FNString:
                    States[deliveryMethod].GetString(index, (obj) => {

                        callBack.Invoke(obj as T);
                    });
                    break;

                default:
                    FN.Logger.Error($"Undefined Get property {type} | {typeof(T)}");
                    break;
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FlushStateDelta()
        {
            // todo
            // if client & Owner then prceed
            // if server proceeds anyway

            if (!IsMine) return;
            foreach (var state in States)
            {
                if (state.Value.IsDirty)
                {
                    var delta = GetStateDelta(state.Key);
                    this.Send(delta);
                    state.Value.IsDirty = false;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(EntangleState delta)
        {
            if (FN.Connections[0].IsConnected)
            {
                var op = EntityStateOperation.GetOperation(NetworkId, FigNet.KernNetz.KN.Room.RoomId, EntityType, delta);
                FN.Connections[0].SendMessage(op, (DeliveryMethod)delta.DeliveryMethod, channelID);

            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntangleState GetState(DeliveryMethod channel)
        {
            return States[channel];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ApplyStateDelta(EntangleState delta)
        {
            
            try
            {
                States[(DeliveryMethod)delta.DeliveryMethod].ApplyStateDelta(delta);
            }
            catch (Exception)
            {
                if (!States.ContainsKey((DeliveryMethod)delta.DeliveryMethod))
                {
                    States.Add((DeliveryMethod)delta.DeliveryMethod, new EntangleState() { DeliveryMethod = delta.DeliveryMethod });
                }
                try
                {
                    States[(DeliveryMethod)delta.DeliveryMethod].ApplyStateDelta(delta);
                }
                catch (Exception e)
                {
                    FN.Logger.Exception(e, e.Message);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntangleState GetStateDelta(DeliveryMethod channel)
        {
            return States[channel].GetDelta();
        }

    }
}
