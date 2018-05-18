﻿using Newtonsoft.Json;
using System;

namespace JsonWebToken
{
    public class SymmetricJwk : JsonWebKey
    {
        private string _k;

        public SymmetricJwk(byte[] bytes)
            : this()
        {
            RawK = bytes;
            _k = Base64UrlEncoder.Encode(bytes);
        }

        public SymmetricJwk()
        {
            Kty = JsonWebAlgorithmsKeyTypes.Octet;
        }

        /// <summary>
        /// Gets or sets the 'k' (Key Value).
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, PropertyName = JsonWebKeyParameterNames.K, Required = Required.Default)]
        public string K
        {
            get => _k;
            set
            {
                _k = value;
                if (value != null)
                {
                    RawK = Base64UrlEncoder.DecodeBytes(value);
                }
            }
        }

        [JsonIgnore]
        public byte[] RawK { get; private set; }

        public override int KeySize => RawK?.Length != 0 ? RawK.Length << 3 : 0;

        public override int SignatureSize
        {
            get
            {
                switch (Alg)
                {
                    case SecurityAlgorithms.HmacSha256:
                        return 32;
                    case SecurityAlgorithms.HmacSha384:
                        return 48;
                    case SecurityAlgorithms.HmacSha512:
                        return 64;
                }

                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Returns a new instance of <see cref="SymmetricJwk"/>.
        /// </summary>
        /// <param name="bytes">An array of <see cref="byte"/> that contains the key in binary.</param>
        public static SymmetricJwk FromByteArray(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            return new SymmetricJwk(bytes);
        }

        /// <summary>
        /// Returns a new instance of <see cref="SymmetricJwk"/>.
        /// </summary>
        /// <param name="key">The Base64Url encoded string that contains the key.</param>
        public static SymmetricJwk FromBase64Url(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return new SymmetricJwk()
            {
                K = key
            };
        }

        public override bool IsSupportedAlgorithm(string algorithm)
        {
            switch (algorithm)
            {
                case SecurityAlgorithms.Aes128CbcHmacSha256:
                case SecurityAlgorithms.Aes192CbcHmacSha384:
                case SecurityAlgorithms.Aes256CbcHmacSha512:
                case SecurityAlgorithms.Aes128KW:
                case SecurityAlgorithms.Aes256KW:
                case SecurityAlgorithms.HmacSha256:
                case SecurityAlgorithms.HmacSha384:
                case SecurityAlgorithms.HmacSha512:
                    return true;
            }

            return false;
        }

        public override SignatureProvider CreateSignatureProvider(string algorithm, bool willCreateSignatures)
        {
            if (IsSupportedAlgorithm(algorithm))
            {
                return new SymmetricSignatureProvider(this, algorithm);
            }

            return null;
        }

        public override KeyWrapProvider CreateKeyWrapProvider(string algorithm)
        {
            if (IsSupportedAlgorithm(algorithm))
            {
                return new SymmetricKeyWrapProvider(this, algorithm);
            }

            return null;
        }

        private bool IsSupportedAuthenticatedEncryptionAlgorithm(string algorithm)
        {
            if (string.IsNullOrEmpty(algorithm))
            {
                return false;
            }

            if (!(algorithm.Equals(SecurityAlgorithms.Aes128CbcHmacSha256, StringComparison.Ordinal)
               || algorithm.Equals(SecurityAlgorithms.Aes192CbcHmacSha384, StringComparison.Ordinal)
               || algorithm.Equals(SecurityAlgorithms.Aes256CbcHmacSha512, StringComparison.Ordinal)))
            {
                return false;
            }

            return true;
        }

        public override AuthenticatedEncryptionProvider CreateAuthenticatedEncryptionProvider(string algorithm)
        {
            if (IsSupportedAuthenticatedEncryptionAlgorithm(algorithm))
            {
                return new AuthenticatedEncryptionProvider(this, algorithm);
            }

            return null;
        }
    }
}
