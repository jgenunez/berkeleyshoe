using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;
using System.ComponentModel;

namespace LocationApp
{
    public class ProductStock : INotifyPropertyChanged
    {
        private int _qty = 1;


        public ProductStock(Item item, string binScan)
        {
            this.Item = item;
            this.PreviousBin = item.BinLocation;
            this.BinScan = binScan;
        }

        public string ID
        {
            get { return this.Item.ItemLookupCode; }
        }

        public string PreviousBin { get; set; }

        public string BinScan { get; set; }

        public int QtyScan 
        {
            get { return _qty; }
            set 
            {
                _qty = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("Qty");
            }
        }

        public int OnHand
        {
            get { return (int)this.Item.Quantity; }
        }

        public void ApplyChanges()
        {
            List<string> locations = this.PreviousBin.Split(new Char[1] { ' ' }).ToList();
            locations.Add(this.BinScan);
            this.Item.BinLocation = String.Join(" ", locations.Distinct()).Trim();
        }

        public Item Item { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event 
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
