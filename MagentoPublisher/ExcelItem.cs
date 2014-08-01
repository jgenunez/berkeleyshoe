using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;

namespace MagentoUpload
{
    class ExcelItem
    {

        public ExcelItem(string SKU)
        {
            string[] details = SKU.Split(new Char[1] { '-' });

            switch (details.Length)
            {
                case 1: Style = details[0];
                    break;
                case 2: Style = details[0];
                    //size = details[1];
                    //width = "M";
                    break;
                case 3: Style = details[0];
                    //size = details[1];
                    //width = details[2];
                    break;
                case 4: Style = details[0] + "-" + details[1];
                    //size = details[2];
                    //width = details[3];
                    break;
            }
        }

        public string Style { get; set; }

        public string SKU { get; set; }

        public string Quantity { get; set; }

        public string Price { get; set; }

        public bsi_posting Posting { get; set; }

        public Item Item { get; set; }
    }
}
