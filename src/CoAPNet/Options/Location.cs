﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CoAPNet.Options
{
    /// <summary>
    /// <para>See section 5.10.7 of [RFC7252]</para>
    /// </summary>
    public class LocationPath : CoapOption
    {
        public LocationPath() : base(optionNumber: CoapRegisteredOptionNumber.LocationPath, maxLength: 255, isRepeatable: true, type: OptionType.String) { }
    }

    /// <summary>
    /// <para>See section 5.10.7 of [RFC7252]</para>
    /// </summary>
    public class LocationQuery : CoapOption
    {
        public LocationQuery() : base(optionNumber: CoapRegisteredOptionNumber.LocationQuery, maxLength: 255, isRepeatable: true, type: OptionType.String){ }
    }
}
