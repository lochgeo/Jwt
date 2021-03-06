﻿// Copyright (c) 2020 Yann Crumeyrolle. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

#if !SUPPORT_AESGCM
using System;

namespace JsonWebToken.Internal
{
    internal sealed class AesGcmKeyUnwrapper : KeyUnwrapper
    {
        public AesGcmKeyUnwrapper(SymmetricJwk key, EncryptionAlgorithm encryptionAlgorithm, KeyManagementAlgorithm algorithm)
            : base(key, encryptionAlgorithm, algorithm)
        {
            ThrowHelper.ThrowNotSupportedException_AlgorithmForKeyWrap(algorithm);
        }

        /// <inheritsdoc />
        public override int GetKeyUnwrapSize(int wrappedKeySize)
            => throw new NotImplementedException();

        /// <inheritsdoc />
        public override bool TryUnwrapKey(ReadOnlySpan<byte> keyBytes, Span<byte> destination, JwtHeader header, out int bytesWritten)
            => throw new NotImplementedException();

        /// <inheritsdoc />
        protected override void Dispose(bool disposing)
        {
        }
    }
}
#endif