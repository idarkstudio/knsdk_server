using System;
using MessagePack;


namespace FigNetCommon
{
    public class FNConstants
    {
        public const float FLOAT_PRECISION = 1000;
    }

    public enum SyncType : byte
    {
        OnValueChange,
        Continuous
    }

    public enum PropertyType : byte
    {
        NotSupported,
        FNShort,
        FNInt,
        FNFloat,
        FNVector2,
        FNVector3,
        FNVector4,
        FNString
    }


    [MessagePackObject]
    public sealed class DataContainer : IFNProperty
    {
        [Key(0)]
        public byte[] data;
    }


    [MessagePack.Union(0, typeof(FNVector3))]
    [MessagePack.Union(1, typeof(FNVector2))]
    [MessagePack.Union(2, typeof(FNVector4))]
    [MessagePack.Union(3, typeof(FNFloat))]
    [MessagePack.Union(4, typeof(FNInt))]
    [MessagePack.Union(5, typeof(FNShort))]
    [MessagePack.Union(6, typeof(FNString))]
    public interface IFNProperty
    { }

    [MessagePackObject]
    public sealed class FNProperty<T>
    {
        [IgnoreMember] public bool IsDirty { get; set; }
        [IgnoreMember] T _value;
        public event Action<T> OnValueChange;
        [IgnoreMember] public T value => _value;
        [Key(0)]
        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (!_value.Equals(value))
                {
                    IsDirty = true;
                    _value = value;
                    OnValueChange?.Invoke(_value);
                }
            }
        }
    }


    [MessagePackObject]
    public sealed class FNShort : IFNProperty
    {
        [IgnoreMember] public bool IsDirty { get; set; }
        [IgnoreMember] short _value;
        [IgnoreMember] public SyncType SyncType { get; set; }

        public event Action<short> OnValueChange;
        [IgnoreMember] public short value => _value;
        [Key(0)]
        public short Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value != value)
                {
                    IsDirty = true;
                    _value = value;
                    OnValueChange?.Invoke(_value);
                }
            }
        }

        public void InvokeOnValueChanged(short value)
        {
            IsDirty = true;
            _value = value;
            OnValueChange?.Invoke(_value);
        }
    }

    [MessagePackObject]
    public sealed class FNInt : IFNProperty
    {
        [IgnoreMember] public bool IsDirty { get; set; }
        [IgnoreMember] int _value;
        [IgnoreMember] public SyncType SyncType { get; set; }
        public event Action<int> OnValueChange;
        [IgnoreMember] public int value => _value;
        [Key(0)]
        public int Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value != value)
                {
                    IsDirty = true;
                    _value = value;
                    OnValueChange?.Invoke(_value);
                }

            }
        }

        public void InvokeOnValueChanged(int value)
        {
            IsDirty = true;
            _value = value;
            OnValueChange?.Invoke(_value);
        }
    }

    [MessagePackObject]
    public sealed class FNFloat : IFNProperty
    {
        [IgnoreMember] public bool IsDirty { get; set; }
        [IgnoreMember] float _value;
        [IgnoreMember] public SyncType SyncType { get; set; }

        public event Action<float> OnValueChange;
        [IgnoreMember] public float value => _value;
        [Key(0)]
        public float Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value != value)
                {
                    IsDirty = true;
                    _value = value;
                    OnValueChange?.Invoke(_value);
                }
            }
        }

        public void InvokeOnValueChanged(float value)
        {
            IsDirty = true;
            _value = value;
            OnValueChange?.Invoke(_value);
        }
    }

    [MessagePackObject]
    public sealed class FNString : IFNProperty
    {
        [IgnoreMember] public bool IsDirty { get; set; }
        [IgnoreMember] string _value;
        [IgnoreMember] public SyncType SyncType { get; set; }

        public event Action<string> OnValueChange;
        [IgnoreMember] public string value => _value;
        [Key(0)]
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value != value)
                {
                    IsDirty = true;
                    _value = value;
                    OnValueChange?.Invoke(_value);
                }

            }
        }

        public void InvokeOnValueChanged(string value)
        {
            IsDirty = true;
            _value = value;
            OnValueChange?.Invoke(_value);
        }
    }


    [MessagePackObject]
    public sealed class FNVector4 : IFNProperty
    {
        [IgnoreMember] public bool IsDirty { get; set; }
        [IgnoreMember] public SyncType SyncType { get; set; }
        [IgnoreMember] FNVec4 _value;
        public event Action<FNVec4> OnValueChange;
        [IgnoreMember] public float x => _value.X;
        [IgnoreMember] public float y => _value.Y;
        [IgnoreMember] public float z => _value.Z;
        [IgnoreMember] public float w => _value.W;

        public FNVector4()
        {
            _value = new FNVec4();
        }

        [Key(0)]
        public FNVec4 Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value != value)
                {
                    IsDirty = true;
                    _value = value;
                    OnValueChange?.Invoke(_value);
                }
            }
        }

        public void InvokeOnValueChanged(FNVec4 value)
        {
            IsDirty = true;
            _value = value;
            OnValueChange?.Invoke(_value);
        }

        public void SetValue(FNVec4 vec4)
        {
            if (!_value.IsEquals(vec4))
            {
                _value.__X__ = (int)(vec4.X * FNConstants.FLOAT_PRECISION);
                _value.__Y__ = (int)(vec4.Y * FNConstants.FLOAT_PRECISION);
                _value.__Z__ = (int)(vec4.Z * FNConstants.FLOAT_PRECISION);
                _value.__W__ = (int)(vec4.W * FNConstants.FLOAT_PRECISION);
                IsDirty = true;
                OnValueChange?.Invoke(_value);
            }
        }

    }

    [MessagePackObject]
    public class FNVector3 : IFNProperty
    {
        [IgnoreMember] public bool IsDirty { get; set; }
        [IgnoreMember] public SyncType SyncType { get; set; }
        [IgnoreMember] FNVec3 _value;
        public event Action<FNVec3> OnValueChange;
        [IgnoreMember] public float x => _value.X;
        [IgnoreMember] public float y => _value.Y;
        [IgnoreMember] public float z => _value.Z;

        public FNVector3()
        {
            _value = new FNVec3();
        }

        [Key(0)]
        public FNVec3 Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value != value)
                {
                    IsDirty = true;
                    _value = value;
                    OnValueChange?.Invoke(_value);
                }
            }
        }

        public void InvokeOnValueChanged(FNVec3 value)
        {
            IsDirty = true;
            _value = value;
            OnValueChange?.Invoke(_value);
        }

        public void SetValue(FNVec3 vec3)
        {

            if (!_value.IsEquals(vec3))
            {
                _value.__X__ = (int)(vec3.X * FNConstants.FLOAT_PRECISION);
                _value.__Y__ = (int)(vec3.Y * FNConstants.FLOAT_PRECISION);
                _value.__Z__ = (int)(vec3.Z * FNConstants.FLOAT_PRECISION);

                IsDirty = true;
                OnValueChange?.Invoke(_value);
            }
        }
    }

    [MessagePackObject]
    public sealed class FNVector2 : IFNProperty
    {
        [IgnoreMember] public bool IsDirty { get; set; }
        [IgnoreMember] public SyncType SyncType { get; set; }
        [IgnoreMember] FNVec2 _value;
        public event Action<FNVec2> OnValueChange;

        [IgnoreMember] public float x => _value.X;
        [IgnoreMember] public float y => _value.Y;

        public FNVector2()
        {
            _value = new FNVec2();
        }

        [Key(0)]
        public FNVec2 Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value != value)
                {
                    IsDirty = true;
                    _value = value;
                    OnValueChange?.Invoke(_value);
                }
            }
        }

        public void InvokeOnValueChanged(FNVec2 value)
        {
            IsDirty = true;
            _value = value;
            OnValueChange?.Invoke(_value);
        }

        public void SetValue(FNVec2 vec2)
        {

            if (!_value.IsEquals(vec2))
            {
                _value.__X__ = (int)(vec2.X * FNConstants.FLOAT_PRECISION);
                _value.__Y__ = (int)(vec2.Y * FNConstants.FLOAT_PRECISION);

                IsDirty = true;
                OnValueChange?.Invoke(_value);
            }
        }
    }


    [MessagePackObject]
    public sealed class FNVec2 : IEquatable<FNVec2>
    {
        [Key(0)] public int __X__;
        [Key(1)] public int __Y__;

        [IgnoreMember]
        public float X => __X__ / FNConstants.FLOAT_PRECISION;

        [IgnoreMember]
        public float Y => __Y__ / FNConstants.FLOAT_PRECISION;

        public FNVec2()
        {

        }

        public FNVec2(float x, float y)
        {
            __X__ = (int)(x * FNConstants.FLOAT_PRECISION);
            __Y__ = (int)(y * FNConstants.FLOAT_PRECISION);
        }

        public void SetValue(float x, float y)
        {
            __X__ = (int)(x * FNConstants.FLOAT_PRECISION);
            __Y__ = (int)(y * FNConstants.FLOAT_PRECISION);
        }

        public void SetValue(FNVec2 vec2)
        {
            __X__ = (int)(vec2.X * FNConstants.FLOAT_PRECISION);
            __Y__ = (int)(vec2.Y * FNConstants.FLOAT_PRECISION);
        }

        public void SetValue(ref FNVec2 vec2)
        {
            __X__ = (int)(vec2.X * FNConstants.FLOAT_PRECISION);
            __Y__ = (int)(vec2.Y * FNConstants.FLOAT_PRECISION);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as FNVec2);
        }

        public bool IsEquals(FNVec2 vec2)
        {
            bool equals = false;

            if ((__X__ == vec2.__X__ || __X__ == vec2.__X__ - 1 || __X__ == vec2.__X__ + 1) &&
                (__Y__ == vec2.__Y__ || __Y__ == vec2.__Y__ - 1 || __Y__ == vec2.__Y__ + 1))
            {
                equals = true;
            }

            return equals;
        }

        public bool Equals(FNVec2 p)
        {
            if (Object.ReferenceEquals(p, null))
            {
                return false;
            }

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, p))
            {
                return true;
            }

            if (this.GetType() != p.GetType())
            {
                return false;
            }

            return (__X__ == p.__X__) && (__Y__ == p.__Y__);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(FNVec2 lhs, FNVec2 rhs)
        {
            if (Object.ReferenceEquals(lhs, null))
            {
                if (Object.ReferenceEquals(rhs, null))
                {
                    return true;
                }

                return false;
            }
            return lhs.Equals(rhs);
        }

        public static bool operator !=(FNVec2 lhs, FNVec2 rhs)
        {
            return !(lhs == rhs);
        }

    }


    [MessagePackObject]
    public sealed class FNVec3 : IEquatable<FNVec3>
    {
        [Key(0)] public int __X__;
        [Key(1)] public int __Y__;
        [Key(2)] public int __Z__;

        [IgnoreMember]
        public float X => __X__ / FNConstants.FLOAT_PRECISION;

        [IgnoreMember]
        public float Y => __Y__ / FNConstants.FLOAT_PRECISION;

        [IgnoreMember]
        public float Z => __Z__ / FNConstants.FLOAT_PRECISION;
        public FNVec3()
        {

        }

        public FNVec3(float x, float y, float z)
        {
            __X__ = (int)(x * FNConstants.FLOAT_PRECISION);
            __Y__ = (int)(y * FNConstants.FLOAT_PRECISION);
            __Z__ = (int)(z * FNConstants.FLOAT_PRECISION);
        }


        public void SetValue(float x, float y, float z)
        {
            __X__ = (int)(x * FNConstants.FLOAT_PRECISION);
            __Y__ = (int)(y * FNConstants.FLOAT_PRECISION);
            __Z__ = (int)(z * FNConstants.FLOAT_PRECISION);
        }

        public void SetValue(FNVec3 vec3)
        {
            __X__ = (int)(vec3.X * FNConstants.FLOAT_PRECISION);
            __Y__ = (int)(vec3.Y * FNConstants.FLOAT_PRECISION);
            __Z__ = (int)(vec3.Z * FNConstants.FLOAT_PRECISION);
        }

        public void SetValue(ref FNVec3 vec3)
        {
            __X__ = (int)(vec3.X * FNConstants.FLOAT_PRECISION);
            __Y__ = (int)(vec3.Y * FNConstants.FLOAT_PRECISION);
            __Z__ = (int)(vec3.Z * FNConstants.FLOAT_PRECISION);
        }


        public bool IsEquals(FNVec3 vec3)
        {
            bool equals = false;

            if ((__X__ == vec3.__X__ || __X__ == vec3.__X__ - 1 || __X__ == vec3.__X__ + 1) &&
                (__Y__ == vec3.__Y__ || __Y__ == vec3.__Y__ - 1 || __Y__ == vec3.__Y__ + 1) &&
                (__Z__ == vec3.__Z__ || __Z__ == vec3.__Z__ - 1 || __Z__ == vec3.__Z__ + 1))
            {
                equals = true;
            }

            return equals;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as FNVec3);
        }

        public bool Equals(FNVec3 p)
        {
            if (Object.ReferenceEquals(p, null))
            {
                return false;
            }

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, p))
            {
                return true;
            }

            if (this.GetType() != p.GetType())
            {
                return false;
            }

            return (__X__ == p.__X__) && (__Y__ == p.__Y__) && (__Z__ == p.__Z__);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(FNVec3 lhs, FNVec3 rhs)
        {
            if (Object.ReferenceEquals(lhs, null))
            {
                if (Object.ReferenceEquals(rhs, null))
                {
                    return true;
                }

                return false;
            }
            return lhs.Equals(rhs);
        }

        public static bool operator !=(FNVec3 lhs, FNVec3 rhs)
        {
            return !(lhs == rhs);
        }

    }

    [MessagePackObject]
    public sealed class FNVec4 : IEquatable<FNVec4>
    {
        [Key(0)] public int __X__;
        [Key(1)] public int __Y__;
        [Key(2)] public int __Z__;
        [Key(3)] public int __W__;

        [IgnoreMember]
        public float X => __X__ / FNConstants.FLOAT_PRECISION;

        [IgnoreMember]
        public float Y => __Y__ / FNConstants.FLOAT_PRECISION;

        [IgnoreMember]
        public float Z => __Z__ / FNConstants.FLOAT_PRECISION;
        [IgnoreMember]
        public float W => __W__ / FNConstants.FLOAT_PRECISION;

        public FNVec4()
        {

        }

        public FNVec4(float x, float y, float z, float w)
        {
            __X__ = (int)(x * FNConstants.FLOAT_PRECISION);
            __Y__ = (int)(y * FNConstants.FLOAT_PRECISION);
            __Z__ = (int)(z * FNConstants.FLOAT_PRECISION);
            __W__ = (int)(w * FNConstants.FLOAT_PRECISION);
        }

        public void SetValue(float x, float y, float z, float w)
        {
            __X__ = (int)(x * FNConstants.FLOAT_PRECISION);
            __Y__ = (int)(y * FNConstants.FLOAT_PRECISION);
            __Z__ = (int)(z * FNConstants.FLOAT_PRECISION);
            __W__ = (int)(w * FNConstants.FLOAT_PRECISION);
        }

        public void SetValue(FNVec4 vec4)
        {
            __X__ = (int)(vec4.X * FNConstants.FLOAT_PRECISION);
            __Y__ = (int)(vec4.Y * FNConstants.FLOAT_PRECISION);
            __Z__ = (int)(vec4.Z * FNConstants.FLOAT_PRECISION);
            __W__ = (int)(vec4.W * FNConstants.FLOAT_PRECISION);
        }

        public void SetValue(ref FNVec4 vec4)
        {
            __X__ = (int)(vec4.X * FNConstants.FLOAT_PRECISION);
            __Y__ = (int)(vec4.Y * FNConstants.FLOAT_PRECISION);
            __Z__ = (int)(vec4.Z * FNConstants.FLOAT_PRECISION);
            __W__ = (int)(vec4.W * FNConstants.FLOAT_PRECISION);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as FNVec4);
        }

        public bool IsEquals(FNVec4 vec4)
        {
            bool equals = false;

            if ((__X__ == vec4.__X__ || __X__ == vec4.__X__ - 1 || __X__ == vec4.__X__ + 1) &&
                (__Y__ == vec4.__Y__ || __Y__ == vec4.__Y__ - 1 || __Y__ == vec4.__Y__ + 1) &&
                (__Z__ == vec4.__Z__ || __Z__ == vec4.__Z__ - 1 || __Z__ == vec4.__Z__ + 1) &&
                (__W__ == vec4.__W__ || __W__ == vec4.__W__ - 1 || __W__ == vec4.__W__ + 1))
            {
                equals = true;
            }

            return equals;
        }

        public bool Equals(FNVec4 p)
        {
            if (Object.ReferenceEquals(p, null))
            {
                return false;
            }

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, p))
            {
                return true;
            }

            if (this.GetType() != p.GetType())
            {
                return false;
            }

            return (__X__ == p.__X__) && (__Y__ == p.__Y__) && (__Z__ == p.__Z__) && (__W__ == p.__W__);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(FNVec4 lhs, FNVec4 rhs)
        {
            if (Object.ReferenceEquals(lhs, null))
            {
                if (Object.ReferenceEquals(rhs, null))
                {
                    return true;
                }

                return false;
            }
            return lhs.Equals(rhs);
        }

        public static bool operator !=(FNVec4 lhs, FNVec4 rhs)
        {
            return !(lhs == rhs);
        }
    }
}
