﻿// Copyright (c) 2018 Yann Crumeyrolle. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for more information.

using JsonWebToken.Internal;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace JsonWebToken
{
    /// <summary>
    /// Defines an abstract class for representing a JWT.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    public abstract class JwtDescriptor
    {
        private static readonly ReadOnlyDictionary<ReadOnlyMemory<byte>, JwtTokenType[]> DefaultRequiredHeaderParameters
            = new ReadOnlyDictionary<ReadOnlyMemory<byte>, JwtTokenType[]>(new Dictionary<ReadOnlyMemory<byte>, JwtTokenType[]>());
        private Jwk _key;

        /// <summary>
        /// Initializes a new instance of <see cref="JwtDescriptor"/>.
        /// </summary>
        protected JwtDescriptor()
            : this(new JwtObject())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="JwtDescriptor"/>.
        /// </summary>
        /// <param name="header"></param>
        protected JwtDescriptor(JwtObject header)
        {
            Header = header;
        }

        /// <summary>
        /// Gets the parameters header.
        /// </summary>
        public JwtObject Header { get; }

        /// <summary>
        /// Gets the <see cref="Jwt"/> used.
        /// </summary>
        public Jwk Key
        {
            get => _key;
            set
            {
                _key = value;
                if (value != null)
                {
                    if (value.Alg != null)
                    {
                        Algorithm = value.Alg;
                    }

                    if (value.Kid != null)
                    {
                        KeyId = value.Kid;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the required header parameters.
        /// </summary>
        protected virtual ReadOnlyDictionary<ReadOnlyMemory<byte>, JwtTokenType[]> RequiredHeaderParameters => DefaultRequiredHeaderParameters;

        /// <summary>
        /// Gets or sets the algorithm header.
        /// </summary>
        public string Algorithm
        {
            get => GetHeaderParameter<string>(HeaderParameters.AlgUtf8);
            set => SetHeaderParameter(HeaderParameters.AlgUtf8, value);
        }

        /// <summary>
        /// Gets or sets the key identifier header parameter.
        /// </summary>
        public string KeyId
        {
            get => GetHeaderParameter<string>(HeaderParameters.KidUtf8);
            set => SetHeaderParameter(HeaderParameters.KidUtf8, value);
        }

        /// <summary>
        /// Gets or sets the JWKS URL header parameter.
        /// </summary>
        public string JwkSetUrl
        {
            get => GetHeaderParameter<string>(HeaderParameters.JkuUtf8);
            set => SetHeaderParameter(HeaderParameters.JkuUtf8, value);
        }

        /// <summary>
        /// Gets or sets the JWK header parameter.
        /// </summary>
        public Jwk Jwk
        {
            get => throw new NotSupportedException(); // GetHeaderParameter<Jwk>(HeaderParameters.JwkUtf8);
            set => throw new NotSupportedException(); //SetHeaderParameter(HeaderParameters.Jwk, value);
        }

        /// <summary>
        /// Gets or sets the X509 URL header parameter.
        /// </summary>
        public string X509Url
        {
            get => GetHeaderParameter<string>(HeaderParameters.X5uUtf8);
            set => SetHeaderParameter(HeaderParameters.X5uUtf8, value);
        }

        /// <summary>
        /// Gets or sets the X509 certification chain header.
        /// </summary>
        public List<string> X509CertificateChain
        {
            get => GetHeaderParameters<string>(HeaderParameters.X5cUtf8);
            set => SetHeaderParameter(HeaderParameters.X5cUtf8, value);
        }

        /// <summary>
        /// Gets or sets the X509 certificate SHA-1 thumbprint header parameter.
        /// </summary>
        public string X509CertificateSha1Thumbprint
        {
            get => GetHeaderParameter<string>(HeaderParameters.X5tUtf8);
            set => SetHeaderParameter(HeaderParameters.X5tUtf8, value);
        }

        /// <summary>
        /// Gets or sets the JWT type 'typ' header parameter.
        /// </summary>
        public string Type
        {
            get => GetHeaderParameter<string>(HeaderParameters.TypUtf8);
            set => SetHeaderParameter(HeaderParameters.TypUtf8, value);
        }

        /// <summary>
        /// Gets or sets the content type header parameter.
        /// </summary>
        public string ContentType
        {
            get => GetHeaderParameter<string>(HeaderParameters.CtyUtf8);
            set => SetHeaderParameter(HeaderParameters.CtyUtf8, value);
        }

        /// <summary>
        /// Gets or sets the critical header parameter.
        /// </summary>
        public List<string> Critical
        {
            get => GetHeaderParameters<string>(HeaderParameters.CritUtf8);
            set => SetHeaderParameter(HeaderParameters.CritUtf8, value);
        }

        /// <summary>
        /// Encodes the current <see cref="JwtDescriptor"/> into it <see cref="string"/> representation.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public abstract void Encode(EncodingContext context, IBufferWriter<byte> output);

        /// <summary>
        /// Gets the header parameter for a specified header name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="utf8Name"></param>
        /// <returns></returns>
        protected T GetHeaderParameter<T>(ReadOnlyMemory<byte> utf8Name)
        {
            if (Header.TryGetValue(utf8Name, out var value))
            {
                return (T)value.Value;
            }

            return default;
        }

        /// <summary>
        /// Sets the header parameter for a specified header name.
        /// </summary>
        /// <param name="utf8Name"></param>
        /// <param name="value"></param>
        protected void SetHeaderParameter(ReadOnlyMemory<byte> utf8Name, string value)
        {
            if (value != null)
            {
                Header.Add(new JwtProperty(utf8Name, value));
            }
            else
            {
                Header.Add(new JwtProperty(utf8Name));
            }
        }

        /// <summary>
        /// Sets the header parameter for a specified header name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected void SetHeaderParameter(string name, string value)
        {
            SetHeaderParameter(Encoding.UTF8.GetBytes(name), value);
        }

        ///// <summary>
        ///// Sets the header parameter for a specified header name.
        ///// </summary>
        ///// <param name="headerName"></param>
        ///// <param name="value"></param>
        //protected void SetHeaderParameter(string headerName, object value)
        //{
        //    if (value != null)
        //    {
        //        Header.Add(new JwtProperty(Encoding.UTF8.GetBytes(headerName), JObject.FromObject(value)));
        //    }
        //    else
        //    {
        //        Header.Add(new JwtProperty(Encoding.UTF8.GetBytes(headerName)));
        //    }
        //}

        /// <summary>
        /// Sets the header parameter for a specified header name.
        /// </summary>
        /// <param name="utf8Name"></param>
        /// <param name="value"></param>
        protected void SetHeaderParameter(ReadOnlyMemory<byte> utf8Name, List<string> value)
        {
            if (value != null)
            {
                Header.Add(new JwtProperty(utf8Name, new JwtArray(value)));
            }
            else
            {
                Header.Add(new JwtProperty(utf8Name));
            }
        }

        /// <summary>
        /// Sets the header parameter for a specified header name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected void SetHeaderParameter(string name, List<string> value)
        {
            SetHeaderParameter(Encoding.UTF8.GetBytes(name), value);
        }

        /// <summary>
        /// Gets the list of header parameters for a header name.
        /// </summary>
        /// <param name="utf8Name"></param>
        /// <returns></returns>
        protected List<T> GetHeaderParameters<T>(ReadOnlyMemory<byte> utf8Name)
        {
            if (Header.TryGetValue(utf8Name, out JwtProperty value))
            {
                if (value.Type == JwtTokenType.Array)
                {
                    return (List<T>)value.Value;
                }

                var list = new List<T> { (T)value.Value };
                return list;
            }

            return null;
        }

        /// <summary>
        /// Validates the current <see cref="JwtDescriptor"/>.
        /// </summary>
        public virtual void Validate()
        {
            foreach (var header in RequiredHeaderParameters)
            {
                if (!Header.TryGetValue(header.Key, out var token) || token.Type == JwtTokenType.Null)
                {
                    Errors.ThrowHeaderIsRequired(header.Key);
                }

                bool headerFound = false;
                for (int i = 0; i < header.Value.Length; i++)
                {
                    if (token.Type == header.Value[i])
                    {
                        headerFound = true;
                        break;
                    }
                }

                if (!headerFound)
                {
                    Errors.ThrowHeaderMustBeOfType(header);
                }
            }
        }
    }
}
