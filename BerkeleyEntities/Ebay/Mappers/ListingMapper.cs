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
        private ProductMapperFactory _productMapperFactory;
        private berkeleyEntities _dataContext;
        private EbayMarketplace _marketplace;
        

        public ListingMapper(berkeleyEntities dataContext, EbayMarketplace marketplace)
        {
            _dataContext = dataContext;
            _marketplace = marketplace;
            _productMapperFactory = new ProductMapperFactory();
        }


        public ItemType Map(EbayListing listing)
        {
            ItemType listingDto = null;

            if (listing.EntityState.Equals(EntityState.Modified))
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

            if (!listing.StartTime.Equals(DateTime.MinValue))
            {
                listingDto.ScheduleTimeSpecified = true;
                listingDto.ScheduleTime = listing.StartTime;
            }

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

            if (!(bool)listing.IsVariation)
            {
                ProductMapper mapper = _productMapperFactory.GetProductData(listing.ListingItems.First().Item);

                listingDto.PrimaryCategory = new CategoryType() { CategoryID = mapper.CategoryID };

                var itemSpecifics = mapper.GetItemSpecifics().Concat(mapper.GetVariationSpecifics()).ToArray();
                listingDto.ItemSpecifics = new NameValueListTypeCollection(itemSpecifics);

                var urls = listing.Relations.Select(p => p.PictureServiceUrl).Select(p => p.Url);
                listingDto.PictureDetails = new PictureDetailsType() { PictureURL = new StringCollection(urls.ToArray()) };

                EbayListingItem listingItem = listing.ListingItems.First();
                listingDto.QuantitySpecified = true;
                listingDto.Quantity = listingItem.Quantity;
                listingDto.StartPrice = new AmountType() {  currencyID = CurrencyCodeType.USD, Value = Convert.ToDouble(listingItem.Price)};

                if (listing.Format.Equals("FixedPriceItem"))
                {
                    listingDto.BestOfferEnabled = true;
                    listingDto.BestOfferEnabledSpecified = true;
                    listingDto.BestOfferDetails = new BestOfferDetailsType() { BestOfferEnabledSpecified = true, BestOfferEnabled = true };
                }
            }
            else
            {
                ProductMatrixMapper matrixMapper = _productMapperFactory.GetProductMatrixData(listingDto.SKU, listing.ListingItems.Select(p => p.Item));
                listingDto.PrimaryCategory = new CategoryType() {  CategoryID = matrixMapper.CategoryID };
                listingDto.ItemSpecifics = new NameValueListTypeCollection(matrixMapper.GetItemSpecifics().ToArray());
                listingDto.Variations = new VariationsType();
                listingDto.Variations.Variation = new VariationTypeCollection();

                foreach (EbayListingItem listingItem in listing.ListingItems)
                {
                    VariationType variationDto = new VariationType();
                    variationDto.SKU = listingItem.Item.ItemLookupCode;
                    variationDto.QuantitySpecified = true;
                    variationDto.Quantity = listingItem.Quantity;
                    variationDto.StartPrice = new AmountType() {  currencyID = CurrencyCodeType.USD, Value = Convert.ToDouble(listingItem.Price) };

                    ProductMapper productData = _productMapperFactory.GetProductData(listingItem.Item);
                    variationDto.VariationSpecifics = new NameValueListTypeCollection(productData.GetVariationSpecifics().ToArray());
                    listingDto.Variations.Variation.Add(variationDto);
                }

                var sets = matrixMapper.GetVariationSpecificSets();
                listingDto.Variations.VariationSpecificsSet = new NameValueListTypeCollection(sets.ToArray());

                var allUrls = listing.Relations.Select(p => p.PictureServiceUrl);

                listingDto.PictureDetails = new PictureDetailsType();
                listingDto.PictureDetails.PictureURL = new StringCollection(allUrls.Where(p => !p.LocalName.Contains("_")).Select(p => p.Url).ToArray());

                if (allUrls.Any(p => p.LocalName.Contains("_")))
                {
                    listingDto.Variations.Pictures = matrixMapper
                        .GetVariationPictures(allUrls.Where(p => p.LocalName.Contains("_")));
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

        public void Map(ItemType listingDto, EbayListing listing)
        {
            listing.Code = listingDto.ItemID;

            listing.Format = listingDto.ListingTypeSpecified ? listingDto.ListingType.ToString() : listing.Format;

            listing.Marketplace = _marketplace;

            listing.Duration = listingDto.ListingDuration != null ? listingDto.ListingDuration : listing.Duration;

            listing.EndTime = listingDto.ListingDetails != null && listingDto.ListingDetails.EndTimeSpecified ?
                listingDto.ListingDetails.EndTime : listing.EndTime;

            listing.StartTime = listingDto.ListingDetails != null && listingDto.ListingDetails.StartTimeSpecified ?
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

            foreach (var listingItem in listing.ListingItems)
            {
                if (!currentSkus.Any(p => p.Equals(listingItem.Sku)))
                {
                    listingItem.Quantity = 0;
                }
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
            EbayListingItem listingItem = listing.ListingItems.SingleOrDefault(p => p.Sku.Equals(sku));

            if (listingItem == null)
            {
                Item item = _dataContext.Items.SingleOrDefault(p => p.ItemLookupCode.Equals(sku));

                listingItem = new EbayListingItem() { Item = item, Listing = listing, Sku = sku };
            }

            return listingItem;
        }

        
    }
}
