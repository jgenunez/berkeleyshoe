using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;

namespace InventoryApp
{
    public class ProductCountRepository
    {
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private PhysicalInventory _inventoryRef;

        private string _user;


        public ProductCountRepository(string inventoryRef, string bin, string user)
        {
            _inventoryRef = _dataContext.PhysicalInventories.Single(p => p.Code.Equals(inventoryRef));

            this.Bin = bin;

            _user = user;
        }

        public string Bin { get; set; }

        public ProductCount GetProductCount(string code)
        {
            Item item = _dataContext.Items.SingleOrDefault(p => p.ItemLookupCode.Equals(code) && p.Inactive == false);

            if (item == null)
            {
                Alias alias = _dataContext.Aliases.SingleOrDefault(p => p.Alias1.Equals(code));

                if (alias != null) { item = alias.Item; }
            }

            ProductCount product = new ProductCount(_inventoryRef, item, Bin, _user);

            product.Sku = item.ItemLookupCode;
            product.Brand = item.SubDescription1;
            product.Department = item.Department.Name;
            product.Expected = (int)item.Quantity;
            product.ExpectedBin = item.BinLocation;
            product.Aliases.AddRange(item.Aliases.Select(p => p.Alias1));

            return product;
        }

        public IEnumerable<ProductCount> GetExistingCounts()
        {
            var existingEntries = _dataContext.InventoryEntries.Where(p => p.PhysicalInventory.ID == _inventoryRef.ID && p.Bin.Equals(Bin));

            var skus = existingEntries.Select(p => p.Item.ItemLookupCode);

            List<ProductCount> counts = new List<ProductCount>();

            foreach (string sku in skus)
            {
                counts.Add(GetProductCount(sku));
            }

            return counts;
        }

        public void SaveChanges()
        {
            _dataContext.SaveChanges();
            _dataContext.Dispose();
        }

        public void DiscardChanges()
        {
            _dataContext.Dispose();
        }
    }
}
