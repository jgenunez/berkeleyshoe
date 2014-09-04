using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace BerkeleyEntities
{

    public class PictureInfo
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public DateTime LastModified { get; set; }
    }

    public class PictureSetRepository
    {
        private string _root = @"P:\products\";
        private Dictionary<string, IEnumerable<string>> _cachedFileNames = new Dictionary<string, IEnumerable<string>>();

        public List<PictureInfo> GetPictures(string brand, List<string> skus)
        {
            string brandRoot = _root + brand + @"\";

            if (!Directory.Exists(brandRoot))
            {
                throw new FileNotFoundException("could not find directory:" + brandRoot);
            }

            List<PictureInfo> pics = new List<PictureInfo>();

            foreach (var group in skus.GroupBy(p => p.Split(new Char[1] {'-'})[0]))
            {
                var targetPaths = GetFileNamesByBrandDir(brandRoot).Where(p => p.ToUpper().Trim().Contains(group.Key + ".") || p.ToUpper().Trim().Contains(group.Key + "-"));

                foreach(string path in targetPaths)
                {
                    string picName = Path.GetFileName(path).Split(new Char[1] { '.' })[0];

                    if (!picName.Contains("_"))
                    {
                        PictureInfo picInfo = new PictureInfo();
                        picInfo.Path = path;
                        picInfo.LastModified = File.GetLastWriteTimeUtc(path);
                        picInfo.Name = picName;
                        pics.Add(picInfo);                
                    }
                    else
                    {
                        if (group.Any(p => Regex.IsMatch(p, picName.Replace("_", "-.*-?"))))
                        {
                            PictureInfo picInfo = new PictureInfo();
                            picInfo.Path = path;
                            picInfo.LastModified = File.GetLastWriteTimeUtc(path);
                            picInfo.Name = picName;
                            pics.Add(picInfo);
                        }
                    }
                }
            }

            return pics;
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
