/*******************************************************************************
 *  Copyright 2008-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *  Licensed under the Apache License, Version 2.0 (the "License"). You may not use
 *  this file except in compliance with the License. A copy of the License is located at
 *
 *  http://aws.amazon.com/apache2.0
 *
 *  or in the "license" file accompanying this file.
 *  This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 *  CONDITIONS OF ANY KIND, either express or implied. See the License for the
 *  specific language governing permissions and limitations under the License.
 * *****************************************************************************/
namespace Petboard.Model
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;

    using Amazon.S3;
    using Amazon.S3.Model;
    using Amazon.SimpleDB;
    using Amazon.SimpleDB.Model;

    using Properties;
    using Util;

    public class Pet
    {
        #region Properties

        public string Birthdate { get; set; }

        public string Breed { get; set; }

        public string Dislikes { get; set; }

        public string Id { get; set; }

        public string Likes { get; set; }

        public string Name { get; set; }

        public string PhotoThumbUrl { get; set; }

        public string Sex { get; set; }

        public string Type { get; set; }

        #endregion

        #region Public Methods

        public static void PutPhoto(string domainName, string itemName, string bucketName, string fileName, Stream fileContent, bool isPublic, AmazonSimpleDBClient sdbClient, AmazonS3Client s3Client)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                return;
            }

            BucketHelper.CheckForBucket(itemName, s3Client);

            PutObjectRequest putObjectRequest = new PutObjectRequest();
            putObjectRequest.BucketName = bucketName;
            putObjectRequest.CannedACL = S3CannedACL.PublicRead;
            putObjectRequest.Key = fileName;
            putObjectRequest.InputStream = fileContent;
            s3Client.PutObject(putObjectRequest);

            DomainHelper.CheckForDomain(domainName, sdbClient);
            PutAttributesRequest putAttrRequest = new PutAttributesRequest()
            {
                DomainName = domainName,
                ItemName = itemName
            };

            putAttrRequest.Attributes.Add(new ReplaceableAttribute
                {
                    Name = "PhotoThumbUrl",
                    Value = String.Format(Settings.Default.S3BucketUrlFormat, String.Format(Settings.Default.BucketNameFormat, HttpContext.Current.User.Identity.Name, itemName), fileName),
                    Replace = true
                });
            sdbClient.PutAttributes(putAttrRequest);

            if (isPublic)
            {
                DomainHelper.CheckForDomain(Settings.Default.PetBoardPublicDomainName, sdbClient);
                putAttrRequest.DomainName = Settings.Default.PetBoardPublicDomainName;
                sdbClient.PutAttributes(putAttrRequest);
            }
        }

        public void Put(string domainName, string itemName, bool isPublic, AmazonSimpleDBClient sdbClient)
        {
            if (String.IsNullOrEmpty(domainName) ||
                String.IsNullOrEmpty(itemName))
            {
                return;
            }

            DomainHelper.CheckForDomain(domainName, sdbClient);
            PutAttributesRequest putAttrRequest = new PutAttributesRequest()
            {
                DomainName = domainName,
                ItemName = itemName
            };

            putAttrRequest.Attributes = new List<ReplaceableAttribute>()
            {
                new ReplaceableAttribute { Name = "Public", Value = isPublic.ToString(), Replace = true },
                new ReplaceableAttribute { Name = "PhotoThumbUrl", Value = !String.IsNullOrEmpty(this.PhotoThumbUrl) ? this.PhotoThumbUrl : String.Empty, Replace = true },
                new ReplaceableAttribute { Name = "Name", Value = this.Name, Replace = true },
                new ReplaceableAttribute { Name = "Type", Value = this.Type, Replace = true },
                new ReplaceableAttribute { Name = "Breed", Value = this.Breed, Replace = true },
                new ReplaceableAttribute { Name = "Sex", Value = this.Sex, Replace = true },
                new ReplaceableAttribute { Name = "Birthdate", Value = this.Birthdate, Replace = true },
                new ReplaceableAttribute { Name = "Likes", Value = this.Likes, Replace = true },
                new ReplaceableAttribute { Name = "Dislikes", Value = this.Dislikes, Replace = true }
            };
            sdbClient.PutAttributes(putAttrRequest);

            if (isPublic)
            {
                DomainHelper.CheckForDomain(Settings.Default.PetBoardPublicDomainName, sdbClient);
                putAttrRequest.DomainName = Settings.Default.PetBoardPublicDomainName;
                sdbClient.PutAttributes(putAttrRequest);
            }
            else
            {
                DeleteAttributesRequest deleteAttributeRequest = new DeleteAttributesRequest()
                {
                    DomainName = Settings.Default.PetBoardPublicDomainName,
                    ItemName = itemName
                };

                sdbClient.DeleteAttributes(deleteAttributeRequest);
            }
        }

        #endregion
    }
}