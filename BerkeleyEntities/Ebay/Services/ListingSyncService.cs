using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBay.Service.Core.Soap;
using BerkeleyEntities;
using eBay.Service.Call;
using eBay.Service.Core.Sdk;
using NLog;
using EbayServices.Mappers;

namespace EbayServices.Services
{
    public class ListingSyncService
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private berkeleyEntities _dataContext = new berkeleyEntities();
        //private List<string> _pendingListings = new List<string>();
        private EbayMarketplace _marketplace;       
        private ListingMapper _listingMapper;

        public ListingSyncService(int marketplaceID)
        {
            _marketplace = _dataContext.EbayMarketplaces.Single(p => p.ID == marketplaceID);
            _listingMapper = new ListingMapper(_dataContext, _marketplace);
        }

        public void MarginalSync()
        {
            DateTime syncTime = DateTime.UtcNow.AddMinutes(-3);

            //var listingIds = new StringCollection(_marketplace.GetWaitingSyncListings().ToArray());

            //SyncListings(listingIds, syncTime);

            if (_marketplace.ListingSyncTime.HasValue && _marketplace.ListingSyncTime.Value > syncTime.AddDays(-3))
            {
                DateTime from = _marketplace.ListingSyncTime.Value.AddMinutes(-5);

                SyncListingsByCreatedTime(from, syncTime);

                SyncListingsByModifiedTime(from, syncTime);
            }
            else
            {
                DateTime from = syncTime.AddDays(-40);
                DateTime to = syncTime.AddDays(32);

                SyncListingsByEndTime(from, to);
            }

            //_marketplace.SetWaitingSyncListings(_pendingListings);

            _marketplace.ListingSyncTime = syncTime;

            _dataContext.SaveChanges();

            _dataContext.Dispose();
        }

        public void SyncListings(StringCollection listings, DateTime syncTime)
        {
            ItemTypeCollection listingDtos = new ItemTypeCollection();

            foreach (string listingID in listings)
            {
                GetItemRequestType request = new GetItemRequestType();
                request.ItemID = listingID;

                SetOutputSelection(request);

                GetItemCall call = new GetItemCall(_marketplace.GetApiContext());

                try
                {
                    GetItemResponseType response = call.ExecuteRequest(request) as GetItemResponseType;
                    listingDtos.Add(response.Item);
                }
                catch (ApiException e)
                {
                    _logger.Error(e.Message);

                    var errors = e.Errors.ToArray();

                    if (errors.Any(p => p.ErrorCode.Equals("17")))
                    {
                        EbayListing listing = _dataContext.EbayListings.Single(p => p.Code.Equals(listingID));
                        listing.Status = "Deleted";
                    }
                }
            }

            ProcessListingData(listingDtos, syncTime);
        }

        private void SyncListingsByCreatedTime(DateTime from, DateTime to)
        {
            GetSellerListRequestType request = new GetSellerListRequestType();

            request.StartTimeFromSpecified = true;
            request.StartTimeToSpecified = true;
            request.StartTimeFrom = from;
            request.StartTimeTo = to;

            
            ProcessListingData(ExecuteGetSellerList(request), to);
            
        }

        private void SyncListingsByEndTime(DateTime from, DateTime to)
        {
            GetSellerListRequestType request = new GetSellerListRequestType();

            request.EndTimeFromSpecified = true;
            request.EndTimeToSpecified = true;
            request.EndTimeFrom = from;
            request.EndTimeTo = to;

            
            ProcessListingData(ExecuteGetSellerList(request), to);
            
        }

        private void SyncListingsByModifiedTime(DateTime from, DateTime to)
        {
            GetSellerEventsRequestType request = new GetSellerEventsRequestType();
            request.DetailLevel = new DetailLevelCodeTypeCollection();
            request.DetailLevel.Add(DetailLevelCodeType.ReturnAll);

            //request.NewItemFilterSpecified = true;
            //request.NewItemFilter = false;

            request.HideVariationsSpecified = true;
            request.HideVariations = false;

            request.IncludeVariationSpecificsSpecified = true;
            request.IncludeVariationSpecifics = false;

            request.ModTimeFromSpecified = true;
            request.ModTimeToSpecified = true;
            request.ModTimeFrom = from;
            request.ModTimeTo = to;


            GetSellerEventsCall call = new GetSellerEventsCall(_marketplace.GetApiContext());
            call.ApiCallBase.Timeout = 120000;

            
            GetSellerEventsResponseType response = call.ExecuteRequest(request) as GetSellerEventsResponseType;
            ProcessListingData(response.ItemArray, to);

        }

        private ItemTypeCollection ExecuteGetSellerList(GetSellerListRequestType request)
        {
            SetOutputSelection(request);

            request.Pagination = new PaginationType();
            request.Pagination.EntriesPerPage = 85;
            request.Pagination.PageNumber = 1;
            request.IncludeVariations = true;
            request.IncludeVariationsSpecified = true;

            GetSellerListCall call = new GetSellerListCall(_marketplace.GetApiContext());

            GetSellerListResponseType response = call.ExecuteRequest(request) as GetSellerListResponseType;

            while (request.Pagination.PageNumber < response.PaginationResult.TotalNumberOfPages)
            {
                request.Pagination.PageNumber++;
                GetSellerListResponseType additionalResponse = call.ExecuteRequest(request) as GetSellerListResponseType;
                response.ItemArray.AddRange(additionalResponse.ItemArray);
            }

            return response.ItemArray;
        }

        private void SetOutputSelection(AbstractRequestType request)
        {
            request.OutputSelector = new StringCollection();
            request.DetailLevel = new DetailLevelCodeTypeCollection();
            request.DetailLevel.Add(DetailLevelCodeType.ReturnAll);

            request.OutputSelector.AddRange(new string[18] 
            { 
                "ItemArray.Item.ItemID", "ItemArray.Item.SellingStatus.QuantitySold", 
                "ItemArray.Item.ListingDetails.EndTime", "ItemArray.Item.ListingDetails.StartTime", 
                "ItemArray.Item.Title", "ItemArray.Item.Quantity" , "ItemArray.Item.ListingType",
                "ItemArray.Item.SKU", "ItemArray.Item.ConditionDisplayName", "ItemArray.Item.ListingDuration",
                "ItemArray.Item.BestOfferEnabled", "ItemArray.Item.Variations", "ItemArray.Item.Variations.Variation",
                "ItemArray.Item.StartPrice", "Pagination.PageNumber", "PaginationResult.TotalNumberOfPages", 
                "ItemArray.Item.ConditionID", "ItemArray.Item.SellingStatus.ListingStatus"
            });
        }

        private void ProcessListingData(ItemTypeCollection listingsDtos, DateTime syncTime)
        {
            foreach (ItemType listingDto in listingsDtos)
            {
                try
                {
                    EbayListing listing = _listingMapper.Map(listingDto);
                    listing.LastSyncTime = syncTime;
                }
                catch (Exception e)
                {
                    _logger.Error(string.Format("Listing ( {1} | {0} ) synchronization failed: {2}", listingDto.ItemID, _marketplace.Code , e.Message));

                    //if (!_pendingListings.Contains(listingDto.ItemID))
                    //{
                    //    _pendingListings.Add(listingDto.ItemID);
                    //}
                }
            }

            _dataContext.SaveChanges();
        }

    }
}
