using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Lookbook
    {
        public string ID { get; set; }

        public string Title { get; set; }

        public string CreatedDate { get; set; }

        public string Styling { get; set; }

        public string Photography {get; set;}

        public string Description { get; set; }

        public string MainImage { get; set; }

        public List<string> Images { get; set; }

        public string GetImageSource(string fileName)
        {
            return @"Content\Lookbook\" + this.ID + @"\" + fileName;
        }
    }
}