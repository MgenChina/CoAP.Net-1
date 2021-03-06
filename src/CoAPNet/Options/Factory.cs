﻿using System;
using System.Reflection;
using System.Collections.Generic;

namespace CoAPNet.Options
{
    public static class Factory
    {
        static Dictionary<int, Type> _options = new Dictionary<int, Type>();

        static Factory()
        {
            Register<UriHost>();
            Register<UriPort>();
            Register<UriPath>();
            Register<UriQuery>();

            Register<ProxyUri>();
            Register<ProxyScheme>();

            Register<LocationPath>();
            Register<LocationQuery>();

            Register<ContentFormat>();
            Register<Accept>();
            Register<MaxAge>();
            Register<ETag>();
            Register<Size1>();

            Register<IfMatch>();
            Register<IfNoneMatch>();
        }

        public static void Register<T>() where T : CoapOption
        {
            Type type = typeof(T);
            Register(type);
        }

        public static void Register(Type type)
        {
            if(!type.GetTypeInfo().IsSubclassOf(typeof(CoapOption)))
                throw new ArgumentException($"Type must be a subclass of {nameof(CoapOption)}");

            var option = (CoapOption)Activator.CreateInstance(type);
            _options.Add(option.OptionNumber, type);
        }

        /// <summary>
        /// Try to create an <see cref="CoapOption"/> from the option number and data. 
        /// </summary>
        /// <param name="number"></param>
        /// <param name="data"></param>
        /// <returns><value>null</value> if the option is unsupported.</returns>
        /// <exception cref="CoapOptionException">If the option number is unsuppported and is critical (See RFC 7252 Section 5.4.1)</exception>
        public static CoapOption Create(int number, byte[] data = null)
        {
            // Let the exception get thrown if index is out of range
            Type type = null;
            if (!_options.TryGetValue(number, out type))
            {
                if (number % 2 == 1)
                    throw new CoapOptionException($"Unsupported critical option ({number})", new ArgumentOutOfRangeException(nameof(number)));
                return null;
            }

            var option = (CoapOption)Activator.CreateInstance(type);
            if (data != null)
                option.FromBytes(data);

            return option;
        }
    }
}
