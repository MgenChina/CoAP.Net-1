﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CoAP.Net
{
    public enum OptionType
    {
        /// <summary>
        /// Defines the <see cref="Option"/> to have no content
        /// </summary>
        Empty,
        /// <summary>
        /// Defines the <see cref="Option"/>'s data is an <see cref="byte[]"/>
        /// </summary>
        Opaque,
        /// <summary>
        /// Defines the <see cref="Option"/>'s value is an <see cref="uint"/>
        /// </summary>
        UInt,
        /// <summary>
        /// Defines the <see cref="Option"/>'s data is a human readable <see cref="String"/>.
        /// </summary>
        String
    }

    // Todo: Caching (Section 5.6 of [RFC7252])
    // Todo: Proxying (Section 5.7 of [RFC7252])
    public abstract class Option
    {
        private readonly int _optionNumber;
        /// <summary>
        /// Gets whether the option should fail if not supported by the CoAP endpoint
        /// <para>See Section 5.4.1 of [RFC7252]</para>
        /// </summary>
        public bool IsCritical {
            get
            {
                return (_optionNumber & 0x01) > 0;
            }
        }

        /// <summary>
        /// Gets if this resource is unsafe to proxy through the CoAP endpoint
        /// <para>See Section 5.4.2 of [RFC7252]</para>
        /// </summary>
        /// <se
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
        public int MinLength { get=>_minLength; }

        private readonly int _maxLength;

        /// <summary>
        /// Gets the maximum length supported by this option. 
        /// </summary>
        public int MaxLength { get=>_maxLength; }

        private bool _isRepeatable;

        /// <summary>
        /// Gets whether this option is allowed to be sent multiple times in a single CoAP message
        /// </summary>
        public bool IsRepeatable { get=>_isRepeatable; }

        private readonly OptionType _type;

        /// <summary>
        /// Gets the <see cref="OptionType"/> of this option.
        /// </summary>
        public OptionType Type { get=>_type; }

        public virtual uint GetDefaultUInt()
        {
            /// Must be overridden by sub class and return a default value if <see cref="Type"/> == <see cref="OptionType.UInt"/>
            throw new InvalidCastException();
        }

        public virtual byte[] GetDefaultOpaque()
        {
            /// Must be overridden by sub class and return a default value if <see cref="Type"/> == <see cref="OptionType.String"/>
            throw new InvalidCastException();
        }

        public virtual string GetDefaultString()
        {
            /// Must be overridden by sub class and return a default value if <see cref="Type"/> == <see cref="OptionType.String"/>
            throw new InvalidCastException();
        }

        protected Option(int optionNumber, int minLength = 0, int maxLength = 0, bool isRepeatable = false, OptionType type = OptionType.Empty)
        {
            _optionNumber = optionNumber;
            _type = type;
            _minLength = minLength;
            _maxLength = maxLength;
            _isRepeatable = isRepeatable;
        }
    }
}
