﻿// Copyright (c) 2020 Yann Crumeyrolle. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using JsonWebToken.Internal;

namespace JsonWebToken
{
    /// <summary>
    /// Defines an encrypted JWT with a <see cref="JwsDescriptor"/> payload.
    /// </summary>
    public sealed class JweDescriptor : JweDescriptor<JwsDescriptor>
    {
        /// <summary>
        /// Initializes an new instance of <see cref="JweDescriptor{TDescriptor}"/>.
        /// </summary>
        public JweDescriptor()
            : base()
        {
        }

        /// <summary>
        /// Initializes an new instance of <see cref="JweDescriptor{TDescriptor}"/>.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="payload"></param>
        public JweDescriptor(JwtObject header, JwsDescriptor payload)
            : base(header, payload)
        {
        }

        /// <summary>
        /// Initializes an new instance of <see cref="JweDescriptor{TDescriptor}"/>.
        /// </summary>
        /// <param name="payload"></param>
        public JweDescriptor(JwsDescriptor payload)
            : base(new JwtObject(3), payload)
        {
        }

        /// <inheritsdoc />
        public override void Validate()
        {
            base.Validate();

            CheckRequiredHeader(HeaderParameters.AlgUtf8, JwtTokenType.Utf8String);
            CheckRequiredHeader(HeaderParameters.EncUtf8, JwtTokenType.Utf8String);
        }
    }
}
