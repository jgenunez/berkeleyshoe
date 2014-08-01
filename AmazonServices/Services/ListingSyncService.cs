using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;
using MarketplaceWebService.Model;
using System.Threading;
using System.IO;
using NLog;

namespace AmazonServices
{
    public class ListingSyncService
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        const string FEED_SUBMITTED = "_SUBMITTED_";
        const string FEED_CANCELLED = "_CANCELLED_";
        const string FEED_INPROGRESS = "_IN_PROGRESS_";
        const string FEED_DONE = "_DONE_";
        const string FEED_DONENODATA = "_DONE_NO_DATA_";

        const string REPORT_LISTINGS = "_GET_MERCHANT_LISTINGS_DATA_";

        private berkeleyEntities _dataContext = new berkeleyEntities();
        //private List<string> _pendingListings = new List<string>();
        private AmznMarketplace _marketplace;
        private DateTime _currentSyncTime;

        public ListingSyncService(int marketplaceID)
        {
            _marketplace = _dataContext.AmznMarketplaces.Single(p => p.ID.Equals(marketplaceID));
        }

        public void Synchronize()
        {
            ReportRequestInfo requestInfo = CheckForExistingRequest(REPORT_LISTINGS);

            if (requestInfo == null)
            {
                requestInfo = RequestReport(REPORT_LISTINGS);
            }

            while (requestInfo.ReportProcessingStatus.Equals(FEED_INPROGRESS) || requestInfo.ReportProcessingStatus.Equals(FEED_SUBMITTED))
            {
                //Wait 5 minutes
                Thread.Sleep(200000);

                requestInfo = Poll(requestInfo.ReportRequestId);
            }

            if (requestInfo.ReportProcessingStatus.Equals(FEED_DONE))
            {
                var lines = GetReport(requestInfo.GeneratedReportId);

                _currentSyncTime = requestInfo.SubmittedDate.ToUniversalTime();

                PersistListings(new Queue<string>(lines));
            }

            _dataContext.Dispose();
        }

        private ReportRequestInfo RequestReport(string reportType)
        {
            RequestReportRequest request = new RequestReportRequest();
            request.Merchant = _marketplace.MerchantId;
            request.ReportType = reportType;
            //request.StartDate = DateTime.Now;
            //request.EndDate = DateTime.Now;

            RequestReportResponse response = _marketplace.GetMWSClient().RequestReport(request);

            ReportRequestInfo requestInfo = response.RequestReportResult.ReportRequestInfo;

            return requestInfo;
        }

        private ReportRequestInfo Poll(string requestID)
        {
            GetReportRequestListRequest request = new GetReportRequestListRequest();
            request.Merchant = _marketplace.MerchantId;
            request.ReportRequestIdList = new IdList();
            request.ReportRequestIdList.Id = new List<string>() { requestID };

            GetReportRequestListResponse response = _marketplace.GetMWSClient().GetReportRequestList(request);

            return response.GetReportRequestListResult.ReportRequestInfo.First();
        }

        private ReportRequestInfo CheckForExistingRequest(string reportType)
        {
            GetReportRequestListRequest request = new GetReportRequestListRequest();
            request.Merchant = _marketplace.MerchantId;
            request.ReportTypeList = new TypeList();
            request.ReportTypeList.Type = new List<string>() { reportType };

            request.RequestedFromDate = DateTime.Now.AddHours(-1);
            request.RequestedToDate = DateTime.Now;

            GetReportRequestListResponse response = _marketplace.GetMWSClient().GetReportRequestList(request);

            List<ReportRequestInfo> requests = response.GetReportRequestListResult.ReportRequestInfo;

            ReportRequestInfo requestInfo = null;

            if (response.GetReportRequestListResult.ReportRequestInfo.Count > 0)
            {
                DateTime latestSubmission = requests.Max(s => s.SubmittedDate);

                requestInfo = requests.SingleOrDefault(p => p.SubmittedDate.Equals(latestSubmission));
            }

            return requestInfo;
        }

        private IEnumerable<string> GetReport(string reportID)
        {
            MemoryStream ms = new MemoryStream();

            GetReportRequest request = new GetReportRequest();
            request.Merchant = _marketplace.MerchantId;
            request.ReportId = reportID;
            request.Report = ms;

            GetReportResponse response = _marketplace.GetMWSClient().GetReport(request);

            StreamReader reader = new StreamReader(ms);

            return reader.ReadToEnd().Split(new Char[1] { '\n' }).Where(p => !string.IsNullOrWhiteSpace(p));
        }

        private void PersistListings(Queue<string> lines)
        {
            while (lines.Count > 0)
            {
                int i = 0;

                while (lines.Count > 0 && i < 500)
                {
                    string line = lines.Dequeue();

                    try
                    {
                        ProccessLine(line);
                    }
                    catch (Exception e)
                    {
                        string sku = line.Split(new Char[1] { '\t' })[3];

                        _logger.Error(string.Format("Listing ( {1} | {0} ) synchronization failed: {2}", sku, _marketplace.Code, e.Message));

                        //if (!_pendingListings.Contains(sku))
                        //{
                        //    _pendingListings.Add(sku);
                        //}
                    }
  
                    i++;
                }

                _dataContext.SaveChanges();
            }

            var deletedListings = _dataContext.AmznListingItems
                    .Where(p => p.MarketplaceID.Equals(_marketplace.ID) && !p.LastSyncTime.Equals(_currentSyncTime));

            foreach (AmznListingItem listingItem in deletedListings)
            {
                listingItem.IsActive = false;
            }

            _marketplace.ListingSyncTime = _currentSyncTime;

            _dataContext.SaveChanges();
        }

        private void ProccessLine(string line)
        {
            string[] values = line.Split(new Char[1] { '\t' });

            string sku, title, price, quantity, openDate, asin, condition;

            title = values[0];
            sku = values[3];
            price = values[4];
            quantity = values[5];
            openDate = values[6].Replace("PDT", "").Replace("PST", "");
            condition = values[12];
            asin = values[16];

            if (title.Length > 200)
            {
                title = title.Substring(0, 200);
            }

            Item item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(sku));

            AmznListingItem listingItem = item.AmznListingItems.SingleOrDefault(p => p.MarketplaceID.Equals(_marketplace.ID));

            if (listingItem == null)
            {
                listingItem = new AmznListingItem();
                listingItem.MarketplaceID = _marketplace.ID;
                listingItem.Item = item;
            }


            if (listingItem.LastSyncTime < _currentSyncTime)
            {
                listingItem.Quantity = int.Parse(quantity);
            }

            listingItem.Price = decimal.Parse(price);
            listingItem.ASIN = asin;
            listingItem.Title = title;
            listingItem.OpenDate = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(openDate), TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
            listingItem.Condition = condition;
            listingItem.LastSyncTime = _currentSyncTime;
            listingItem.IsActive = true;
        }
    }
}
