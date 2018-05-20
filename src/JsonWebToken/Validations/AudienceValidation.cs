﻿using System;
using System.Collections.Generic;

namespace JsonWebToken.Validations
{
    public class AudienceValidation : IValidation
    {
        private readonly IEnumerable<string> _audiences;

        public AudienceValidation(IEnumerable<string> audiences)
        {
            _audiences = audiences;
        }

        public TokenValidationResult TryValidate(JsonWebToken jwt)
        {
            bool missingAudience = true;
            foreach (string audience in jwt.Audiences)
            {
                missingAudience = false;
                if (string.IsNullOrWhiteSpace(audience))
                {
                    continue;
                }

                foreach (string validAudience in _audiences)
                {
                    if (string.Equals(audience, validAudience, StringComparison.Ordinal))
                    {
                        return TokenValidationResult.Success(jwt);
                    }
                }
            }

            if (missingAudience)
            {
                return TokenValidationResult.MissingAudience(jwt);
            }

            return TokenValidationResult.InvalidAudience(jwt);
        }
    }
}
