﻿#region License
// Copyright 2017 Roman Vaughan (NZSmartie)
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Linq;

namespace CoAPNet
{
    [ExcludeFromCodeCoverage]
    public class CoapOptionException : CoapException
    {
        public CoapOptionException() : base() { }

        public CoapOptionException(string message) : base(message, CoapMessageCode.BadOption) { }

        public CoapOptionException(string message, Exception innerException) : base(message, innerException, CoapMessageCode.BadOption) { }
    }

    public enum OptionType
    {
        /// <summary>
        /// Defines the <see cref="CoapOption"/> to have no content
        /// </summary>
        Empty,
        /// <summary>
        /// Defines the <see cref="CoapOption"/>'s data is an <c><see cref="byte"/>[]</c>
        /// </summary>
        Opaque,
        /// <summary>
        /// Defines the <see cref="CoapOption"/>'s value is an <see cref="uint"/>
        /// </summary>
        UInt,
        /// <summary>
        /// Defines the <see cref="CoapOption"/>'s data is a human readable <see cref="String"/>.
        /// </summary>
        String
    }

    /// <summary>
    /// Helper extension methods for handling collections of <see cref="CoapOption"/>s
    /// </summary>
    public static class CoapOptionExtensions
    {
        /// <summary>
        /// Gets the first <typeparamref name="OptionType"/> in <paramref name="collection"/> that matches the class type. 
        /// </summary>
        /// <typeparam name="OptionType"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static OptionType Get<OptionType>(this ICollection<CoapOption> collection) where OptionType : CoapOption
        {
            return (OptionType)collection.FirstOrDefault(o => o is OptionType);
        }

        /// <summary>
        /// Gets the first <typeparamref name="OptionType"/> in <paramref name="collection"/> that matches the class type. 
        /// </summary>
        /// <typeparam name="OptionType"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IEnumerable<OptionType> GetAll<OptionType>(this ICollection<CoapOption> collection) where OptionType : CoapOption
        {
            return collection.Where(o => o is OptionType).Select(o => (OptionType)o);
        }

        /// <summary>
        /// Checks if the <typeparamref name="OptionType"/> exists in <paramref name="collection"/>
        /// </summary>
        /// <typeparam name="OptionType"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static bool Contains<OptionType>(this ICollection<CoapOption> collection) where OptionType : CoapOption
        {
            return collection.Any(o => o is OptionType);
        }

        /// <summary>
        /// Gets the first <see cref="CoapOption"/> in <paramref name="collection"/> that matches the <paramref name="optionNumber"/>
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="optionNumber"></param>
        /// <returns></returns>
        public static bool Contains(this ICollection<CoapOption> collection, int optionNumber)
        {
            return collection.Any(o => o.OptionNumber == optionNumber);
        }
    }

    // TODO: Caching (Section 5.6 of [RFC7252])
    // TODO: Proxying (Section 5.7 of [RFC7252])
    public class CoapOption : IComparable<CoapOption>
    {
        private readonly int _optionNumber;
        /// <summary>
        /// Gets whether the option should fail if not supported by the CoAP endpoint
        /// <para>See Section 5.4.1 of [RFC7252]</para>
        /// </summary>
        public bool IsCritical
        {
            get
            {
                return (_optionNumber & 0x01) > 0;
            }
        }

        public override string ToString()
        {
            if (_type == OptionType.Empty)
                return $"<{GetType().Name}> (empty)";

            if (_type == OptionType.Opaque)
                return $"<{GetType().Name}> ({_length} bytes)";

            if (_type == OptionType.String)
                return $"<{GetType().Name}> \"({(string)_value}\"";

            return $"{GetType().Name}";
        }

        /// <summary>
        /// Gets if this resource is unsafe to proxy through the CoAP endpoint
        /// <para>See Section 5.4.2 of [RFC7252]</para>
        /// </summary>
        public bool IsUnsafe
        {
            get
            {
                return (_optionNumber & 0x02) > 0;
            }
        }

        /// <summary>
        /// Gets whether the option is allowed to have a chache key?
        /// </summary>
        public bool NoCacheKey
        {
            get
            {
                return (_optionNumber & 0x1e) == 0x1c;
            }
        }

        /// <summary>
        /// Gets the unique option number as defined in the CoAP Option Numbers Registry
        /// <para>See section 12.2 of [RFC7252]</para>
        /// </summary>
        public int OptionNumber { get => _optionNumber; }

        private readonly int _minLength;

        /// <summary>
        /// Gets the minimum length supported by this option. 
        /// </summary>
        public int MinLength { get => _minLength; }

        private readonly int _maxLength;

        /// <summary>
        /// Gets the maximum length supported by this option. 
        /// </summary>
        public int MaxLength { get => _maxLength; }

        private bool _isRepeatable;

        /// <summary>
        /// Gets whether this option is allowed to be sent multiple times in a single CoAP message
        /// </summary>
        public bool IsRepeatable { get => _isRepeatable; }

        private readonly OptionType _type;

        /// <summary>
        /// Gets the <see cref="OptionType"/> of this option.
        /// </summary>
        public OptionType Type { get => _type; }

