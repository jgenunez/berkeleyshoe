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

        public DateTime DateOfCompletion { get; set; }

        public string Styling { get; set; }

        public string Photography {get; set;}

        public string Url { get; set; }

        public string Description { get; set; }
    }
}