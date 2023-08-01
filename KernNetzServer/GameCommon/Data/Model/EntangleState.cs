using System;
using MessagePack;
using FigNetCommon;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FigNet.KernNetz
{
    [MessagePackObject]
    public sealed class EntangleState
    {
        [IgnoreMember] public bool IsDirty;
        [Key(0)] public int DeliveryMethod = 0;
        [Key(1)] public Dictionary<ushort, IFNProperty> Properties = new Dictionary<ushort, IFNProperty>();

        [IgnoreMember] private EntangleState stateDelta;

        [IgnoreMember]
        private readonly Dictionary<PropertyType, ushort> TypeMapping = new Dictionary<PropertyType, ushort>()
        {
            { PropertyType.FNShort, 256 },
            { PropertyType.FNInt, 512 },
            { PropertyType.FNFloat, 768 },
            { PropertyType.FNVector2, 1024 },
            { PropertyType.FNVector3, 1280 },
            { PropertyType.FNVector4, 1536 },
            { PropertyType.FNString, 1792 }
        };

        [IgnoreMember]
        public int DeltaCount => Properties.Count;

        private class PendingCallback<T>
        {
            Dictionary<byte, Action<T>> pendingCallbacks = new Dictionary<byte, Action<T>>();

            public void AppendProperty(byte index, Action<T> callback)
            {
                if (!pendingCallbacks.ContainsKey(index))
                {
                    pendingCallbacks.Add(index, callback);
                }
            }

            public Action<T> FetchProperty(byte index)
            {
                Action<T> action = null;
                if (pendingCallbacks.ContainsKey(index))
                {
                    action = pendingCallbacks[index];
                }
                return action;
            }

        }

        [IgnoreMember] private PendingCallback<FNShort> shortPendingCallbacks = null;
        [IgnoreMember] private PendingCallback<FNInt> intPendingCallbacks = null;
        [IgnoreMember] private PendingCallback<FNFloat> floatPendingCallbacks = null;
        [IgnoreMember] private PendingCallback<FNVector2> vec2PendingCallbacks = null;
        [IgnoreMember] private PendingCallback<FNVector3> vec3PendingCallbacks = null;
        [IgnoreMember] private PendingCallback<FNVector4> vec4PendingCallbacks = null;
        [IgnoreMember] private PendingCallback<FNString> stringPendingCallbacks = null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearAllStates()
        {
            if (stateDelta.Properties.Count > 0) stateDelta.Properties.Clear();
        }

        public void ClearProperties()
        {
            if (Properties.Count > 0) Properties.Clear();
        }


        public EntangleState GetDelta()
        {
            if (stateDelta == null) stateDelta = new EntangleState();
            stateDelta.DeliveryMethod = this.DeliveryMethod;
            ClearAllStates();

            foreach (var prop in Properties)
            {
                if (prop.Key < TypeMapping[PropertyType.FNShort])
                {
                    var Short = prop.Value as FNShort;

                    if (Short.IsDirty)
                    {
                        stateDelta.Properties.Add(prop.Key, Short);
                        if (Short.SyncType == SyncType.OnValueChange)
                            Short.IsDirty = false;
                    }
                }
                else if (prop.Key < TypeMapping[PropertyType.FNInt])
                {
                    var Int = prop.Value as FNInt;

                    if (Int.IsDirty)
                    {
                        stateDelta.Properties.Add(prop.Key, Int);
                        if (Int.SyncType == SyncType.OnValueChange)
                            Int.IsDirty = false;
                    }
                }
                else if (prop.Key < TypeMapping[PropertyType.FNFloat])
                {
                    var Float = prop.Value as FNFloat;

                    if (Float.IsDirty)
                    {
                        stateDelta.Properties.Add(prop.Key, Float);
                        if (Float.SyncType == SyncType.OnValueChange)
                            Float.IsDirty = false;
                    }
                }
                else if (prop.Key < TypeMapping[PropertyType.FNVector2])
                {
                    var Vec2 = prop.Value as FNVector2;

                    if (Vec2.IsDirty)
                    {
                        stateDelta.Properties.Add(prop.Key, Vec2);
                        if (Vec2.SyncType == SyncType.OnValueChange)
                            Vec2.IsDirty = false;
                    }
                }
                else if (prop.Key < TypeMapping[PropertyType.FNVector3])
                {
                    var Vec3 = prop.Value as FNVector3;

                    if (Vec3.IsDirty)
                    {
                        stateDelta.Properties.Add(prop.Key, Vec3);
                        if (Vec3.SyncType == SyncType.OnValueChange)
                            Vec3.IsDirty = false;
                    }
                }
                else if (prop.Key < TypeMapping[PropertyType.FNVector4])
                {
                    var Vec4 = prop.Value as FNVector4;

                    if (Vec4.IsDirty)
                    {
                        stateDelta.Properties.Add(prop.Key, Vec4);
                        if (Vec4.SyncType == SyncType.OnValueChange)
                            Vec4.IsDirty = false;
                    }
                }
                else if (prop.Key < TypeMapping[PropertyType.FNString])
                {
                    var Str = prop.Value as FNString;

                    if (Str.IsDirty)
                    {
                        stateDelta.Properties.Add(prop.Key, Str);
                        if (Str.SyncType == SyncType.OnValueChange)
                            Str.IsDirty = false;
                    }
                }
            }

            return stateDelta;
        }

        public void ApplyStateDelta(EntangleState delta)
        {
            foreach (var prop in delta.Properties)
            {
                if (Properties.ContainsKey(prop.Key))
                {
                    if (prop.Key < TypeMapping[PropertyType.FNShort])
                    {
                        var Short = prop.Value as FNShort;
                        (Properties[prop.Key] as FNShort).Value = Short.value;

                    }
                    else if (prop.Key < TypeMapping[PropertyType.FNInt])
                    {
                        var Int = prop.Value as FNInt;
                        (Properties[prop.Key] as FNInt).Value = Int.value;
                    }
                    else if (prop.Key < TypeMapping[PropertyType.FNFloat])
                    {
                        var Float = prop.Value as FNFloat;
                        (Properties[prop.Key] as FNFloat).Value = Float.value;
                    }
                    else if (prop.Key < TypeMapping[PropertyType.FNVector2])
                    {
                        var Vec2 = prop.Value as FNVector2;
                        (Properties[prop.Key] as FNVector2).Value = Vec2.Value;
                    }
                    else if (prop.Key < TypeMapping[PropertyType.FNVector3])
                    {
                        var Vec3 = prop.Value as FNVector3;
                        (Properties[prop.Key] as FNVector3).Value = Vec3.Value;
                    }
                    else if (prop.Key < TypeMapping[PropertyType.FNVector4])
                    {
                        var Vec4 = prop.Value as FNVector4;
                        (Properties[prop.Key] as FNVector4).Value = Vec4.Value;
                    }
                    else if (prop.Key < TypeMapping[PropertyType.FNString])
                    {
                        var Str = prop.Value as FNString;
                        (Properties[prop.Key] as FNString).Value = Str.Value;
                    }
                }
                else
                {
                    IsDirty = true;

                    if (prop.Key < TypeMapping[PropertyType.FNShort])
                    {
                        var Short = prop.Value as FNShort;

                        Properties.Add(prop.Key, Short);
                        Short.IsDirty = true;
                        Short.OnValueChange += (val) =>
                        {
                            IsDirty = true;
                        };
                        var cb = shortPendingCallbacks?.FetchProperty((byte)(prop.Key));
                        cb?.Invoke(Short);
                        Short.InvokeOnValueChanged(Short.Value);
                    }
                    else if (prop.Key < TypeMapping[PropertyType.FNInt])
                    {
                        if (prop.Key < TypeMapping[PropertyType.FNInt])
                        {
                            var Int = prop.Value as FNInt;

                            Properties.Add(prop.Key, Int);
                            Int.IsDirty = true;
                            Int.OnValueChange += (val) =>
                            {
                                IsDirty = true;
                            };
                            var cb = intPendingCallbacks?.FetchProperty((byte)(prop.Key - TypeMapping[PropertyType.FNShort]));  // todo: double check
                            cb?.Invoke(Int);

                            Int.InvokeOnValueChanged(Int.Value);
                        }
                    }
                    else if (prop.Key < TypeMapping[PropertyType.FNFloat])
                    {
                        var Float = prop.Value as FNFloat;

                        Properties.Add(prop.Key, Float);
                        Float.IsDirty = true;
                        Float.OnValueChange += (val) =>
                        {
                            IsDirty = true;
                        };

                        var cb = floatPendingCallbacks?.FetchProperty((byte)(prop.Key - TypeMapping[PropertyType.FNInt]));  // todo: double check
                        cb?.Invoke(Float);

                        Float.InvokeOnValueChanged(Float.Value);
                    }
                    else if (prop.Key < TypeMapping[PropertyType.FNVector2])
                    {
                        var Vec2 = prop.Value as FNVector2;

                        Properties.Add(prop.Key, Vec2);
                        Vec2.IsDirty = true;
                        Vec2.OnValueChange += (val) =>
                        {
                            IsDirty = true;
                        };
                        var cb = vec2PendingCallbacks?.FetchProperty((byte)(prop.Key - TypeMapping[PropertyType.FNFloat]));
                        cb?.Invoke(Vec2);

                        Vec2.InvokeOnValueChanged(Vec2.Value);
                    }
                    else if (prop.Key < TypeMapping[PropertyType.FNVector3])
                    {
                        var Vec3 = prop.Value as FNVector3;


                        Properties.Add(prop.Key, Vec3);
                        Vec3.IsDirty = true;
                        Vec3.OnValueChange += (val) =>
                        {
                            IsDirty = true;
                        };
                        var cb = vec3PendingCallbacks?.FetchProperty((byte)(prop.Key - TypeMapping[PropertyType.FNVector2]));
                        cb?.Invoke(Vec3);

                        Vec3.InvokeOnValueChanged(Vec3.Value);
                    }
                    else if (prop.Key < TypeMapping[PropertyType.FNVector4])
                    {
                        var Vec4 = prop.Value as FNVector4;

                        Properties.Add(prop.Key, Vec4);
                        Vec4.IsDirty = true;
                        Vec4.OnValueChange += (val) =>
                        {
                            IsDirty = true;
                        };
                        var cb = vec4PendingCallbacks?.FetchProperty((byte)(prop.Key - TypeMapping[PropertyType.FNVector3]));
                        cb?.Invoke(Vec4);

                        Vec4.InvokeOnValueChanged(Vec4.Value);
                    }
                    else if (prop.Key < TypeMapping[PropertyType.FNString])
                    {
                        var Str = prop.Value as FNString;

                        Properties.Add(prop.Key, Str);
                        Str.IsDirty = true;
                        Str.OnValueChange += (val) =>
                        {
                            IsDirty = true;
                        };
                        var cb = stringPendingCallbacks?.FetchProperty((byte)(prop.Key - TypeMapping[PropertyType.FNVector4]));
                        cb?.Invoke(Str);

                        Str.InvokeOnValueChanged(Str.Value);
                    }
                }
            }
        }



        public bool SetInt(byte index, FNInt value)
        {
            ushort _index = (ushort)(index + TypeMapping[PropertyType.FNShort]);
            if (!Properties.ContainsKey(_index) && value != null)
            {
                IsDirty = true;
                value.IsDirty = true;
                value.OnValueChange += (val) =>
                {
                    IsDirty = true;
                };
                Properties.Add(_index, value);
                return true;
            }
            else
            {
                Utils.Logger.Info($"index: {index} is already occupied OR value is null");
                return false;
            }
        }

        public void GetInt(byte index, Action<FNInt> callBack)
        {
            ushort _index = (ushort)(index + TypeMapping[PropertyType.FNShort]);
            bool feint = Properties.TryGetValue(_index, out IFNProperty feInt);
            if (feint)
            {
                var Int = feInt as FNInt;
                callBack?.Invoke(Int);
            }
            else
            {
                if (intPendingCallbacks == null) intPendingCallbacks = new PendingCallback<FNInt>();
                intPendingCallbacks.AppendProperty(index, callBack);
            }
        }

        public bool SetShort(byte index, FNShort value)
        {
            if (!Properties.ContainsKey(index) && value != null)
            {
                IsDirty = true;
                value.IsDirty = true;
                value.OnValueChange += (val) =>
                {
                    IsDirty = true;
                };
                Properties.Add(index, value);
                return true;
            }
            else
            {
                Utils.Logger.Info($"index: {index} is already occupied OR value is null");
                return false;
            }
        }

        public void GetShort(byte index, Action<FNShort> callBack)
        {
            bool feshort = Properties.TryGetValue(index, out IFNProperty feFshort);
            if (feshort)
            {
                var Short = feFshort as FNShort;
                callBack?.Invoke(Short);
            }
            else
            {
                if (shortPendingCallbacks == null) shortPendingCallbacks = new PendingCallback<FNShort>();
                shortPendingCallbacks.AppendProperty(index, callBack);
            }
        }

        public bool SetFloat(byte index, FNFloat value)
        {
            ushort _index = (ushort)(index + TypeMapping[PropertyType.FNInt]);
            if (!Properties.ContainsKey(_index) && value != null)
            {
                IsDirty = true;
                value.IsDirty = true;
                value.OnValueChange += (val) =>
                {
                    IsDirty = true;
                };

                Properties.Add(_index, value);
                return true;
            }
            else
            {
                Utils.Logger.Info($"index: {index} is already occupied OR value is null");
                return false;
            }
        }

        public void GetFloat(byte index, Action<FNFloat> callBack)
        {
            ushort _index = (ushort)(index + TypeMapping[PropertyType.FNInt]);
            bool fefloat = Properties.TryGetValue(_index, out IFNProperty feFloat);

            if (fefloat)
            {
                var Float = feFloat as FNFloat;
                callBack?.Invoke(Float);
            }
            else
            {
                if (floatPendingCallbacks == null) floatPendingCallbacks = new PendingCallback<FNFloat>();
                floatPendingCallbacks.AppendProperty(index, callBack);
            }
        }


        public bool SetVector2(byte index, FNVector2 value)
        {
            ushort _index = (ushort)(index + TypeMapping[PropertyType.FNFloat]);
            if (!Properties.ContainsKey(_index) && value != null)
            {
                IsDirty = true;
                value.IsDirty = true;
                value.OnValueChange += (val) =>
                {
                    IsDirty = true;
                };
                Properties.Add(_index, value);
                return true;
            }
            else
            {
                Utils.Logger.Info($"index: {index} is already occupied OR value is null");
                return false;
            }
        }


        public void GetVector2(byte index, Action<FNVector2> callBack)
        {
            ushort _index = (ushort)(index + TypeMapping[PropertyType.FNFloat]);
            bool fevector2 = Properties.TryGetValue(_index, out IFNProperty feVector2);
            if (fevector2)
            {
                var Vec2 = feVector2 as FNVector2;
                callBack?.Invoke(Vec2);
            }
            else
            {
                if (vec2PendingCallbacks == null) vec2PendingCallbacks = new PendingCallback<FNVector2>();
                vec2PendingCallbacks.AppendProperty(index, callBack);
            }
        }


        public bool SetVector3(byte index, FNVector3 value)
        {
            ushort _index = (ushort)(index + TypeMapping[PropertyType.FNVector2]);
            if (!Properties.ContainsKey(_index) && value != null)
            {
                IsDirty = true;
                value.IsDirty = true;
                value.OnValueChange += (val) =>
                {
                    IsDirty = true;
                };
                Properties.Add(_index, value);
                return true;
            }
            else
            {
                Utils.Logger.Info($"index: {index} is already occupied OR value is null");
                return false;
            }
        }

        public void GetVector3(byte index, Action<FNVector3> callBack)
        {
            ushort _index = (ushort)(index + TypeMapping[PropertyType.FNVector2]);
            bool fevector3 = Properties.TryGetValue(_index, out IFNProperty feVector3);
            if (fevector3)
            {
                FNVector3 Vec3 = feVector3 as FNVector3;
                callBack?.Invoke(Vec3);
            }
            else
            {
                if (vec3PendingCallbacks == null) vec3PendingCallbacks = new PendingCallback<FNVector3>();
                vec3PendingCallbacks.AppendProperty(index, callBack);
            }
        }

        public bool SetVector4(byte index, FNVector4 value)
        {
            ushort _index = (ushort)(index + TypeMapping[PropertyType.FNVector3]);
            if (!Properties.ContainsKey(_index) && value != null)
            {
                IsDirty = true;
                value.IsDirty = true;
                value.OnValueChange += (val) =>
                {
                    IsDirty = true;
                };
                Properties.Add(_index, value);
                return true;
            }
            else
            {
                Utils.Logger.Info($"index: {index} is already occupied OR value is null");
                return false;
            }
        }

        public void GetVector4(byte index, Action<FNVector4> callBack)
        {
            ushort _index = (ushort)(index + TypeMapping[PropertyType.FNVector3]);
            bool fevector4 = Properties.TryGetValue(_index, out IFNProperty feVector4);
            if (fevector4)
            {
                var vec4 = feVector4 as FNVector4;
                callBack?.Invoke(vec4);
            }
            else
            {
                if (vec4PendingCallbacks == null) vec4PendingCallbacks = new PendingCallback<FNVector4>();
                vec4PendingCallbacks.AppendProperty(index, callBack);
            }
        }

        public bool SetString(byte index, FNString value)
        {
            ushort _index = (ushort)(index + TypeMapping[PropertyType.FNVector4]);
            if (!Properties.ContainsKey(_index) && value != null)
            {
                IsDirty = true;
                value.IsDirty = true;
                value.OnValueChange += (val) =>
                {
                    IsDirty = true;
                };
                Properties.Add(_index, value);
                return true;
            }
            else
            {
                Utils.Logger.Info($"index: {index} is already occupied OR value is null");
                return false;
            }
        }

        public void GetString(byte index, Action<FNString> callBack)
        {
            ushort _index = (ushort)(index + TypeMapping[PropertyType.FNVector4]);
            bool feStr = Properties.TryGetValue(_index, out IFNProperty festr);
            if (feStr)
            {
                var Str = festr as FNString;
                callBack?.Invoke(Str);
            }
            else
            {
                if (stringPendingCallbacks == null) stringPendingCallbacks = new PendingCallback<FNString>();
                stringPendingCallbacks.AppendProperty(index, callBack);
            }
        }

        public void BindOnValueChangeProperties()
        {
            foreach (var prop in Properties)
            {
                if (prop.Key < TypeMapping[PropertyType.FNShort])
                {
                    var Short = prop.Value as FNShort;
                    Short.OnValueChange += (val) =>
                    {
                        IsDirty = true;
                    };
                }
                else if (prop.Key < TypeMapping[PropertyType.FNInt])
                {
                    var Int = prop.Value as FNInt;
                    Int.OnValueChange += (val) =>
                    {
                        IsDirty = true;
                    };
                }
                else if (prop.Key < TypeMapping[PropertyType.FNFloat])
                {
                    var Float = prop.Value as FNFloat;
                    Float.OnValueChange += (val) =>
                    {
                        IsDirty = true;
                    };
                }
                else if (prop.Key < TypeMapping[PropertyType.FNVector2])
                {
                    var Vec2 = prop.Value as FNVector2;
                    Vec2.OnValueChange += (val) =>
                    {
                        IsDirty = true;
                    };
                }
                else if (prop.Key < TypeMapping[PropertyType.FNVector3])
                {
                    var Vec3 = prop.Value as FNVector3;
                    Vec3.OnValueChange += (val) =>
                    {
                        IsDirty = true;
                    };
                }
                else if (prop.Key < TypeMapping[PropertyType.FNVector4])
                {
                    var Vec4 = prop.Value as FNVector4;
                    Vec4.OnValueChange += (val) =>
                    {
                        IsDirty = true;
                    };
                }
                else if (prop.Key < TypeMapping[PropertyType.FNString])
                {
                    var Str = prop.Value as FNString;
                    Str.OnValueChange += (val) =>
                    {
                        IsDirty = true;
                    };
                }
            }
        }
    }

}
