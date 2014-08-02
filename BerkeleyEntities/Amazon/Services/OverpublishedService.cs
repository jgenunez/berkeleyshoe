using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;
using System.Timers;
using System.Threading;
using NLog;

namespace AmazonServices
{
    public class OverpublishedService
    {
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private AmznMarketplace _marketplace;
        private Publisher _publisher;
        

        public OverpublishedService(int marketplaceID)
        {
            _marketplace = _dataContext.AmznMarketplaces.Single(p => p.ID == marketplaceID);
            _publisher = new Publisher(_dataContext, _marketplace);
        }

        public void BalanceQuantities()
        {
            //if (!_marketplace.ListingSyncTime.HasValue || _marketplace.ListingSyncTime.Value < DateTime.UtcNow.AddHours(-1))
            //{
            //    throw new InvalidOperationException(_marketplace.Name + " listings must be synchronized in order to fix overpublished");
            //}

            if (!_marketplace.OrderSyncTime.HasValue || _marketplace.OrderSyncTime.Value < DateTime.UtcNow.AddHours(-1))
            {
                throw new InvalidOperationException(_marketplace.Name + " orders must be synchronized in order to fix overpublished");
            }

            var activeListings = _dataContext.AmznListingItems
                .Include("OrderItems")
                .Include("Item.EbayListingItems.OrderItems.Order")
                .Where(p => p.IsActive && p.Quantity > 0).ToList();


            foreach (AmznListingItem listingItem in activeListings)
            {
                if (listingItem.Quantity > listingItem.Item.QtyAvailable)
                {
                    listingItem.Quantity = listingItem.Item.QtyAvailable;
                }
            }

            _publisher.PublishInventoryData();

            while (_publisher.AnyPendingSubmission)
            {
                Thread.Sleep(30000);

                _publisher.PollSubmissionStatus();
            }

            _dataContext.Dispose();
        }

        //private void DetermineOverpublished()
        //{

        //    var activeListings = _dataContext.AmznListingItems
        //        .Include("OrderItems")
        //        .Include("Item.EbayListingItems.OrderItems.Order")
        //        .Where(p => p.IsActive && p.Quantity > 0).ToList();


        //    foreach (AmznListingItem listingItem in activeListings)
        //    {
        //        if (listingItem.Quantity > listingItem.Item.QtyAvailable)
        //        {
        //            Inventory inventoryData = new Inventory();
        //            inventoryData.SKU = listingItem.Item.ItemLookupCode;
        //            inventoryData.Item = listingItem.Item.QtyAvailable.ToString();

        //            AmazonEnvelopeMessage msg = new AmazonEnvelopeMessage();
        //            msg.MessageID = _currentMsgID.ToString();
        //            msg.Item = inventoryData;

        //            _messages.Add(msg);

        //            _currentMsgID++;
        //        }
        //    }
        //}
    }
}
