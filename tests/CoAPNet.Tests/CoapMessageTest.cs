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
using System.Linq;
using System.Text;
using System.Collections.Generic;
using NUnit;
using NUnit.Framework;

namespace CoAPNet.Tests
{
    [TestFixture]
    public class CoapMessageTest
    {
        private CoapMessage _message = null;

        [SetUp]
        public void InitTest()
        {
            _message = new CoapMessage();
        }

        [TearDown]
        public void CleanupTest()
        {
            _message = null;
        }

        [Test]
        [Category("[RFC7252] Section 3"), Category("Encode")]
        public void TestMessageEmpty()
        {
            _message.Type = CoapMessageType.Confirmable;
            _message.Code = CoapMessageCode.None;
            _message.Id = 1234;

            var expected = new byte[] { 0x40, 0x00, 0x04, 0xD2 };
            var actual = _message.Serialise();

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [Test]
        [Category("[RFC7252] Section 3"), Category("Encode")]
        public void TestMessageEmptyWithToken()
        {
            _message.Type = CoapMessageType.Acknowledgement;
            _message.Code = CoapMessageCode.None;
            _message.Id = 1234;
            _message.Token = new byte[] { 0xC0, 0xff, 0xee };

            var expected = new byte[] { 0x60, 0x00, 0x04, 0xD2 };
            var actual = _message.Serialise();

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [Test]
        [Category("[RFC7252] Section 3")]
        public void TestMessageEmptyAckknowledgement()
        {
            _message = new CoapMessage();
            _message.Type = CoapMessageType.Acknowledgement;
            _message.Code = CoapMessageCode.Valid;
            _message.Id = 1235;

            var expected = new byte[] { 0x60, 0x43, 0x04, 0xD3 };
            var actual = _message.Serialise();

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [Test]
        [Category("[RFC7252] Section 3"), Category("Encode")]
        public void TestMessageEncodeRequest()
        {
            _message.Type = CoapMessageType.Confirmable;
            _message.Code = CoapMessageCode.Get;
            _message.Id = 16962;
            _message.Token = new byte[] { 0xde, 0xad, 0xbe, 0xef };

            _message.Options.Add(new Options.UriPath(".well-known"));
            _message.Options.Add(new Options.UriPath("core"));

            var expected = new byte[] {
                0x44, 0x01, 0x42, 0x42, 0xde, 0xad, 0xbe, 0xef, 0xBB, 0x2E, 0x77, 0x65, 0x6C, 0x6C, 0x2D,
                0x6B, 0x6E, 0x6F, 0x77, 0x6E, 0x04, 0x63, 0x6F, 0x72, 0x65
            };
            var actual = _message.Serialise();

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [Test]
        [Category("[RFC7252] Section 3"), Category("Encode")]
        public void TestMessageEncodeResponse()
        {
            _message.Type = CoapMessageType.Acknowledgement;
            _message.Code = CoapMessageCode.Content;
            _message.Id = 16962;
            _message.Token = new byte[] { 0xde, 0xad, 0xbe, 0xef };
            _message.Options.Add(new Options.ContentFormat(Options.ContentFormatType.ApplicationLinkFormat));

            _message.Payload = Encoding.UTF8.GetBytes("<.well-known/core/>");

            var expected = new byte[] {
                0x64, 0x45, 0x42, 0x42, 0xde, 0xad, 0xbe, 0xef, 0xc1, 0x28, 0xff, 0x3c, 0x2e, 0x77, 0x65, 0x6c, 0x6c, 0x2d, 0x6b, 0x6e,
                0x6f, 0x77, 0x6e, 0x2f, 0x63, 0x6f, 0x72, 0x65, 0x2f, 0x3e
            };
            var actual = _message.Serialise();

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [Test]
        [Category("[RFC7252] Section 3"), Category("Decode")]
        public void TestMessageDecodeRequest()
        {
            this._message.Deserialise(new byte[] {
                0x44, 0x01, 0x42, 0x42, 0xde, 0xad, 0xbe, 0xef, 0xBB, 0x2E, 0x77, 0x65, 0x6C, 0x6C, 0x2D,
                0x6B, 0x6E, 0x6F, 0x77, 0x6E, 0x04, 0x63, 0x6F, 0x72, 0x65
            });

            Assert.AreEqual(CoapMessageType.Confirmable, _message.Type);
            Assert.AreEqual(CoapMessageCode.Get, _message.Code);
            Assert.AreEqual(16962, _message.Id);

            Assert.IsTrue(new byte[] { 0xde, 0xad, 0xbe, 0xef }.SequenceEqual(_message.Token));

            Assert.IsTrue(new List<CoapOption> {
                new Options.UriPath(".well-known"),
                new Options.UriPath("core"),
            }.SequenceEqual(_message.Options));
        }

        [Test]
        [Category("[RFC7252] Section 3"), Category("Decode")]
        public void TestMessageDecodeResponse()
        {
            _message.Deserialise(new byte[] {
                0x64, 0x45, 0x42, 0x42, 0xde, 0xad, 0xbe, 0xef, 0xc1, 0x28, 0xff, 0x3c, 0x2e, 0x77, 0x65, 0x6c, 0x6c, 0x2d, 0x6b, 0x6e,
                0x6f, 0x77, 0x6e, 0x2f, 0x63, 0x6f, 0x72, 0x65, 0x2f, 0x3e
            });

            Assert.AreEqual(CoapMessageType.Acknowledgement, _message.Type);
            Assert.AreEqual(CoapMessageCode.Content, _message.Code);
            Assert.AreEqual(16962, _message.Id);
            Assert.IsTrue(new byte[] { 0xde, 0xad, 0xbe, 0xef }.SequenceEqual(_message.Token));
            Assert.IsTrue(new List<CoapOption>{
                new Options.ContentFormat(Options.ContentFormatType.ApplicationLinkFormat),
            }.SequenceEqual(_message.Options));

            Assert.IsTrue(Encoding.UTF8.GetBytes("<.well-known/core/>").SequenceEqual(_message.Payload));
        }

        [Test]
        [Category("[RFC7252] Section 3"), Category("Decode")]
        public void TestMessageFormatError()
        {
            Assert.Throws<CoapMessageFormatException>(() =>
            {
                _message.Deserialise(new byte[] { 0x40, 0x00, 0x10, 0x00, 0xFF, 0x12, 0x34 });
            }, "Empty message with payload");

            // Verify that Message.Id was decoded 
            Assert.AreEqual(0x1000, _message.Id);

            Assert.Throws<CoapMessageFormatException>(() =>
            {
                _message.Deserialise(new byte[] { 0x52, 0x00, 0xAA, 0x55, 0x12, 0x34 });
            }, "Empty message with tag");

            // Verify that Message.Id was decoded 
            Assert.AreEqual(0xAA55, _message.Id);

            Assert.Throws<CoapMessageFormatException>(() =>
            {
                _message.Deserialise(new byte[] { 0x60, 0x00, 0xC3, 0x3C, 0xc1, 0x28 });
            }, "Empty message with options");

            // Verify that Message.Id was decoded 
            Assert.AreEqual(0xC33C, _message.Id);

            Assert.Throws<CoapMessageFormatException>(() =>
            {
                _message.Deserialise(new byte[] { 0x40, 0x20, 0x12, 0x34, 0xc1, 0x28 });
            }, "Message with invalid Message.Code class");
        }

        [Test]
        [Category("[RFC7252] Section 3.1"), Category("Encode")]
        public void TestMessageOptionsOutOfOrder()
        {
            // Arrange
            var expectedOptions = new List<CoapOption>
            {
                new Options.UriHost("example.net"),
                new Options.UriPath(".well-known"),
                new Options.UriPath("core"),
                new Options.Accept(Options.ContentFormatType.ApplicationLinkFormat),
            };

            // Act
            _message = new CoapMessage
            {
                Options = new List<CoapOption> {
                    new Options.Accept(Options.ContentFormatType.ApplicationLinkFormat),
                    new Options.UriPath(".well-known"),
                    new Options.UriPath("core"),
                    new Options.UriHost("example.net"),
                }
            };

            // Assert
            Assert.IsTrue(expectedOptions.SequenceEqual(_message.Options));
        }

        [Test]
        [Category("[RFC7252] Section 3.1"), Category("Encode")]
        public void TestMessageFromUri()
        {
            _message.FromUri("coap://example.net/.well-known/core");

            var expectedOptions = new List<CoapOption>
            {
                new Options.UriHost("example.net"),
                new Options.UriPath(".well-known"),
                new Options.UriPath("core"),
            };

            Assert.IsTrue(expectedOptions.SequenceEqual(_message.Options));

            // Test again but using static CreateFromUri method
            var message = CoapMessage.CreateFromUri("coap://example.net/.well-known/core");
            Assert.IsTrue(expectedOptions.SequenceEqual(message.Options));
        }

        [Test]
        [Category("[RFC7252] Section 3.1"), Category("Encode")]
        public void TestMessageToUri()
        {
            _message = new CoapMessage
            {
                Options = new List<CoapOption>
                {
                    new Options.UriHost("example.net"),
                    new Options.UriPath(".well-known"),
                    new Options.UriPath("core"),
                }
            };
            var expected = new Uri("coap://example.net/.well-known/core");
            var actual = _message.GetUri();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [Category("[RFC7252] Section 3.1"), Category("Encode")]
        public void TestMessageToUriDTLS()
        {
            _message = new CoapMessage
            {
                Options = new List<CoapOption>
                {
                    new Options.UriHost("example.net"),
                    new Options.UriPort(Coap.PortDTLS),
                    new Options.UriPath(".well-known"),
                    new Options.UriPath("core"),
                }
            };
            var expected = new Uri("coaps://example.net/.well-known/core");
            var actual = _message.GetUri();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [Category("[RFC7252] Section 3.1"), Category("Encode")]
        public void TestMessageToUriCustomPort()
        {
            _message = new CoapMessage
            {
                Options = new List<CoapOption>
                {
                    new Options.UriHost("example.net"),
                    new Options.UriPort(1234),
                    new Options.UriPath(".well-known"),
                    new Options.UriPath("core"),
                }
            };
            var expected = new Uri("coap://example.net:1234/.well-known/core");
            var actual = _message.GetUri();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [Category("[RFC7252] Section 6.4")]
        public void TestMessageFromUriIPv4()
        {
            _message.FromUri("coap://198.51.100.1:61616//%2F//?%2F%2F&?%26");

            var expectedOptions = new List<CoapOption> {
                new Options.UriHost("198.51.100.1"),
                new Options.UriPort(61616),
                new Options.UriPath(""),
                new Options.UriPath("/"),
                new Options.UriPath(""),
                new Options.UriPath(""),
                new Options.UriQuery("//"),
                new Options.UriQuery("?&"),
            };

            Assert.AreEqual(expectedOptions, _message.Options);

            // Test again but using static CreateFromUri method
            var message = CoapMessage.CreateFromUri("coap://198.51.100.1:61616//%2F//?%2F%2F&?%26");
            Assert.AreEqual(expectedOptions,message.Options);
        }

        [Test]
        [Category("[RFC7252] Section 6.4")]
        public void TestMessageFromUriSpecialChars()
        {
            _message.FromUri("coap://\u307B\u3052.example/%E3%81%93%E3%82%93%E3%81%AB%E3%81%A1%E3%81%AF");

            var expectedOptions = new List<CoapOption>
            {
                new Options.UriHost("xn--18j4d.example"),
                new Options.UriPath("\u3053\u3093\u306b\u3061\u306f"),
            };

            Assert.AreEqual(expectedOptions, _message.Options);

            // Test again but using static CreateFromUri method
            var message = CoapMessage.CreateFromUri("coap://\u307B\u3052.example/%E3%81%93%E3%82%93%E3%81%AB%E3%81%A1%E3%81%AF");
            Assert.AreEqual(expectedOptions, message.Options);
        }
    }
}
