﻿using System;
using Xunit;

namespace JsonWebToken.Tests.Cryptography
{
    public class HmacSha256Tests : HmacShaTests
    {
        protected override HmacSha2 Create(ReadOnlySpan<byte> key)
        {
            return new HmacSha256(key);
        }

        protected override Sha2 CreateShaAlgorithm()
        {
            return new Sha256();
        }

        protected override int BlockSize { get { return 64; } }

        [Fact]
        public void HmacSha256_Rfc4231_1()
        {
            VerifyHmac(1, "b0344c61d8db38535ca8afceaf0bf12b881dc200c9833da726e9376c2e32cff7");
        }

        [Fact]
        public void HmacSha256_Rfc4231_2()
        {
            VerifyHmac(2, "5bdcc146bf60754e6a042426089575c75a003f089d2739839dec58b964ec3843");
        }

        [Fact]
        public void HmacSha256_Rfc4231_3()
        {
            VerifyHmac(3, "773ea91e36800e46854db8ebd09181a72959098b3ef8c122d9635514ced565fe");
        }

        [Fact]
        public void HmacSha256_Rfc4231_4()
        {
            VerifyHmac(4, "82558a389a443c0ea4cc819899f2083a85f0faa3e578f8077a2e3ff46729665b");
        }

        [Fact]
        public void HmacSha256_Rfc4231_5()
        {
            // RFC 4231 only defines the first 128 bits of this value.
            VerifyHmac(5, "a3b6167473100ee06e0c796c2955552b00000000000000000000000000000000", 128 / 8);
        }

        [Fact]
        public void HmacSha256_Rfc4231_6()
        {
            VerifyHmac(6, "60e431591ee0b67f0d8a26aacbf5b77f8e0bc6213728c5140546040f0ee37f54");
        }

        [Fact]
        public void HmacSha256_Rfc4231_7()
        {
            VerifyHmac(7, "9b09ffa71b942fcb27635fbcd5b0e944bfdc63644f0713938a7f51535c3a35e2");
        }

        [Fact]
        public void HmacSha256_EmptyKey()
        {
            VerifyHmac_Empty("5F8C56F9C91C6CAE4BF58F114BDCE53CF0ED424B191BAAC469181A1194A52260");
        }
    }
}
