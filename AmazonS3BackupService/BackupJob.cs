using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonS3BackupService
{
    public class BackupJob
    {
        public DateTime LastBackup { get; set; }

        public string Bucket { get; set; }

        public string SearchPattern { get; set; }

        public string LocalRoot { get; set; }

        public string DetinationRoot { get; set; }

        public bool Active { get; set; }
    }
}
