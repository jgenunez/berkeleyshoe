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
        private const string LASTBACKUP_FILE = "LastBackup.xml";
        private const string BUCKET_NAME = "berkeleybackup";
        private const string LOCAL_PICTURE_ROOT = @"P:\products";
        private const string BUCKET_PICTURE_ROOT = "picture-backups";

        private static IAmazonS3 _s3Client;

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            _s3Client = AWSClientFactory.CreateAmazonS3Client();

            DateTime lastBackup = GetLastBackupTime().AddMinutes(-5);

            DirectoryInfo rootDir = new DirectoryInfo(LOCAL_PICTURE_ROOT);

            var brandDirs = rootDir.GetDirectories().Where(p => p.LastWriteTime > lastBackup);

            foreach (var brandDir in brandDirs)
            {
                var files = brandDir.GetFiles().Where(p => p.LastWriteTime > lastBackup);

                foreach (var file in files)
                {
                    PushToAmazons3(file);
                }
            }
        }

        public static void PushToAmazons3(FileInfo file)
        {
            PutObjectRequest request = new PutObjectRequest();
            request.BucketName = BUCKET_NAME;
            request.ContentType = "image/jpeg";
            request.FilePath = file.FullName;
            request.Key = file.FullName.Replace(LOCAL_PICTURE_ROOT, BUCKET_PICTURE_ROOT).Replace("\\","/");

            try
            {
                PutObjectResponse response = _s3Client.PutObject(request);

                _logger.Info(string.Format("{0} uploaded successfully",file.Name));
            }
            catch (AmazonS3Exception ex)
            {
                _logger.Error(string.Format("Request ID: {0}  Error Code: {1}  Message: {2}", ex.RequestId, ex.ErrorCode, ex.Message));
            }
        }

        public static void SaveLastBackupTime(DateTime lastBackup)
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), LASTBACKUP_FILE);

            XmlSerializer serializer = new XmlSerializer(typeof(DateTime));

            serializer.Serialize(new FileStream(path, FileMode.Create), lastBackup);
        }

        public static DateTime GetLastBackupTime()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), LASTBACKUP_FILE);

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