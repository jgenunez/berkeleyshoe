using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using BerkeleyEntities;
using System.Data;

namespace InventoryApp
{
    public class ProductCount
    {
        private Item _item = null;
        private PhysicalInventory _inventoryRef = null;
        private string _currentBin = null;
        private string _currentUser = null;

        public ProductCount(PhysicalInventory inventoryRef, Item item, string bin, string user)
        {
            _currentBin = bin;
            _currentUser = user;
            _item = item;
            _inventoryRef = inventoryRef;

            this.Aliases = new List<string>();
        }

        public string Brand { get; set; }

        public string Sku { get; set; }

        public List<string> Aliases { get; set; }

        public int Counted
        {
            set 
            {
                InventoryEntry entry = GetInventoryEntry();

                if (entry == null)
                {
                    entry = new InventoryEntry();
                    entry.Item = _item;
                    entry.Bin = _currentBin;
                    entry.PhysicalInventory = _inventoryRef;
                    
                }

                entry.User = _currentUser;
                entry.LastModified = DateTime.Now;
                entry.Counted = value; 
            }
            get 
            {
                InventoryEntry entry = GetInventoryEntry();

                if (entry != null)
                {
                    return entry.Counted;
                }
                else
                {
                    return 0;
                }
                           
            }
        }

        public int Expected { get; set; }

        public string Bin
        {
            get
            {
                var binGroups = _item.InventoryEntries.Where(p => p.PhysicalInventoryID == _inventoryRef.ID).GroupBy(p => p.Bin);

                StringBuilder bin = new StringBuilder();

                foreach (var binGroup in binGroups)
                {
                    if (binGroup.Sum(s => s.Counted) > 0)
                    {
                        bin.AppendLine(binGroup.Key + "(" + binGroup.Sum(s => s.Counted) + ") ");
                    }
                }

                return bin.ToString();
            }
        }

        public string ExpectedBin { get; set; }

        public string Department { get; set; }

        public bool HasChanges()
        {
            return _item.InventoryEntries
                .Where(p => p.PhysicalInventoryID == _inventoryRef.ID)
                .Any(p => p.EntityState.Equals(EntityState.Added) || p.EntityState.Equals(EntityState.Modified));
        }

        private InventoryEntry GetInventoryEntry()
        {
            return _item.InventoryEntries.SingleOrDefault(p => p.Bin.Equals(_currentBin) && p.PhysicalInventoryID == _inventoryRef.ID);
        }



        //public event PropertyChangedEventHandler PropertyChanged;

        //// Create the OnPropertyChanged method to raise the event 
        //public void OnPropertyChanged(string name)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;

        //    if (handler != null)
        //    {
        //        handler(this, new PropertyChangedEventArgs(name));
        //    }
        //}
    }
}
