﻿using Newtonsoft.Json;
using System;
using System.Security.Cryptography;

namespace JsonWebToken
{
    public class EccJwk : AsymmetricJwk
    {
        private string _x;
        private string _y;

        public EccJwk(ECParameters parameters)
            : this()
        {
            parameters.Validate();

            RawD = parameters.D;
            RawX = parameters.Q.X;
            RawY = parameters.Q.Y;
            switch (parameters.Curve.Oid.FriendlyName)
            {
                case "nistP256":
                    Crv = EllipticalCurves.P256;
                    break;
                case "nistP384":
                    Crv = EllipticalCurves.P384;
                    break;
                case "nistP521":
                    Crv = EllipticalCurves.P521;
                    break;
                default:
                    Errors.ThrowNotSupportedCurve(parameters.Curve.Oid.FriendlyName);
                    break;
            }
        }

        private EccJwk(string crv, byte[] d, byte[] x, byte[] y)
        {
            Crv = crv;
            RawD = CloneByteArray(d);
            RawX = CloneByteArray(x);
            RawY = CloneByteArray(y);
        }

        public EccJwk()
        {
            Kty = JsonWebKeyTypeNames.EllipticCurve;
        }

        /// <summary>
        /// Gets or sets the 'crv' (Curve).
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, PropertyName = JsonWebKeyParameterNames.Crv, Required = Required.Default)]
        public string Crv { get; set; }

        /// <summary>
        /// Gets or sets the 'x' (X Coordinate).
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, PropertyName = JsonWebKeyParameterNames.X, Required = Required.Default)]
        public string X
        {
            get
            {
                if (_x == null)
                {
                    if (RawX != null && RawX.Length != 0)
                    {
                        _x = Base64Url.Encode(RawX);
                    }
                }

                return _x;
            }
            set
            {
                _x = value;
                if (value != null)
                {
                    RawX = Base64Url.Base64UrlDecode(value);
                }
                else
                {
                    RawX = null;
                }
            }
        }

        [JsonIgnore]
        public byte[] RawX { get; private set; }

        /// <summary>
        /// Gets or sets the 'y' (Y Coordinate).
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, PropertyName = JsonWebKeyParameterNames.Y, Required = Required.Default)]
        public string Y
        {
            get
            {
                if (_y == null)
                {
                    if (RawY != null && RawY.Length != 0)
                    {
                        _y = Base64Url.Encode(RawY);
                    }
                }

                return _y;
            }
            set
            {
                _y = value;
                if (value != null)
                {
                    RawY = Base64Url.Base64UrlDecode(value);
                }
                else
                {
                    RawY = null;
                }
            }
        }

        [JsonIgnore]
        public byte[] RawY { get; private set; }

        public override bool HasPrivateKey => RawD != null;

        public override int KeySizeInBits
        {
            get
            {
                switch (Crv)
                {
                    case EllipticalCurves.P256:
                        return 256;
                    case EllipticalCurves.P384:
                        return 384;
                    case EllipticalCurves.P521:
                        return 521;
                    default:
                        Errors.ThrowNotSupportedCurve(Crv);
                        return 0;
                }
            }
        }

        public ECDsa CreateECDsa(SignatureAlgorithm algorithm, bool usePrivateKey)
        {
            int validKeySize = ValidKeySize(algorithm);
            if (KeySizeInBits != validKeySize)
            {
                Errors.ThrowInvalidEcdsaKeySize(this, algorithm, validKeySize, KeySizeInBits);
            }

            return ECDsa.Create(ExportParameters(usePrivateKey));
        }

        private static int ValidKeySize(SignatureAlgorithm algorithm)
        {
            return algorithm.RequiredKeySizeInBits;
        }

        public override bool IsSupported(SignatureAlgorithm algorithm)
        {
            return algorithm.Category == AlgorithmCategory.EllipticCurve;
        }

        public override bool IsSupported(KeyManagementAlgorithm algorithm)
        {
            return algorithm.Category == AlgorithmCategory.EllipticCurve;
        }

        public override bool IsSupported(EncryptionAlgorithm algorithm)
        {
            return algorithm.Category == EncryptionTypes.AesHmac || algorithm.Category == EncryptionTypes.AesGcm;
        }

        public override Signer CreateSigner(SignatureAlgorithm algorithm, bool willCreateSignatures)
        {
            if (algorithm == null)
            {
                return null;
            }

            if (IsSupported(algorithm))
            {
                return new EcdsaSigner(this, algorithm, willCreateSignatures);
            }

            return null;
        }

        public override KeyWrapper CreateKeyWrapper(EncryptionAlgorithm encryptionAlgorithm, KeyManagementAlgorithm contentEncryptionAlgorithm)
        {
#if NETCOREAPP2_1
            return new EcdhKeyWrapper(this, encryptionAlgorithm, contentEncryptionAlgorithm);
#else
            return null;
#endif
        }

        public ECParameters ExportParameters(bool includePrivateParameters = false)
        {
            var parameters = new ECParameters
            {
                Q = new ECPoint
                {
                    X = RawX,
                    Y = RawY
                }
            };
            if (includePrivateParameters)
            {
                parameters.D = RawD;
            }

            switch (Crv)
            {
                case EllipticalCurves.P256:
                    parameters.Curve = ECCurve.NamedCurves.nistP256;
                    break;
                case EllipticalCurves.P384:
                    parameters.Curve = ECCurve.NamedCurves.nistP384;
                    break;
                case EllipticalCurves.P521:
                    parameters.Curve = ECCurve.NamedCurves.nistP521;
                    break;
                default:
                    Errors.ThrowNotSupportedCurve(Crv);
                    break;
            }

            return parameters;
        }

        public static EccJwk GenerateKey(string curveId, bool withPrivateKey)
        {
            if (string.IsNullOrEmpty(curveId))
            {
                throw new ArgumentNullException(nameof(curveId));
            }

            ECCurve curve = default;
            switch (curveId)
            {
                case EllipticalCurves.P256:
                    curve = ECCurve.NamedCurves.nistP256;
                    break;
                case EllipticalCurves.P384:
                    curve = ECCurve.NamedCurves.nistP384;
                    break;
                case EllipticalCurves.P521:
                    curve = ECCurve.NamedCurves.nistP521;
                    break;
                default:
                    Errors.ThrowNotSupportedCurve(curveId);
                    break;
            }

            using (var ecdsa = ECDsa.Create())
            {
                ecdsa.GenerateKey(curve);
                var parameters = ecdsa.ExportParameters(withPrivateKey);
                return FromParameters(parameters);
            }
        }

        public override JsonWebKey ExcludeOptionalMembers()
        {
            return new EccJwk(Crv, RawD, RawX, RawY);
        }

        /// <summary>
        /// Returns a new instance of <see cref="EccJwk"/>.
        /// </summary>
        /// <param name="parameters">A <see cref="byte"/> that contains the key parameters.</param>
        public static EccJwk FromParameters(ECParameters parameters, bool computeThumbprint = false)
        {
            var key = new EccJwk(parameters);
            if (computeThumbprint)
            {
                key.Kid = key.ComputeThumbprint(false);
            }

            return key;
        }

        public override byte[] ToByteArray()
        {
#if NETCOREAPP2_1
            using (var ecdh = ECDiffieHellman.Create(ExportParameters()))
            {
                return ecdh.PublicKey.ToByteArray();
            }
#else
            throw new NotImplementedException();
#endif
        }
    }
}