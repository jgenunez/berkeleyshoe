using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class LookbookController : Controller
    {
        private List<Lookbook> _lookbooks = new List<Lookbook>();

        // GET: Lookbook
        public ActionResult Index()
        {
            var lookbooks = this.ListLookbook();

            return View(lookbooks);
        }

        public ActionResult Details(string id)
        {
            var lookbooks = this.ListLookbook();

            return View(lookbooks.Single(p => p.ID.Equals(id)));
        }

        private List<Lookbook> ListLookbook()
        {
            List<Lookbook> lookbooks = new List<Lookbook>();

            string lookbookRoot = "/Content/Lookbook/";

            string baseDir = Server.MapPath(lookbookRoot);
            
            var dirs = Directory.EnumerateDirectories(baseDir);

            XmlSerializer serializer = new XmlSerializer(typeof(Lookbook));

            foreach (string dir in dirs)
            {
                if (System.IO.File.Exists(dir + @"\lookbook.xml"))
                {
                    Lookbook lookbook = null;

                    using (FileStream stream = new FileStream(dir + @"\lookbook.xml", FileMode.OpenOrCreate))
                    {
                        lookbook = serializer.Deserialize(stream) as Lookbook;
                    }

                    lookbook.ID = dir.Replace(baseDir, "").Replace(".xml", "");

                    var images = Directory.EnumerateFiles(dir, "*.jpg").Select(p => Path.GetFileName(p));

                    if (images.Any(p => p.Contains("main.jpg")))
                    {
                        lookbook.MainImage = images.First(p => p.Contains("main.jpg"));
                    }
                    else
                    {
                        lookbook.MainImage = images.FirstOrDefault();
                    }

                    foreach (var image in images)
                    {
                        lookbook.Images.Add(image);
                    }

                    lookbooks.Add(lookbook);
                }
                
            }


            return lookbooks;
        }
    }
}