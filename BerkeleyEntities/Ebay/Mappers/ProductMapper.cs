﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBay.Service.Core.Soap;
using BerkeleyEntities;
using System.Globalization;

namespace EbayServices
{
    public abstract class ProductMapper
    {
        protected Item _item;

        public ProductMapper(Item item)
        {
            _item = item;
        }

        public string CategoryID
        {
            get { return _item.Department.code;  }
        }

        public abstract List<NameValueListType> GetItemSpecifics();

        public abstract List<NameValueListType> GetVariationSpecifics();

        public List<KeyValuePair<string,BerkeleyEntities.Attribute>> GetAttributes()
        {
            return _item.Attributes.ToList();
        }

        protected NameValueListType BuildItemSpecific(string name, string[] values)
        {
            NameValueListType itemSpecific = new NameValueListType();
            itemSpecific.Name = name;
            itemSpecific.Value = new StringCollection(values);

            return itemSpecific;
        }

        protected string ToTitleCase(string word)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(word);
        }
    }
}