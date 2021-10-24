﻿using System;

namespace Authentication.Configuration
{
    public class JwtConfig
    {
        public string Secret { get; set; }
        public TimeSpan ExpiryTimeFrame { get; set; }
    }
}
