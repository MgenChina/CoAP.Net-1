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
using System.Diagnostics.CodeAnalysis;

namespace CoAPNet
{
    [ExcludeFromCodeCoverage]
    public class CoapException : Exception
    {
        public CoapMessageCode ResponseCode { get; }

        public CoapException()
        {
            ResponseCode = CoapMessageCode.InternalServerError;
        }

        public CoapException(string message) : base(message)
        {
            ResponseCode = CoapMessageCode.InternalServerError;
        }

        public CoapException(string message, CoapMessageCode responseCode) : base(message)
        {
            ResponseCode = responseCode;
        }

        public CoapException(string message, Exception innerException) : base(message, innerException)
        {
            ResponseCode = CoapMessageCode.InternalServerError;
        }

        public CoapException(string message, Exception innerException, CoapMessageCode responseCode) : base(message, innerException)
        {
            ResponseCode = responseCode;
        }
    }
}