        protected readonly object _default;

        protected object _value = null;
        protected int _length = 0;

        public uint DefaultUInt
        {
            get
            {
                if (_type != OptionType.UInt)
                    throw new InvalidCastException();
                return _default == null ? 0 : (uint)_default;
            }
        }

        public uint ValueUInt
        {
            get
            {
                if (_type != OptionType.UInt)
                    throw new InvalidCastException();
                return (uint)_value;
            }
            set
            {
                if (_type != OptionType.UInt)
                    throw new InvalidCastException();
                _value = value;
                if (value > 0xFFFFFFu)
                    _length = 4;
                else if (value > 0xFFFFu)
                    _length = 3;
                else if (value > 0xFFu)
                    _length = 2;
                else if (value > 0u)
                    _length = 1;
                else
                    _length = 0;
            }
        }

        public byte[] DefaultOpaque
        {
            get
            {
                if (_type != OptionType.Opaque)
                    throw new InvalidCastException();
                return (byte[])_default;
            }
        }

        public byte[] ValueOpaque
        {
            get
            {
                if (_type != OptionType.Opaque)
                    throw new InvalidCastException();
                return (byte[])_value;
            }
            set
            {
                if (_type != OptionType.Opaque)
                    throw new InvalidCastException();
                _value = value;
                _length = value == null ? 0 : value.Length;
            }
        }

        public string DefaultString
        {
            get
            {
                if (_type != OptionType.String)
                    throw new InvalidCastException();
                return (string)_default;
            }
        }

        public string ValueString
        {
            get
            {
                if (_type != OptionType.String)
                    throw new InvalidCastException();
                return (string)_value;
            }
            set
            {
                if (_type != OptionType.String)
                    throw new InvalidCastException();
                _value = value;
                _length = value == null ? 0 : Encoding.UTF8.GetByteCount(value);
            }
        }

        public int Length { get => _length; }

        public byte[] GetBytes()
        {
            if (_type == OptionType.Empty)
                return new byte[0];

            if (_type == OptionType.Opaque)
                return (byte[])_value;

            if (_type == OptionType.String)
                return Encoding.UTF8.GetBytes((string)_value);

            var data = new byte[_length];
            uint i = 0, value = (uint)_value;
            if (_length == 4)
                data[i++] = (byte)((value & 0xFF000000u) >> 24);
            if (_length >= 3)
                data[i++] = (byte)((value & 0xFF0000u) >> 16);
            if (_length >= 2)
                data[i++] = (byte)((value & 0xFF00u) >> 8);
            if (_length >= 1)
                data[i++] = (byte)(value & 0xFFu);
            return data;
        }

        public void FromBytes(byte[] data)
        {
            if (_type == OptionType.Empty)
            {
                if ((data?.Length ?? 0) > 0)
                    throw new InvalidCastException("Empty option does not accept any data");
                return;
            }

            if (_type == OptionType.Opaque)
            {
                ValueOpaque = data;
                return;
            }

            if (_type == OptionType.String)
            {
                ValueString = Encoding.UTF8.GetString(data);
                return;
            }

            
            uint i=0, value = 0;
            if (data.Length == 4)
                value = (uint)(data[i++] << 24);
            if (data.Length >= 3)
                value |= (uint)(data[i++] << 16);
            if (data.Length >= 2)
                value |= (uint)(data[i++] << 8);
            if (data.Length >= 1)
                value |= data[i++];
            ValueUInt = value;
        }

        protected internal CoapOption(int optionNumber, int minLength = 0, int maxLength = 0, bool isRepeatable = false, OptionType type = OptionType.Empty, object defaultValue = null)
        {
            _optionNumber = optionNumber;
            _type = type;
            _minLength = minLength;
            _maxLength = maxLength;
            _isRepeatable = isRepeatable;
            _default = defaultValue;
        }

        public override bool Equals(object obj)
        {
            var option = obj as CoapOption;
            if (option == null)
                return base.Equals(obj);

            if (option._optionNumber != _optionNumber)
                return false;
            // Asume all other parameters of the option match; Only focus on the value

            switch (_type)
            {
                case OptionType.Empty:
                    return true;
                case OptionType.UInt:
                    return (uint)option._value == (uint)_value;
                case OptionType.Opaque:
                    return ((byte[])option._value).SequenceEqual((byte[])_value);
                case OptionType.String:
                    return ((string)option._value).Equals((string)_value, StringComparison.Ordinal);
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Gets a hashcode unique to the <see cref="CoapOption"/> sub class. 
        /// </summary>
        /// <remarks>
        /// This will generate and store the hashcode based on the subclass's full name. 
        /// </remarks>
        private static Dictionary<Type, int> _hashCode = new Dictionary<System.Type, int>();
        public override int GetHashCode()
        {
            if (_hashCode.TryGetValue(GetType(), out int hashcode) == false)
            {
                hashcode = GetType().FullName.GetHashCode();
                _hashCode.Add(GetType(), hashcode);
            }

            return hashcode;
        }

        int IComparable<CoapOption>.CompareTo(CoapOption other)
        {
            return _optionNumber - other._optionNumber;
        }
    }
}
