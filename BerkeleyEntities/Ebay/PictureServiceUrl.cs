﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BerkeleyEntities
{
    public partial class EbayPictureServiceUrl
    {
        public bool IsExpired()
        {
            if (this.Relations.Any(p => p.Listing.Status.Equals("Active") || p.Listing.EndTime > DateTime.UtcNow.AddDays(-30)) || this.TimeUploaded > DateTime.UtcNow.AddHours(-10))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public string GetVariationCode()
        {
            return this.LocalName.Split(new char[1] { '-' })[0].Split(new char[1] { '_' })[1];
        }
    }
}