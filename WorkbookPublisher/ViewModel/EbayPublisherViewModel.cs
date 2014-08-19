using BerkeleyEntities;
using BerkeleyEntities.Ebay;
using Microsoft.TeamFoundation.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WorkbookPublisher
{
    enum StatusCode { PENDING, PENDING_CREATION, PENDING_UPDATE, COMPLETED }

    public class EbayPublisherViewModel
    {
        private const string STATUS_ACTIVE = "Active";
        private const string FORMAT_FIXEDPRICE = "FixedPriceItem";
        private const string FORMAT_AUCTION = "Chinese";

        private RelayCommand _publish;

        private PictureSetRepository _picSetRepository = new PictureSetRepository();
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private EbayMarketplace _marketplace;
        private Publisher _publisher;

        private List<>

        private Dictionary<EbayListing, List<EbayEntry>> _targetListings = new Dictionary<EbayListing, List<EbayEntry>>();

        public EbayPublisherViewModel(int marketplaceID, IEnumerable<EbayEntry> entries)
        {
            this.Entries = new ObservableCollection<EbayEntry>(entries);
            this.CanPublish = true;
            this.CanFixErrors = false;

            _marketplace = _dataContext.EbayMarketplaces.Single(p => p.ID == marketplaceID);
            _publisher = new Publisher(_dataContext, _marketplace);
            _publisher.Result += Publisher_Result;

            UpdateEntries();
        }

        public ObservableCollection<EbayEntry> Entries { get; set; }

        public string Header 
        {
            get { return _marketplace.Code; }
        }

        public bool CanPublish { get; set; }

        public bool CanFixErrors { get; set; }

        public ICommand Publish 
        {
            get
            {
                if (_publish == null)
                {
                    _publish = new RelayCommand(PublishAsync);
                }

                return _publish;
            }
        }

        private async void PublishAsync()
        {
            this.CanPublish = false;

            var pendingEntries = this.Entries.Where(p => p.Status.Equals("waiting"));

            HandleFixedPriceEntries(pendingEntries.Where(p => !p.IsAuction()));

            HandleAuctionEntries(pendingEntries.Where(p => p.IsAuction()));

            await Task.Run( () => _publisher.Publish());
        }

        private void HandleAuctionEntries(IEnumerable<EbayEntry> entries)
        {
            var auctions = _marketplace.Listings.Where(p => p.Format.Equals(FORMAT_AUCTION)).Where(p => p.Status.Equals(STATUS_ACTIVE)).ToList();

            foreach (EbayEntry entry in entries)
            {
                if (!auctions.Any(p => p.Sku.Equals(entry.Sku)))
                {
                    
                    CreateAuction(entry);

                }
                else
                {
                    entry.Completed = false;
                    entry.Message = "cannot modify auctions";
                }
            }

        }

        private void CreateAuction(EbayEntry entry)
        {
            EbayListing listing = new EbayListing();
            listing.Sku = entry.Sku;
            listing.Marketplace = _marketplace;
            listing.Condition = GetConditionID(entry.Condition);
            listing.Duration = GetDuration(entry.Format);
            listing.Format = FORMAT_AUCTION;
            listing.FullDescription = entry.FullDescription;
            listing.Title = entry.Title;
            listing.IsVariation = false;

            if (entry.StartDateSpecified)
            {
                listing.StartTime = entry.StartDate;
            }

            EbayListingItem listingItem = new EbayListingItem();
            listingItem.Item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));
            listingItem.Listing = listing;
            listingItem.Quantity = entry.Q;
            listingItem.Price = entry.P;

            _targetListings.Add(listing, new List<EbayEntry>() { entry });

            try
            {
                AssignPictures(listing);
            }
            catch (FileNotFoundException e)
            {
                _dataContext.EbayListings.Detach(listing);
                entry.Message = e.Message;
            }
        }

        private void HandleFixedPriceEntries(IEnumerable<EbayEntry> entries)
        {
            var fixedPrice = _marketplace.Listings.Where(p => p.Status.Equals(STATUS_ACTIVE) && p.Format.Equals(FORMAT_FIXEDPRICE)).ToList();

            foreach (var group in entries.GroupBy(p => p.ClassName))
            {
                List<EbayEntry> pending = new List<EbayEntry>();

                foreach (EbayEntry entry in group)
                {
                    if (fixedPrice.Any(p => p.Sku.Equals(entry.Sku)))
                    {
                        UpdateIndividualListing(fixedPrice.Single(p => p.Sku.Equals(entry.Sku)), entry);
                    }
                    else
                    {
                        pending.Add(entry);
                    }
                }

                if (pending.Count > 0 && fixedPrice.Any(p => p.Sku.Equals(group.Key)))
                {
                    UpdateVariationListing(fixedPrice.Single(p => p.Sku.Equals(group.Key)), pending);
                }
                else if (pending.Count() == 1)
                {
                    CreateIndividualListing(pending.First());
                }
                else if (pending.Count() > 1)
                {
                    CreateVariationListing(pending, group.Key);
                }
            }
        }

        private void UpdateIndividualListing(EbayListing listing, EbayEntry entry)
        {
            EbayListingItem listingItem = listing.ListingItems.First();

            listingItem.Quantity = entry.Q;
            listingItem.Price = entry.P;

            if (!string.IsNullOrWhiteSpace(entry.Title))
            {
                listing.Title = entry.Title;
            }
            if (!string.IsNullOrWhiteSpace(entry.FullDescription))
            {
                listing.FullDescription = entry.FullDescription;
            }

            _targetListings.Add(listing, new List<EbayEntry>() { entry });
        }

        private void CreateIndividualListing(EbayEntry entry)
        {
            EbayListing listing = new EbayListing();
            listing.Sku = entry.Sku;
            listing.Marketplace = _marketplace;
            listing.Condition = GetConditionID(entry.Condition);
            listing.Duration = GetDuration(entry.Format);
            listing.Format = FORMAT_FIXEDPRICE;
            listing.FullDescription = entry.FullDescription;
            listing.Title = entry.Title;
            listing.IsVariation = false;

            EbayListingItem listingItem = new EbayListingItem();
            listingItem.Item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));
            listingItem.Listing = listing;
            listingItem.Quantity = entry.Q;
            listingItem.Price = entry.P;

            try
            {
                AssignPictures(listing);
            }
            catch (FileNotFoundException e)
            {
                entry.Message = e.Message;
            }

            _targetListings.Add(listing, new List<EbayEntry>() { entry });
        }

        private void UpdateVariationListing(EbayListing listing, IEnumerable<EbayEntry> entries)
        {
            if (entries.Any(p => !string.IsNullOrWhiteSpace(p.Title)))
            {
                EbayEntry entry = entries.First(p => !string.IsNullOrWhiteSpace(p.Title));
            }
            if (entries.Any(p => !string.IsNullOrWhiteSpace(p.FullDescription)))
            {
                listing.FullDescription = entries.First(p => !string.IsNullOrWhiteSpace(p.FullDescription)).FullDescription;
            }

            listing.Title = listing.Title;

            foreach (EbayEntry entry in entries)
            {
                Item item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));

                EbayListingItem listingItem = listing.ListingItems.SingleOrDefault(p => p.Item.ID == item.ID);

                if (listingItem == null)
                {
                    listingItem = new EbayListingItem() { Item = item, Listing = listing };
                }

                listingItem.Quantity = entry.Q;
                listingItem.Price = entry.P;
            }

            _targetListings.Add(listing, entries.ToList());
        }

        private void CreateVariationListing(IEnumerable<EbayEntry> entries, string className)
        {
            EbayListing listing = new EbayListing();
            listing.Sku = className;
            listing.Marketplace = _marketplace;
            listing.Condition = GetConditionID(entries.First(p => !string.IsNullOrWhiteSpace(p.Condition)).Condition);
            listing.Duration = GetDuration(entries.First(p => !string.IsNullOrWhiteSpace(p.Format)).Format);
            listing.Format = FORMAT_FIXEDPRICE;
            listing.FullDescription = entries.First(p => !string.IsNullOrWhiteSpace(p.FullDescription)).FullDescription;
            listing.Title = entries.First(p => !string.IsNullOrWhiteSpace(p.Title)).Title;
            listing.IsVariation = true;

            foreach (EbayEntry entry in entries)
            {
                EbayListingItem listingItem = new EbayListingItem();
                listingItem.Item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(entry.Sku));
                listingItem.Listing = listing;
                listingItem.Quantity = entry.Q;
                listingItem.Price = entry.P;
            }

            try
            {
                AssignPictures(listing);
            }
            catch (FileNotFoundException e)
            {
                entries.ToList().ForEach(p => p.Message = e.Message);
            }

            _targetListings.Add(listing, entries.ToList());
        }

        private void AssignPictures(EbayListing listing)
        {
            string brand = listing.ListingItems.First().Item.SubDescription1;
            var skus = listing.ListingItems.Select(p => p.Item.ItemLookupCode);

            var pics = _picSetRepository.GetPictures(brand, skus.ToList());

            foreach (PictureInfo picInfo in pics)
            {
                var urls = _dataContext.EbayPictureServiceUrls.Where(p => p.LocalName.Equals(picInfo.Name)).ToList();

                EbayPictureServiceUrl url = urls.SingleOrDefault(p => !p.IsExpired() && picInfo.LastModified < p.TimeUploaded);

                if (url == null)
                {
                    url = new EbayPictureServiceUrl();
                    url.LocalName = picInfo.Name;
                    url.Path = picInfo.Path;
                    new EbayPictureUrlRelation() { PictureServiceUrl = url, Listing = listing, CreatedTime = DateTime.UtcNow };
                }
                else
                {
                    if (!listing.Relations.Any(p => p.PictureServiceUrl.ID == url.ID))
                    {
                        new EbayPictureUrlRelation() { PictureServiceUrl = url, Listing = listing, CreatedTime = DateTime.UtcNow };
                    }
                }
            }
        }

        private string GetDuration(string code)
        {
            switch (code)
            {
                case "GTC": return code;

                case "BIN": return "Days_30";

                case "A7": return "Days_7";

                case "A1": return "Days_1";

                case "A3": return "Days_3";

                case "A5": return "Days_5";

                default: return string.Empty;
            }
        }

        private string GetFormat(string code)
        {
            switch (code)
            {
                case "BIN" :
                case "GTC" :
                    return FORMAT_FIXEDPRICE;
                case "A1":
                case "A3":
                case "A5":
                case "A7" :
                    return FORMAT_AUCTION;
                default :
                    return string.Empty;
            }
        }

        private string GetConditionID(string condition)
        {
            switch(condition)
            {
                case "NEW" : return "1000";
                case "NWB": return "1500";
                case "PRE": return "1750";
                case "NWD": return "3000";

                default : return null;
            }
        }

        private void Publisher_Result(ResultArgs e)
        {
            foreach (EbayEntry entry in _targetListings[e.Listing])
            {
                entry.Message = e.Message;
                entry.Completed = !e.IsError;
            }
        }

        private void UpdateEntries()
        {
            foreach (EbayEntry entry in this.Entries)
            {
                Item item = _dataContext.Items.SingleOrDefault();
                entry.Brand = item.SubDescription1;
                entry.ClassName = item.ClassName;

                if (item == null)
                {
                    entry.Message = "sku not found";
                    continue;
                }

                string format = GetFormat(entry.Format);

                if (format.Equals(FORMAT_FIXEDPRICE))
                {
                    var listingItems = item.EbayListingItems.Where(p =>
                        p.Listing.MarketplaceID == _marketplace.ID &&
                        p.Listing.Status.Equals("ACTIVE") &&
                        p.Listing.Format.Equals(format)
                        );

                    if (listingItems.Count() == 1 && entry.Q == listingItems.First().Quantity)
                    {
                        entry.Completed = true;
                    }

                }
                else if (format.Equals(FORMAT_AUCTION))
                {
                    double days = double.Parse(GetDuration(entry.Format).Split(new Char[1]{'_'})[1]);
                    DateTime endDate = entry.StartDate.AddDays(days);

                    var listingItems = item.EbayListingItems.Where(p =>
                        p.Listing.MarketplaceID == _marketplace.ID &&
                        p.Listing.Status.Equals("ACTIVE") &&
                        (!(entry.StartDate > p.Listing.EndTime) && !(endDate < p.Listing.StartTime)) &&
                        p.Listing.Format.Equals(format)
                        );

                    if (listingItems.Count() > 0)
                    {
                        entry.Completed = true;
                    }
                }
                
                if (entry.Status.Equals("waiting"))
                {
                    if (format.Equals(FORMAT_AUCTION) && entry.Q > 1)
                    {
                        entry.Message = "auction max qty is 1";
                    }
                    if (entry.Q > item.Quantity)
                    {
                        entry.Message = "qty to publish exceeds on-hand";
                    }
                    if (item.Department == null)
                    {
                        entry.Message = "department classification required";
                    }
                    if (entry.Title.Count() > 80)
                    {
                        entry.Message = "title max characters is 80";
                    }
                }

            }

            if (this.Entries.All(p => p.Status.Equals("completed") || p.Status.Equals("error")))
            {
                this.CanPublish = false;
            }
        }
    }

    public class EbayEntry : INotifyPropertyChanged
    {
        private bool _completed;
        private string _message;

        public EbayEntry()
        {
            this.Message = string.Empty;
            this.Completed = false;
            this.Title = string.Empty;
            this.StartDate = DateTime.UtcNow;
        }

        public uint RowIndex { get; set; }
        public string Brand { get; set; }
        public string ClassName { get; set; }
        public string Sku { get; set; }
        public int Q { get; set; }
        public decimal P { get; set; }
        public DateTime StartDate { get; set; }
        public bool StartDateSpecified { get; set; }
        public string Format { get; set; }
        public string Title { get; set; }
        public string Condition { get; set; }
        public string FullDescription { get; set; }

        public bool Completed
        {
            get { return _completed; }
            set
            {
                _completed = value;

                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Status"));
                }
            }
        }
        public string Message
        {
            get { return _message; }
            set
            {
                var msgs = _message.Split(new Char[1] { '|' }).ToList();

                msgs.Add(value);

                msgs.ForEach(p => p = p.Trim());

                _message = string.Join(" | ", msgs);

                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Message"));
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Status"));
                }
            }
        }

        public string Status 
        {
            get 
            {
                if (this.Completed)
                {
                    return "completed";
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(this.Message))
                    {
                        return "waiting";
                    }
                    else
                    {
                        return "error";
                    }
                }
            }
        }

        public bool IsAuction()
        {
            if (this.Format.Contains("A"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public bool EndDate { 
            get 
            { 
                switch()
            } 
        }
    }
}
