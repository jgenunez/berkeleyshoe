//
// AMGD
// Marketplaces:
// Amazon
//  A1 = ShopUsLast
//  A2 = 
//  A3 = 
//  A4 = 
// eBay
//  E1 = Mecalzo
//  E2 = OneMillionShoes
//  E3 = Pick-a-shoe
//  E4 = SmallAvenue
//  E5 = 
// Websites
//  W1 =
//  W2 =
//  W3
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BSI_InventoryPreProcessor
{
    public class ItemExcel
    {
        private string _gender;

        public static int ITEM_TYPE_SINGLE = 0;
        public static int ITEM_TYPE_PARENT = 10;

        public String Rows 
        {
            get 
            {
                if (Items.Count == 0)
                {
                    return this.Row;
                }
                else 
                {
                    return string.Join(" ", this.Items.Select(p => p.Row));
                }
            }
        }

        
        public int Type { get; set; }
        public int Received { get; set; }
        public int Variation { get; set; }
        public int Quantity { get; set; }
        public uint MarketPlaces { get; set; } // BIT Flag for marketplaces

        public bool Ok2Publish { get; set; }

        public Decimal MSRP { get; set; }
        public Decimal Cost { get; set; }
        public Decimal Price { get; set; }

        public String Result { get; set; }

        public String Row { get; set; }
        public String Brand { get; set; }
        public String SKU { get; set; }
        public String ItemLookupCode { get; set; }
        public String UPC { get; set; }
        public String Alias { get; set; }
        public String Size { get; set; }
        public String Width { get; set; }
        public String Condition { get; set; }
        public String Category { get; set; }
        public String Style { get; set; }
        public String RMS_Description { get; set; }
        public String FullDescription { get; set; }
        public String Keywords { get; set; }
        public String Material { get; set; }
        public String Color { get; set; }
        public String Shade { get; set; }
        public String HeelHeight { get; set; }       
        public String Gender
        {
            get { return _gender; }
            set
            {
                switch (value)
                {
                    case "MEN": value = "MENS"; break;

                    case "WOMEN": value = "WOMENS"; break;

                    case "JUNIORS": value = "JUNIOR"; break;
                }
                _gender = value;
            }
        }
        public String SellingFormat { get; set; }   
        public String purchaseOrder { get; set; }
        public String listUser { get; set; }
        public String Widths { get; set; }
        public String Title { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<ItemExcel> Items;
        public List<ItemMarketplace> Markets;

        public List<string> Pictures;
        public List<string> URLPictures;

        public int QtyA1 { get; set; }
        public int QtyA2 { get; set; }
        public int QtyA3 { get; set; }
        public int QtyA4 { get; set; }
        public int QtyE1 { get; set; }
        public int QtyE2 { get; set; }
        public int QtyE3 { get; set; }
        public int QtyE4 { get; set; }
        public int QtyE5 { get; set; }
        public int QtyW1 { get; set; }
        public int QtyW2 { get; set; }
        public int QtyW3 { get; set; }

        public Decimal PriceA1 { get; set; }
        public Decimal PriceA2 { get; set; }
        public Decimal PriceA3 { get; set; }
        public Decimal PriceA4 { get; set; }
        public Decimal PriceE1 { get; set; }
        public Decimal PriceE2 { get; set; }
        public Decimal PriceE3 { get; set; }
        public Decimal PriceE4 { get; set; }
        public Decimal PriceE5 { get; set; }

        public Decimal PriceW1 { get; set; }
        public Decimal PriceW2 { get; set; }
        public Decimal PriceW3 { get; set; }

        public ItemExcel()
        {
            this.StartDate = DateTime.MinValue;
            this.EndDate = DateTime.MinValue;

            this.SellingFormat = "GTC";

            Type = ITEM_TYPE_SINGLE;
            Ok2Publish = false;
            Brand = ItemLookupCode = Alias = RMS_Description = Gender = Title = FullDescription = Keywords = "";
            SKU = Size = Width = Condition = Category = Style = Material = Color = Shade = HeelHeight = SellingFormat = "";
            Widths = UPC = "";
            Variation = 0;
            Received = 0;

            Quantity = QtyA1 = QtyA2 = QtyA3 = QtyA4 = 0;
            Price = PriceA1 = PriceA2 = PriceA3 = PriceA4 = 0;

            QtyE1 = QtyE2 = QtyE3 = QtyE4 = QtyE5 = 0;
            PriceE1 = PriceE2 = PriceE3 = PriceE4 = PriceE5 = 0;
            
            QtyW1 = QtyW2 = QtyW3 = 0;
            PriceW1 = PriceW2 = PriceW3 = 0;

            purchaseOrder = "";
            listUser = "";

            Cost = MSRP = 0; 
            StartDate = new DateTime(1903, 1, 1); // Very old date
            EndDate = StartDate.AddDays(1);
            MarketPlaces = 0;
            Items = new List<ItemExcel>();
            Markets = new List<ItemMarketplace>();
            Pictures = new List<string>();
            URLPictures = new List<string>();
        } // ItemExcel

        public ItemExcel(ItemExcel pc)
        {
            this.Ok2Publish = pc.Ok2Publish;
            this.Brand = String.Copy(pc.Brand);
            this.SKU = String.Copy(pc.SKU);
            this.ItemLookupCode = String.Copy(pc.ItemLookupCode);
            this.Alias = String.Copy(pc.Alias);
            this.RMS_Description = String.Copy(pc.RMS_Description);
            this.Gender = String.Copy(pc.Gender);
            this.Condition = String.Copy(pc.Condition);
            this.Category = String.Copy(pc.Category);
            this.Style = String.Copy(pc.Style);
            this.Variation = pc.Variation;
            this.Widths = String.Copy(pc.Widths);
            this.UPC = String.Copy(pc.UPC);
            this.Size = String.Copy(pc.Size);
            this.Width = String.Copy(pc.Width);
            this.MarketPlaces = pc.MarketPlaces;

            this.Title = String.Copy(pc.Title);
            this.FullDescription = String.Copy(pc.FullDescription);
            this.Keywords = String.Copy(pc.Keywords);

            this.Material = String.Copy(pc.Material);
            this.Color = String.Copy(pc.Color);
            this.Shade = String.Copy(pc.Shade);
            this.HeelHeight = String.Copy(pc.HeelHeight);

            this.Received = pc.Received;
            this.Cost = pc.Cost;
            this.MSRP = pc.MSRP;

            this.purchaseOrder = String.Copy(pc.purchaseOrder);
            this.listUser = String.Copy(pc.listUser);

            this.Quantity = pc.Quantity;
            this.Price = pc.Price;

            this.QtyA1 = pc.QtyA1; this.PriceA1 = pc.PriceA1;
            this.QtyA2 = pc.QtyA2; this.PriceA2 = pc.PriceA2;
            this.QtyA3 = pc.QtyA3; this.PriceA3 = pc.PriceA3;
            this.QtyA4 = pc.QtyA4; this.PriceA4 = pc.PriceA4;

            this.QtyE1 = pc.QtyE1; this.PriceE1 = pc.PriceE1;
            this.QtyE2 = pc.QtyE2; this.PriceE2 = pc.PriceE2;
            this.QtyE3 = pc.QtyE3; this.PriceE3 = pc.PriceE3;
            this.QtyE4 = pc.QtyE4; this.PriceE4 = pc.PriceE4;
            this.QtyE5 = pc.QtyE5; this.PriceE5 = pc.PriceE5;

            this.QtyW1 = pc.QtyW1; this.PriceW1 = pc.PriceW1;
            this.QtyW2 = pc.QtyW2; this.PriceW2 = pc.PriceW2;
            this.QtyW3 = pc.QtyW3; this.PriceW3 = pc.PriceW3;

            this.SellingFormat = String.Copy(pc.SellingFormat);
            this.StartDate = pc.StartDate;
            this.EndDate = pc.EndDate;
            
            this.Items = new List<ItemExcel>();
            foreach (ItemExcel pi in pc.Items)
            {
                ItemExcel lu = new ItemExcel();
                lu.copyNewItem(pi);
                this.Items.Add(lu);
            } // foreach

            this.Markets = new List<ItemMarketplace>();
            foreach (ItemMarketplace pm in pc.Markets)
                this.Markets.Add(new ItemMarketplace(pm));

            this.Pictures = new List<string>();
            foreach (String ls in pc.Pictures)
                this.Pictures.Add(ls);

            this.URLPictures = new List<string>();
            foreach (String ls in pc.URLPictures)
                this.URLPictures.Add(ls);
            
        } // ItemExcel(ItemExcel pc)

        public void copyNewItem(ItemExcel pc)
        {
            this.Ok2Publish = pc.Ok2Publish;
            this.Brand = pc.Brand;
            this.SKU = pc.SKU;
            this.ItemLookupCode = pc.ItemLookupCode;
            this.Alias = pc.Alias;
            this.RMS_Description = pc.RMS_Description;
            this.Gender = pc.Gender;
            this.Condition = pc.Condition;
            this.Category = pc.Category;
            this.Style = pc.Style;
            this.Variation = pc.Variation;
            this.Widths = pc.Widths;
            this.UPC = pc.UPC;
            this.Size = pc.Size;
            this.Width = pc.Width;

            this.Title = pc.Title;
            this.FullDescription = pc.FullDescription;
            this.Keywords = pc.Keywords;

            this.Material = pc.Material;
            this.Color = pc.Color;
            this.Shade = pc.Shade;
            this.HeelHeight = pc.HeelHeight;

            this.Received = pc.Received;
            this.Cost = pc.Cost;
            this.MSRP = pc.MSRP;

            this.purchaseOrder = pc.purchaseOrder;
            this.listUser = pc.listUser;

            this.Quantity = pc.Quantity;
            this.Price = pc.Price;

            this.QtyA1 = pc.QtyA1; this.PriceA1 = pc.PriceA1;
            this.QtyA2 = pc.QtyA2; this.PriceA2 = pc.PriceA2;
            this.QtyA3 = pc.QtyA3; this.PriceA3 = pc.PriceA3;
            this.QtyA4 = pc.QtyA4; this.PriceA4 = pc.PriceA4;

            this.QtyE1 = pc.QtyE1; this.PriceE1 = pc.PriceE1;
            this.QtyE2 = pc.QtyE2; this.PriceE2 = pc.PriceE2;
            this.QtyE3 = pc.QtyE3; this.PriceE3 = pc.PriceE3;
            this.QtyE4 = pc.QtyE4; this.PriceE4 = pc.PriceE4;
            this.QtyE5 = pc.QtyE5; this.PriceE5 = pc.PriceE5;

            this.QtyW1 = pc.QtyW1; this.PriceW1 = pc.PriceW1;
            this.QtyW2 = pc.QtyW2; this.PriceW2 = pc.PriceW2;
            this.QtyW3 = pc.QtyW3; this.PriceW3 = pc.PriceW3;

            this.SellingFormat = pc.SellingFormat;
            this.StartDate = pc.StartDate;
            this.EndDate = pc.EndDate;

            this.MarketPlaces = pc.MarketPlaces;

        } // copyNewItem(ItemExcel pc)

        public Decimal getPriceForMarketplace(int pmktidx)
        {
            Decimal lr = 0;
            int ly = -1;
            Decimal[] lprcs = new Decimal[ItemMarketplace.MARKETPLACE_MAXMARKETS];

            lprcs[++ly] = this.PriceA1; // 0
            lprcs[++ly] = this.PriceA2; // 1
            lprcs[++ly] = this.PriceA3; // 2
            lprcs[++ly] = this.PriceA4; // 3

            lprcs[++ly] = this.PriceE1; // 4
            lprcs[++ly] = this.PriceE2; // 5
            lprcs[++ly] = this.PriceE3; // 6
            lprcs[++ly] = this.PriceE4; // 7
            lprcs[++ly] = this.PriceE5; // 8

            lprcs[++ly] = this.PriceW1; // 9
            lprcs[++ly] = this.PriceW2; // 10
            lprcs[++ly] = this.PriceW3; // 11

            if (pmktidx < lprcs.Length)
                lr = lprcs[pmktidx];

            return lr;
        } // getPriceForMarketplace


        public int getQuantityForMarketplace(int pmktidx)
        {
            int lr = 0, ly = -1;
            int[] lqtys = new int[ItemMarketplace.MARKETPLACE_MAXMARKETS];

            lqtys[++ly] = this.QtyA1; lqtys[++ly] = this.QtyA2;
            lqtys[++ly] = this.QtyA3; lqtys[++ly] = this.QtyA4;
            lqtys[++ly] = this.QtyE1; lqtys[++ly] = this.QtyE2;
            lqtys[++ly] = this.QtyE3; lqtys[++ly] = this.QtyE4;
            lqtys[++ly] = this.QtyE5; lqtys[++ly] = this.QtyW1;
            lqtys[++ly] = this.QtyW2; lqtys[++ly] = this.QtyW3;

            if (pmktidx < lqtys.Length)
                lr = lqtys[pmktidx];

            return lr;
        } // getPriceForMarketplace

        public void setQuantityForMarketplace(int lquantity, int pmktidx)
        {
            if (pmktidx == 0) this.QtyA1 = lquantity;
            if (pmktidx == 1) this.QtyA2 = lquantity;
            if (pmktidx == 2) this.QtyA3 = lquantity;
            if (pmktidx == 3) this.QtyA4 = lquantity;

            if (pmktidx == 4) this.QtyE1 = lquantity;
            if (pmktidx == 5) this.QtyE2 = lquantity;
            if (pmktidx == 6) this.QtyE3 = lquantity;
            if (pmktidx == 7) this.QtyE4 = lquantity;
            if (pmktidx == 8) this.QtyE5 = lquantity;

            if (pmktidx == 9) this.QtyW1 = lquantity;
            if (pmktidx == 10) this.QtyW2 = lquantity;
            if (pmktidx == 11) this.QtyW3 = lquantity;
        } // setPriceForMarketplace

        public void setPriceForMarketplace(Decimal lprice, int pmktidx)
        {
            if (pmktidx == 0) this.PriceA1 = lprice;
            if (pmktidx == 1) this.PriceA2 = lprice;
            if (pmktidx == 2) this.PriceA3 = lprice;
            if (pmktidx == 3) this.PriceA4 = lprice;

            if (pmktidx == 4) this.PriceE1 = lprice;
            if (pmktidx == 5) this.PriceE2 = lprice;
            if (pmktidx == 6) this.PriceE3 = lprice;
            if (pmktidx == 7) this.PriceE4 = lprice;
            if (pmktidx == 8) this.PriceE5 = lprice;

            if (pmktidx == 9) this.PriceW1 = lprice;
            if (pmktidx == 10) this.PriceW2 = lprice;
            if (pmktidx == 11) this.PriceW3 = lprice;
        } // setPriceForMarketplace

        public uint checkMarketPlaces()
        {
            uint lr = 0;

            if (this.Items.Count == 0)
            {
                int lx = 0;
                for (uint li = 1; lx < ItemMarketplace.MARKETPLACE_MAXMARKETS; lx++, li <<= 1)
                {
                    if (this.getQuantityForMarketplace(lx) > 0) lr |= li;
                } // for (int li = 1; lx < ItemMarketplace.MARKETPLACE_MAXMARKETS; lx++, li <<= 1)
            }
            else
            {
                foreach (ItemExcel lix in this.Items)
                {
                    int lx = 0;
                    for (uint li = 1; lx < ItemMarketplace.MARKETPLACE_MAXMARKETS; lx++, li <<= 1)
                    {
                        if (lix.getQuantityForMarketplace(lx) > 0) lr |= li;
                    } // for (int li = 1; lx < ItemMarketplace.MARKETPLACE_MAXMARKETS; lx++, li <<= 1)
                } // foreach
            };

            this.MarketPlaces = lr;
            return lr;
        } // checkMarketPlaces
    } // ExcelItem
} // ProcessInventory

