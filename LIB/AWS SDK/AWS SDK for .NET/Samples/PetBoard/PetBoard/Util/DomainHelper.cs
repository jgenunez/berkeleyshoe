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
namespace Petboard.Util
{
    using System;
    using System.Collections.Generic;
    using System.Web;

    using Amazon.SimpleDB;
    using Amazon.SimpleDB.Model;

    using Properties;

    public static class DomainHelper
    {
        #region Public Methods

        public static bool DoesDomainExist(string domainName, IAmazonSimpleDB sdbClient)
        {
            ListDomainsRequest listDomainsRequest = new ListDomainsRequest();
            ListDomainsResponse response = sdbClient.ListDomains(listDomainsRequest);
            if (response != null)
            {
                return response.DomainNames.Contains(domainName);
            }

            return false;
        }

        public static void CheckForDomain(string domainName, IAmazonSimpleDB sdbClient)
        {
            VerifyKeys();

            ListDomainsRequest listDomainsRequest = new ListDomainsRequest();
            ListDomainsResponse listDomainsResponse = sdbClient.ListDomains(listDomainsRequest);
            if (!listDomainsResponse.DomainNames.Contains(domainName))
            {
                CreateDomainRequest createDomainRequest = new CreateDomainRequest() { DomainName = domainName };
                sdbClient.CreateDomain(createDomainRequest);
            }
        }

        public static bool CheckForDomains(string[] expectedDomains, AmazonSimpleDBClient sdbClient)
        {
            VerifyKeys();

            ListDomainsRequest listDomainsRequest = new ListDomainsRequest();
            ListDomainsResponse listDomainsResponse = sdbClient.ListDomains(listDomainsRequest);

            foreach (string expectedDomain in expectedDomains)
            {
                if (!listDomainsResponse.DomainNames.Contains(expectedDomain))
                {
                    // No point checking any more domains because
                    // at least 1 domain doesn't exist
                    return false;
                }
            }

            // We got this far, indicating that all expectedDomains 
            // were found in the domain list
            return true;
        }

        public static void VerifyKeys()
        {
            if (String.IsNullOrEmpty(Settings.Default.AWSAccessKey.Trim()) || 
                String.IsNullOrEmpty(Settings.Default.AWSSecretAccessKey.Trim()))
            {
                if (HttpContext.Current.Request.Path.ToLowerInvariant() != "/setup.aspx")
                {
                    HttpContext.Current.Response.Redirect("~/Setup.aspx");
                }
            }
        }

        #endregion
    }
}