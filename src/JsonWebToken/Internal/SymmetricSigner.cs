// Copyright (c) 2020 Yann Crumeyrolle. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;

namespace JsonWebToken.Internal
{
    /// <summary>
    /// Provides signing and verifying operations using a <see cref="SymmetricJwk"/> and specifying an algorithm.
    /// </summary>
    internal sealed class SymmetricSigner : Signer
    {
        private readonly HmacSha2 _hashAlgorithm;
        private bool _disposed;

        /// <summary>
        /// This is the minimum <see cref="SymmetricJwk"/>.KeySize when creating and verifying signatures.
        /// </summary>
        public const int DefaultMinimumSymmetricKeySizeInBits = 128;

        private readonly int _hashSizeInBytes;
        private readonly int _base64HashSizeInBytes;
        private int _minimumKeySizeInBits = DefaultMinimumSymmetricKeySizeInBits;

        public SymmetricSigner(SymmetricJwk key, SignatureAlgorithm algorithm)
            : this(key.AsSpan(), algorithm)
        {
        }

        public SymmetricSigner(ReadOnlySpan<byte> key, SignatureAlgorithm algorithm)
            : base(algorithm)
        {
            if (key.Length << 3 < MinimumKeySizeInBits)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_AlgorithmRequireMinimumKeySize(key.Length << 3, algorithm.Name, MinimumKeySizeInBits);
            }

            if (algorithm.Category != AlgorithmCategory.Hmac)
            {
                ThrowHelper.ThrowNotSupportedException_SignatureAlgorithm(algorithm);
            }

            _hashSizeInBytes = Algorithm.RequiredKeySizeInBits >> 2;
            _base64HashSizeInBytes = Base64Url.GetArraySizeRequiredToEncode(_hashSizeInBytes);
            _hashAlgorithm = Algorithm.Id switch
            {
                Algorithms.HmacSha256 => new HmacSha256(key),
                Algorithms.HmacSha384 => new HmacSha384(key),
                Algorithms.HmacSha512 => new HmacSha512(key),
                _ => new NotSupportedHmacSha(algorithm)
            };
        }

        /// <inheritsdoc />
        public override int HashSizeInBytes => _hashSizeInBytes;

        public override int Base64HashSizeInBytes => _base64HashSizeInBytes;

        /// <summary>
        /// Gets or sets the minimum <see cref="SymmetricJwk"/>.KeySize.
        /// </summary>
        public int MinimumKeySizeInBits
        {
            get
            {
                return _minimumKeySizeInBits;
            }

            set
            {
                if (value < DefaultMinimumSymmetricKeySizeInBits)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException_MustBeAtLeast(ExceptionArgument.value, DefaultMinimumSymmetricKeySizeInBits);
                }

                _minimumKeySizeInBits = value;
            }
        }

        /// <inheritsdoc />
        public override bool TrySign(ReadOnlySpan<byte> input, Span<byte> destination, out int bytesWritten)
        {
            if (_disposed)
            {
                ThrowHelper.ThrowObjectDisposedException(GetType());
            }

            _hashAlgorithm.ComputeHash(input, destination);
            bytesWritten = destination.Length;
            return true;
        }

        /// <inheritsdoc />
        public override bool Verify(ReadOnlySpan<byte> input, ReadOnlySpan<byte> signature)
        {
            if (_disposed)
            {
                ThrowHelper.ThrowObjectDisposedException(GetType());
            }

            Span<byte> hash = stackalloc byte[_hashSizeInBytes];
            _hashAlgorithm.ComputeHash(input, hash);
            return CryptographicOperations.FixedTimeEquals(signature, hash);
        }

        /// <inheritsdoc />
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _hashAlgorithm.Clear();
                }

                _disposed = true;
            }
        }

        private sealed class NotSupportedHmacSha : HmacSha2
        {
            public NotSupportedHmacSha(SignatureAlgorithm algorithm)
                : base(ShaNull.Shared, default)
            {
                ThrowHelper.ThrowNotSupportedException_Algorithm(algorithm.Name);
            }

            private sealed class ShaNull : Sha2
            {
                public static readonly ShaNull Shared = new ShaNull();

                public override int HashSize => 0;

                public override int BlockSize => 0;

                public override void ComputeHash(ReadOnlySpan<byte> source, ReadOnlySpan<byte> prepend, Span<byte> destination, Span<byte> w)
                {
                }

                public override int GetWorkingSetSize(int sourceLength)
                {
                    return 0;
                }
            }
        }
    }
}
