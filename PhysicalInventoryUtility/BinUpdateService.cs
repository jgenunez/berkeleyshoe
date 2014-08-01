using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;

namespace InventoryApp
{
    public class BinUpdateService
    {
        private berkeleyEntities _dataContext = new berkeleyEntities();
        private PhysicalInventory _inventoryRef = null;


        public BinUpdateService(string inventoryRef)
        {
            _inventoryRef = _dataContext.PhysicalInventories.Single(p => p.Code.Equals(inventoryRef));
        }

        public void Update()
        {
            List<InventoryEntry> entryGroups = _dataContext.InventoryEntries.Where(p => p.PhysicalInventory.ID == _inventoryRef.ID).ToList();

            var groups = entryGroups.GroupBy(p => p.Item.ID);
            //var entryGroups = _dataContext.InventoryEntries.Where(p => p.PhysicalInventory.ID == _inventoryRef.ID && p.Bin.Equals("WALL")).GroupBy(p => p.Item.ID);

            foreach (var entryGroup in groups)
            {
                Item item = _dataContext.Items.Single(p => p.ID == entryGroup.Key);
                var expectedBins = item.BinLocation.Split(new Char[1] { ' ' }).Distinct().Where(p => !string.IsNullOrWhiteSpace(p));
                var actualBins = entryGroup.Select(p => p.Bin);

                List<string> currentBin = new List<string>(expectedBins);
                currentBin.ForEach(p => p = p.Trim().ToUpper());
                foreach (string bin in expectedBins.Except(actualBins))
                {
                    Bsi_LocationLog log = new Bsi_LocationLog();
                    log.Item = item;
                    log.Location = "- " + bin;
                    log.Quantity = 0;
                    log.UpdateDate = DateTime.Now;
                    log.User = _inventoryRef.Code;
                    log.BeforeChange = string.Join(" ", currentBin);
                    currentBin.Remove(bin);
                }

                foreach (InventoryEntry entry in entryGroup)
                {
                    Bsi_LocationLog log = new Bsi_LocationLog();
                    log.Item = item;
                    log.Location = "+ " + entry.Bin;
                    log.Quantity = entry.Counted;
                    log.UpdateDate = DateTime.Now;
                    log.User = entry.User + " (" + _inventoryRef.Code + ")";
                    log.BeforeChange = string.Join(" ", currentBin);

                    if (!currentBin.Any(p => p.Equals(entry.Bin)))
                    {
                        currentBin.Add(entry.Bin);
                    }
                }

                int charCount = currentBin.Sum(s => s.Count()) + (currentBin.Count() - 1);

                List<string> notesField = new List<string>();

                while (charCount > 20)
                {
                    string bin = currentBin.First();
                    currentBin.Remove(bin);
                    notesField.Add(bin);
                    charCount = currentBin.Sum(s => s.Count()) + (currentBin.Count() - 1);
                }

                item.BinLocation = string.Join(" ", currentBin);

                if (notesField.Count > 0)
                {
                    item.Notes = item.Notes.Trim() + string.Join(" ", notesField);
                }

            }

            _dataContext.SaveChanges();
        }



    }
}
