using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;
using BerkeleyEntities.Amazon;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Input;
using Microsoft.TeamFoundation.MVVM;
using WorkbookPublisher.View;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;
using WorkbookPublisher.ViewModel;
using AmazonServices.Mappers;
using System.Windows;
using System.Data;

namespace WorkbookPublisher.ViewModel
{
    
    public class AmznPublisherViewModel : PublisherViewModel
    {
        public AmznPublisherViewModel(ExcelWorkbook workbook, string marketplaceCode) : base(workbook, marketplaceCode)
        {
            _readEntriesCommand = new ReadCommand(_workbook, marketplaceCode, typeof(AmznEntry));
            _publishCommand = new AmznPublishCommand(marketplaceCode);
            _updateCommand = new UpdateCommand(_workbook, marketplaceCode);

            _readEntriesCommand.ReadCompleted += _publishCommand.ReadCompletedHandler;
            _readEntriesCommand.ReadCompleted += _updateCommand.ReadCompletedHandler;
            _updateCommand.FixCompleted += _readEntriesCommand.FixCompletedHandler;
            _updateCommand.FixCompleted += _publishCommand.FixCompletedHandler;
        }
    }


    public class AmznPublishCommand : PublishCommand
    {
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private AmznMarketplace _marketplace;

        public List<AmznEntry> _processingEntries = new List<AmznEntry>();
        private Queue<IEnumerable<PublishingResult>> _pendingResubmission = new Queue<IEnumerable<PublishingResult>>();

        public AmznPublishCommand(string marketplaceCode)
        {
            _marketplace = _dataContext.AmznMarketplaces.Single(p => p.Code.Equals(marketplaceCode));
        }

        private void Publisher_Result(List<PublishingResult> results)
        {
            foreach (var result in results)
            {
                AmznEntry entry = _processingEntries.Single(p => p.Sku.Equals(result.Data.Sku));

                entry.ClearMessages();

                if (result.HasError)
                {
                    entry.Status = StatusCode.Error;
                    entry.Message = result.Message;
                }
                else
                {
                    entry.Status = StatusCode.Completed;
                    entry.Command = string.Empty;
                }
            }

            var productErrors = results.Where(p => p.HasError && p.Data.ProductData != null);

            if (productErrors.Count() > 0)
            {
                _pendingResubmission.Enqueue(productErrors); 
            }
        }

        public async override void Publish(IEnumerable<ListingEntry> entries)
        {
            _processingEntries.AddRange(entries.Cast<AmznEntry>());

            List<ListingItemDto> listingItems = new List<ListingItemDto>();

            foreach (AmznEntry entry in entries)
            {
                ListingItemDto listingItem = new ListingItemDto();

                listingItem.Sku = entry.Sku;

                listingItem.Title = entry.Title;

                var existingListingItem = _dataContext.AmznListingItems.SingleOrDefault(p => p.IsActive && p.Item != null && p.Item.ItemLookupCode.Equals(entry.Sku) && p.MarketplaceID == _marketplace.ID);


                listingItem.IncludeProductData = existingListingItem == null || entry.GetUpdateFlags().Any(p => p.Equals("PRODUCTDATA"));

                listingItem.Qty = entry.Q;
                listingItem.QtySpecified = existingListingItem == null || existingListingItem.Quantity != entry.Q;

                listingItem.Price = entry.P;
                listingItem.PriceSpecified = existingListingItem == null || decimal.Compare(existingListingItem.Price, entry.P) != 0 || entry.GetUpdateFlags().Any(p => p.Equals("SALE"));

                if (entry.SalePriceSpecified)
                {
                    listingItem.SaleData = new SaleData() { SalePrice = entry.SalePrice, StartDate = entry.SaleStart, EndDate = entry.SaleEnd };
                }

                listingItems.Add(listingItem);

                entry.Status = StatusCode.Processing;
            }

            BerkeleyEntities.Amazon.AmazonServices services = new BerkeleyEntities.Amazon.AmazonServices();

            services.Result += Publisher_Result;

            await Task.Run(() => services.Publish(_marketplace.ID, listingItems, "Publisher"));

            while (_pendingResubmission.Count > 0)
            {
                var results = _pendingResubmission.Dequeue();

                RepublishDataWindow republishForm = new RepublishDataWindow();
                republishForm.DataContext = CollectionViewSource.GetDefaultView(results);
                republishForm.ShowDialog();

                if ((bool)republishForm.DialogResult)
                {
                    foreach (var result in results)
                    {
                        AmznEntry entry = _processingEntries.Single(p => p.Sku.Equals(result.Data.Sku));
                        entry.Status = StatusCode.Processing;
                    }

                    await Task.Run(() => services.Publish(_marketplace.ID, results.Select(p => p.Data), "Publisher"));
                }
            }

            this.RaisePublishCompleted();
        }

    }

}
