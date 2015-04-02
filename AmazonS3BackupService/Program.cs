using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;

using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using Amazon.S3;
using Amazon.S3.Model;
using System.Xml.Serialization;
using NLog;

namespace AmazonS3BackupService
{
    class Program
    {
        private const string BACKUP_JOB_FILE = "BackupJobs.xml";

        private static IAmazonS3 _s3Client;

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            try
            {
                _s3Client = AWSClientFactory.CreateAmazonS3Client();

                string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), BACKUP_JOB_FILE);

                XmlSerializer serializer = new XmlSerializer(typeof(List<BackupJob>));

                List<BackupJob> jobs = (List<BackupJob>)serializer.Deserialize(new FileStream(path, FileMode.Open));

                foreach (var job in jobs.Where(p => p.Active))
                {
                    ExecuteBackupJob(job);

                    _logger.Info("finished uploading: " + job.LocalRoot);
                }

                serializer.Serialize(new FileStream(path, FileMode.Create), jobs);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + " " + ex.StackTrace);
            }
        }

        public static void ExecuteBackupJob(BackupJob backupJob)
        {
            DateTime currentBackup = DateTime.Now;

            DirectoryInfo localRoot = new DirectoryInfo(backupJob.LocalRoot);

            var dirs = localRoot.GetDirectories("*", SearchOption.AllDirectories).Where(p => p.LastWriteTime >= backupJob.LastBackup);

            foreach (var dir in dirs)
            {
                var files = dir.GetFiles(backupJob.SearchPattern).Where(p => p.LastWriteTime >= backupJob.LastBackup);

                foreach (var file in files)
                {
                    try
                    {
                        string key = file.FullName.Replace(backupJob.LocalRoot, backupJob.DetinationRoot).Replace("\\", "/");

                        PushToAmazons3(backupJob.Bucket, key, file);

                        _logger.Info(string.Format("{0} uploaded", file.Name));
                    }
                    catch (AmazonS3Exception ex)
                    {
                        _logger.Error(string.Format("{0} could not upload. Request ID: {1}  Error Code: {2}  Message: {3}", file.Name, ex.RequestId, ex.ErrorCode, ex.Message));
                    }
                }
            }

            backupJob.LastBackup = currentBackup;
        }   

        public static void PushToAmazons3(string bucket, string key, FileInfo file)
        {
            PutObjectRequest request = new PutObjectRequest();
            request.BucketName = bucket;
            request.FilePath = file.FullName;
            request.Key = key;

            PutObjectResponse response = _s3Client.PutObject(request);
        }

        public static void SaveLastBackupTime(DateTime lastBackup)
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), BACKUP_JOB_FILE);

            XmlSerializer serializer = new XmlSerializer(typeof(DateTime));

            serializer.Serialize(new FileStream(path, FileMode.Create), lastBackup);
        }

        public static DateTime GetLastBackupTime()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), BACKUP_JOB_FILE);

            XmlSerializer serializer = new XmlSerializer(typeof(DateTime));

            DateTime lastBackup = (DateTime)serializer.Deserialize(new FileStream(path, FileMode.Open));

            if (lastBackup == null)
            {
                lastBackup = DateTime.MinValue;
            }

            return lastBackup;
        }

        public static string GetServiceOutput()
        {
            StringBuilder sb = new StringBuilder(1024);
            using (StringWriter sr = new StringWriter(sb))
            {
                sr.WriteLine("===========================================");
                sr.WriteLine("Welcome to the AWS .NET SDK!");
                sr.WriteLine("===========================================");

                // Print the number of Amazon EC2 instances.
                IAmazonEC2 ec2 = AWSClientFactory.CreateAmazonEC2Client();
                DescribeInstancesRequest ec2Request = new DescribeInstancesRequest();

                try
                {
                    DescribeInstancesResponse ec2Response = ec2.DescribeInstances(ec2Request);
                    int numInstances = 0;
                    numInstances = ec2Response.Reservations.Count;
                    sr.WriteLine(string.Format("You have {0} Amazon EC2 instance(s) running in the {1} region.",
                                               numInstances, ConfigurationManager.AppSettings["AWSRegion"]));
                }
                catch (AmazonEC2Exception ex)
                {
                    if (ex.ErrorCode != null && ex.ErrorCode.Equals("AuthFailure"))
                    {
                        sr.WriteLine("The account you are using is not signed up for Amazon EC2.");
                        sr.WriteLine("You can sign up for Amazon EC2 at http://aws.amazon.com/ec2");
                    }
                    else
                    {
                        sr.WriteLine("Caught Exception: " + ex.Message);
                        sr.WriteLine("Response Status Code: " + ex.StatusCode);
                        sr.WriteLine("Error Code: " + ex.ErrorCode);
                        sr.WriteLine("Error Type: " + ex.ErrorType);
                        sr.WriteLine("Request ID: " + ex.RequestId);
                    }
                }
                sr.WriteLine();

                // Print the number of Amazon SimpleDB domains.
                IAmazonSimpleDB sdb = AWSClientFactory.CreateAmazonSimpleDBClient();
                ListDomainsRequest sdbRequest = new ListDomainsRequest();

                try
                {
                    ListDomainsResponse sdbResponse = sdb.ListDomains(sdbRequest);

                    int numDomains = 0;
                    numDomains = sdbResponse.DomainNames.Count;
                    sr.WriteLine(string.Format("You have {0} Amazon SimpleDB domain(s) in the {1} region.",
                                               numDomains, ConfigurationManager.AppSettings["AWSRegion"]));
                }
                catch (AmazonSimpleDBException ex)
                {
                    if (ex.ErrorCode != null && ex.ErrorCode.Equals("AuthFailure"))
                    {
                        sr.WriteLine("The account you are using is not signed up for Amazon SimpleDB.");
                        sr.WriteLine("You can sign up for Amazon SimpleDB at http://aws.amazon.com/simpledb");
                    }
                    else
                    {
                        sr.WriteLine("Caught Exception: " + ex.Message);
                        sr.WriteLine("Response Status Code: " + ex.StatusCode);
                        sr.WriteLine("Error Code: " + ex.ErrorCode);
                        sr.WriteLine("Error Type: " + ex.ErrorType);
                        sr.WriteLine("Request ID: " + ex.RequestId);
                    }
                }
                sr.WriteLine();

                // Print the number of Amazon S3 Buckets.
                IAmazonS3 s3Client = AWSClientFactory.CreateAmazonS3Client();

                try
                {
                    ListBucketsResponse response = s3Client.ListBuckets();
                    int numBuckets = 0;
                    if (response.Buckets != null &&
                        response.Buckets.Count > 0)
                    {
                        numBuckets = response.Buckets.Count;
                    }
                    sr.WriteLine("You have " + numBuckets + " Amazon S3 bucket(s).");
                }
                catch (AmazonS3Exception ex)
                {
                    if (ex.ErrorCode != null && (ex.ErrorCode.Equals("InvalidAccessKeyId") ||
                        ex.ErrorCode.Equals("InvalidSecurity")))
                    {
                        sr.WriteLine("Please check the provided AWS Credentials.");
                        sr.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                    }
                    else
                    {
                        sr.WriteLine("Caught Exception: " + ex.Message);
                        sr.WriteLine("Response Status Code: " + ex.StatusCode);
                        sr.WriteLine("Error Code: " + ex.ErrorCode);
                        sr.WriteLine("Request ID: " + ex.RequestId);
                    }
                }
                sr.WriteLine("Press any key to continue...");
            }
            return sb.ToString();
        }
    }
}