using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;
using eBay.Service.Core.Soap;
using System.Data;
using System.Text.RegularExpressions;

namespace EbayServices.Mappers
{
    public class ListingMapper
    {
        private PictureSetRepository _pictureSetRepository = new PictureSetRepository();
        private ProductDataFactory _productDataFactory;
        private berkeleyEntities _dataContext;
        private EbayMarketplace _marketplace;
        

        public ListingMapper(berkeleyEntities dataContext, EbayMarketplace marketplace)
        {
            _dataContext = dataContext;
            _marketplace = marketplace;
            _productDataFactory = new ProductDataFactory(_dataContext);
        }


        public ItemType Map(EbayListing listing)
        {
            ItemType listingDto = null;

            if (listing.EntityState.Equals(EntityState.Modified) || listing.ListingItems.Any(p => p.EntityState.Equals(EntityState.Modified)))
            {
                listingDto = UpdateListingDto(listing);
            }
            else if (listing.EntityState.Equals(EntityState.Added))
            {
                listingDto = CreateListingDto(listing);
            }

            return listingDto;
        }

        private ItemType CreateListingDto(EbayListing listing)
        {
            ItemType listingDto = new ItemType();
            listingDto.SKU = listing.Sku;
            listingDto.ListingTypeSpecified = true;
            listingDto.ListingType = (ListingTypeCodeType)Enum.Parse(typeof(ListingTypeCodeType), listing.Format);
            listingDto.ListingDuration = listing.Duration;
            listingDto.ConditionIDSpecified = true;
            listingDto.ConditionID = int.Parse(listing.Condition);
            listingDto.Title = listing.Title;

            listingDto.Description = _marketplace.GetTemplate().Replace("<!-- INSERT FULL DESCRIPTION -->", listing.FullDescription);

            listingDto.PayPalEmailAddress = _marketplace.PayPalAccount;

            listingDto.DispatchTimeMaxSpecified = true;
            listingDto.DispatchTimeMax = 1;

            listingDto.CountrySpecified = true;
            listingDto.Country = CountryCodeType.US;

            listingDto.Location = "Near to you";

            listingDto.Currency = CurrencyCodeType.USD;
            listingDto.CurrencySpecified = true;

            listingDto.HitCounterSpecified = true;
            listingDto.HitCounter = HitCounterCodeType.BasicStyle;

            listingDto.PaymentMethods = new BuyerPaymentMethodCodeTypeCollection() { BuyerPaymentMethodCodeType.PayPal };

            listingDto.ShippingDetails = new ShippingDetailsType(){
                ShippingType = ShippingTypeCodeType.Flat,
                ShippingServiceOptions = new ShippingServiceOptionsTypeCollection() { 
                    new ShippingServiceOptionsType() { 
                        ShippingServicePrioritySpecified = true,
                        ShippingServicePriority = 1,
                        ShippingService = ShippingServiceCodeType.UPSGround.ToString(),
                        ShippingServiceCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 6.95 },
                        ShippingServiceAdditionalCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 2.50 },
                        ShippingSurcharge = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 8.50 }},
                    new ShippingServiceOptionsType(){
                        ShippingServicePrioritySpecified = true,
                        ShippingServicePriority = 2,
                        ShippingService = ShippingServiceCodeType.UPSNextDay.ToString(),
                        ShippingServiceCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 29.95 },
                        ShippingServiceAdditionalCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 10.00 }}},
                InternationalShippingServiceOption = new InternationalShippingServiceOptionsTypeCollection() {
                    new InternationalShippingServiceOptionsType(){
                        ShippingServicePrioritySpecified = true,
                        ShippingServicePriority = 1,
                        ShippingService =  ShippingServiceCodeType.USPSPriorityMailInternational.ToString(),
                        ShippingServiceCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 17.99 } ,
                        ShippingServiceAdditionalCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 5.00 } ,
                        ShipToLocation = new StringCollection { CountryCodeType.CA.ToString() }},
                    new InternationalShippingServiceOptionsType(){
                        ShippingServicePrioritySpecified = true,
                        ShippingServicePriority = 2,
                        ShippingService =  ShippingServiceCodeType.USPSPriorityMailInternational.ToString(),
                        ShippingServiceCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 40.00 } ,
                        ShippingServiceAdditionalCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 10.00 } ,
                        ShipToLocation = new StringCollection { ShippingRegionCodeType.Worldwide.ToString() }},
                    new InternationalShippingServiceOptionsType(){
                        ShippingServicePrioritySpecified = true,
                        ShippingServicePriority = 3,
                        ShippingService =  ShippingServiceCodeType.UPSWorldWideExpress.ToString(),
                        ShippingServiceCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 60.00 } ,
                        ShippingServiceAdditionalCost = new AmountType() { currencyID = CurrencyCodeType.USD, Value = 10.00 } ,
                        ShipToLocation = new StringCollection { ShippingRegionCodeType.Worldwide.ToString() }}}
            };
            
            listingDto.ReturnPolicy = new ReturnPolicyType() 
            { 
                ReturnsAcceptedOption = "ReturnsAccepted", 
                ReturnsWithinOption = "Days_30", 
                ShippingCostPaidByOption = "Buyer" 
            };

            if (listing.Format.Equals("FixedPriceItem"))
            {
                listingDto.BestOfferEnabled = true;
                listingDto.BestOfferEnabledSpecified = true;
                listingDto.BestOfferDetails = new BestOfferDetailsType() { BestOfferEnabledSpecified = true, BestOfferEnabled = true };
            }

            if (!(bool)listing.IsVariation)
            {
                ProductData productData = _productDataFactory.GetProductData(listing.Sku);
                listingDto.PrimaryCategory = new CategoryType() { CategoryID = productData.CategoryID };

                var itemSpecifics = productData.GetItemSpecifics().Concat(productData.GetVariationSpecifics()).ToArray();

                listingDto.ItemSpecifics = new NameValueListTypeCollection(itemSpecifics);

                var urls = listing.Relations.Select(p => p.PictureServiceUrl).Select(p => p.Url);
                listingDto.PictureDetails = new PictureDetailsType() { PictureURL = new StringCollection(urls.ToArray()) };

                EbayListingItem listingItem = listing.ListingItems.First();
                listingDto.QuantitySpecified = true;
                listingDto.Quantity = listingItem.Quantity;
                listingDto.StartPrice = new AmountType() {  currencyID = CurrencyCodeType.USD, Value = Convert.ToDouble(listingItem.Price)};
            }
            else
            {
                ProductMatrixData productMatrixData = _productDataFactory.GetProductMatrixData(listingDto.SKU, listing.ListingItems.Select(p => p.Item.ItemLookupCode));
                listingDto.PrimaryCategory = new CategoryType() {  CategoryID = productMatrixData.CategoryID };
                listingDto.ItemSpecifics = new NameValueListTypeCollection(productMatrixData.GetItemSpecifics().ToArray());
                listingDto.Variations = new VariationsType();
                listingDto.Variations.Variation = new VariationTypeCollection();

                foreach (EbayListingItem listingItem in listing.ListingItems)
                {
                    VariationType variationDto = new VariationType();
                    variationDto.SKU = listingItem.Item.ItemLookupCode;
                    variationDto.QuantitySpecified = true;
                    variationDto.Quantity = listingItem.Quantity;
                    variationDto.StartPrice = new AmountType() {  currencyID = CurrencyCodeType.USD, Value = Convert.ToDouble(listingItem.Price) };

                    ProductData productData = _productDataFactory.GetProductData(listingItem.Item.ItemLookupCode);
                    variationDto.VariationSpecifics = new NameValueListTypeCollection(productData.GetVariationSpecifics().ToArray());
                    listingDto.Variations.Variation.Add(variationDto);
                }

                var sets = productMatrixData.GetVariationSpecificSets();
                listingDto.Variations.VariationSpecificsSet = new NameValueListTypeCollection(sets.ToArray());

                var allUrls = listing.Relations.Select(p => p.PictureServiceUrl);
                listingDto.PictureDetails = new PictureDetailsType();
                listingDto.PictureDetails.PictureURL = new StringCollection(allUrls.Where(p => p.VariationAttributeValue.Equals("N/A")).Select(p => p.Url).ToArray());
                listingDto.Variations.Pictures = new PicturesTypeCollection();

                foreach (var set in sets)
                {
                    var urls = allUrls.Where(p => set.Value.Contains(p.VariationAttributeValue));

                    if (urls.Count() > 0)
                    {
                        PicturesType pictures = new PicturesType();
                        pictures.VariationSpecificName = set.Name;
                        pictures.VariationSpecificPictureSet = new VariationSpecificPictureSetTypeCollection();
                        var urlGroups = urls.GroupBy(p => p.VariationAttributeValue);
                        foreach (var urlGroup in urlGroups)
                        {
                            VariationSpecificPictureSetType picSet = new VariationSpecificPictureSetType();
                            picSet.PictureURL = new StringCollection(urlGroup.Select(p => p.Url).ToArray());
                            picSet.VariationSpecificValue = urlGroup.Key;
                        }

                        listingDto.Variations.Pictures.Add(pictures);
                    }
                }
            }
            return listingDto;
        }

        private ItemType UpdateListingDto(EbayListing listing)
        {
            ItemType listingDto = new ItemType();
            listingDto.ItemID = listing.Code;
            listingDto.SKU = listing.Sku;
            listingDto.ListingTypeSpecified = true;
            listingDto.ListingType = (ListingTypeCodeType)Enum.Parse(typeof(ListingTypeCodeType), listing.Format);
            listingDto.ListingDuration = listing.Duration;
            listingDto.ConditionIDSpecified = true;
            listingDto.ConditionID = int.Parse(listing.Condition);
            listingDto.Title = listing.Title;

            if ((bool)listing.IsVariation)
            {
                listingDto.Variations = new VariationsType();
                listingDto.Variations.Variation = new VariationTypeCollection();
                foreach (EbayListingItem listingItem in listing.ListingItems)
                {
                    VariationType variationDto = new VariationType();
                    variationDto.SKU = listingItem.Item.ItemLookupCode;
                    variationDto.QuantitySpecified = true;
                    variationDto.Quantity = listingItem.Quantity;
                    variationDto.StartPrice = new AmountType() { currencyID = CurrencyCodeType.USD, Value = Convert.ToDouble(listingItem.Price) };
                    listingDto.Variations.Variation.Add(variationDto);
                }
            }
            else
            {
                EbayListingItem listingItem = listing.ListingItems.First();
                listingDto.QuantitySpecified = true;
                listingDto.Quantity = listingItem.Quantity;
                listingDto.StartPrice = new AmountType() { currencyID = CurrencyCodeType.USD, Value = Convert.ToDouble(listingItem.Price) };
            }

            return listingDto;
        }

        public EbayListing Map(ItemType listingDto)
        {
            EbayListing listing = _dataContext.EbayListings.SingleOrDefault(p => 
                p.MarketplaceID.Equals(_marketplace.ID) && 
                p.Code.Equals(listingDto.ItemID));

            if (listing == null)
            {
                listing = new EbayListing();
                listing.Code = listingDto.ItemID;
                listing.Format = listingDto.ListingTypeSpecified ? listingDto.ListingType.ToString() : listing.Format;
                listing.MarketplaceID = _marketplace.ID;
            }

            listing.Duration = listingDto.ListingDuration != null ? listingDto.ListingDuration : listing.Duration;

            listing.EndTime = listingDto.ListingDetails != null && listingDto.ListingDetails.EndTimeSpecified ? 
                listingDto.ListingDetails.EndTime : listing.EndTime;

            listing.StartTime = listingDto.ListingDetails != null && listingDto.ListingDetails.StartTimeSpecified? 
                listingDto.ListingDetails.StartTime : listing.StartTime;

            listing.Title = listingDto.Title != null ? listingDto.Title : listing.Title;
            listing.Sku = listingDto.SKU != null ? listingDto.SKU : listing.Sku;
            listing.Condition = listingDto.ConditionIDSpecified ? listingDto.ConditionID.ToString() : listing.Condition;

            listing.Status = listingDto.SellingStatus != null && listingDto.SellingStatus.ListingStatusSpecified ?
                listingDto.SellingStatus.ListingStatus.ToString() : listing.Status;

            if (listingDto.Variations == null)
            {
                listing.IsVariation = false;
                MapListingItem(listing, listingDto);
            }
            else
            {
                listing.IsVariation = true;
                MapListingItem(listing, listingDto.Variations);
            }

            return listing;
        }

        private void MapListingItem(EbayListing listing, ItemType listingDto)
        {
            string sku = listingDto.SKU != null ? listingDto.SKU.ToUpper().Trim() : listing.Sku;

            EbayListingItem listingItem = FindListingItem(listing, sku);

            listingItem.Price = listingDto.StartPrice != null ? decimal.Parse(listingDto.StartPrice.Value.ToString()) : listingItem.Price;

            listingItem.Quantity = 
                listingDto.QuantitySpecified && 
                listingDto.SellingStatus != null && 
                listingDto.SellingStatus.QuantitySoldSpecified ?
                    listingDto.Quantity - listingDto.SellingStatus.QuantitySold : listingItem.Quantity;
        }

        private void MapListingItem(EbayListing listing, VariationsType variationDtos)
        {
            var currentSkus = variationDtos.Variation.ToArray().Select(p => p.SKU);
            var droppedListingItems = listing.ListingItems.Where(p => !currentSkus.Any(s => s.Equals(p.Item.ItemLookupCode)));
            foreach (EbayListingItem listingItem in droppedListingItems)
            {
                listingItem.Quantity = 0;
            }

            foreach (VariationType variationDto in variationDtos.Variation)
            {
                string sku = variationDto.SKU.ToUpper().Trim();
                EbayListingItem listingItem = FindListingItem(listing, sku);
                listingItem.Price = variationDto.StartPrice != null ? Convert.ToDecimal(variationDto.StartPrice.Value) : listingItem.Price;
                listingItem.Quantity =
                    variationDto.QuantitySpecified &&
                    variationDto.SellingStatus != null &&
                    variationDto.SellingStatus.QuantitySoldSpecified ?
                        variationDto.Quantity - variationDto.SellingStatus.QuantitySold : listingItem.Quantity;
            }
        }

        private EbayListingItem FindListingItem(EbayListing listing, string sku)
        {
            EbayListingItem listingItem = listing.ListingItems.SingleOrDefault(p => p.Item.ItemLookupCode.Equals(sku));

            if (listingItem == null)
            {
                Item item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(sku));

                listingItem = new EbayListingItem() { Item = item, Listing = listing };
            }

            return listingItem;
        }

        
    }
}
