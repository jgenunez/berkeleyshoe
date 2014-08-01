using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BerkeleyEntities
{
    public class PictureSet
    {
        public PictureSet()
        {
            this.Pictures = new List<PictureInfo>();
        }

        public string Name { get; set; }

        public List<PictureInfo> Pictures { get; set; }
    }

    public class PictureInfo
    {
        public string Name { get; set; }

        public string VariationAttributeValue { get; set; }

        public string Path { get; set; }

        public DateTime LastModified { get; set; }
    }

    public class PictureSetRepository
    {
        private string _root = @"P:\products\";
        private Dictionary<string, IEnumerable<string>> _cachedFileNames = new Dictionary<string, IEnumerable<string>>();

        public PictureSet GetPictureSet(string brand, string name)
        {
            string brandRoot = _root + brand + @"\";

            if (!Directory.Exists(brandRoot))
            {
                throw new NotImplementedException("could not find directory:" + brandRoot);
            }

            PictureSet picSet = new PictureSet();
            picSet.Name = name;

            var targetPaths = GetFileNamesByBrandDir(brandRoot).Where(p => p.Contains(name + "-") || p.Contains(name + "."));

            foreach (string path in targetPaths)
            {
                PictureInfo picInfo = new PictureInfo();
                picInfo.Path = path;
                picInfo.LastModified = File.GetLastWriteTimeUtc(path);
                picInfo.Name = Path.GetFileName(path).Split(new Char[1] { '.' })[0];

                picSet.Pictures.Add(picInfo);
            }

            return picSet;
        }



        private string[] GetFileNamesByBrandDir(string path)
        {
            if (!_cachedFileNames.ContainsKey(path))
            {
                var fileData = Directory.GetFiles(path);
                _cachedFileNames.Add(path, fileData);
            }

            return _cachedFileNames[path].ToArray();
        }


    }
}
