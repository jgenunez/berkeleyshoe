using BerkeleyEntities;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace WorkbookPublisher.ViewModel
{
    public abstract class PublisherViewModel : INotifyPropertyChanged
    {
        protected PublishCommand _publishCommand;
        protected ReadCommand _readEntriesCommand;
        protected UpdateCommand _updateCommand;

        protected ObservableCollection<ListingEntry> _entries = new ObservableCollection<ListingEntry>();
        protected ExcelWorkbook _workbook;

        protected string _marketplaceCode;

        public PublisherViewModel(ExcelWorkbook workbook, string marketplaceCode)
        {
            _marketplaceCode = marketplaceCode;

            _workbook = workbook;

            this.Entries = CollectionViewSource.GetDefaultView(_entries);
            
            _entries.CollectionChanged += (e, x) => 
            {
                if (x.NewItems != null)
                {
                    foreach (INotifyPropertyChanged item in x.NewItems)
                    {
                        item.PropertyChanged += (p, b) =>
                        {
                            if (this.PropertyChanged != null)
                            {
                                this.PropertyChanged(this, new PropertyChangedEventArgs("Completed"));
                                this.PropertyChanged(this, new PropertyChangedEventArgs("Pending"));
                                this.PropertyChanged(this, new PropertyChangedEventArgs("Error"));
                                this.PropertyChanged(this, new PropertyChangedEventArgs("Processing"));
                            }
                        };
                    } 
                }

                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Completed"));
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Pending"));
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Error"));
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Processing"));
                }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICollectionView Entries { get; set; }

        public string Header { get { return _marketplaceCode; } }

        public int Processing
        {
            get 
            {
                if (this.Entries != null)
                {
                    var entries = this.Entries.SourceCollection.OfType<ListingEntry>();

                    return entries.Where(p => p.Status.Equals(StatusCode.Processing)).Count();
                }
                else
                {
                    return 0;
                }
            }
        }

        public int Completed
        {
            get
            {
                if (this.Entries != null)
                {
                    var entries = this.Entries.SourceCollection.OfType<ListingEntry>();

                    return entries.Where(p => p.Status.Equals(StatusCode.Completed)).Count();
                }
                else
                {
                    return 0;
                }
            }
        }

        public int Error
        {
            get
            {
                if (this.Entries != null)
                {
                    var entries = this.Entries.SourceCollection.OfType<ListingEntry>();

                    return entries.Where(p => p.Status.Equals(StatusCode.Error)).Count();
                }
                else
                {
                    return 0;
                }
            }
        }

        public int Pending
        {
            get
            {
                if (this.Entries != null)
                {
                    var entries = this.Entries.SourceCollection.OfType<ListingEntry>();

                    return entries.Where(p => p.Status.Equals(StatusCode.Pending)).Count();
                }
                else
                {
                    return 0;
                }
            }
        }

        public ICommand PublishCommand 
        {
            get
            {

                return _publishCommand;
            }
        }

        public ICommand ReadCommand 
        {
            get 
            {
                return _readEntriesCommand;
            }
        }

        public ICommand UpdateCommand
        {
            get
            {
                return _updateCommand;
            }
        }
    }

    public class UpdateCommand : ICommand
    {
        private string _marketplaceCode;
        private bool _canExecute = false;
        private ExcelWorkbook _workbook;

        public UpdateCommand(ExcelWorkbook workbook, string marketplaceCode)
        {
            _workbook = workbook;
            _marketplaceCode = marketplaceCode;
        }

        public event EventHandler FixCompleted;
        public event EventHandler CanExecuteChanged;

        public void ReadCompletedHandler(object e, EventArgs args)
        {
            SetCanExecute(true);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public void Execute(object parameter)
        {
            SetCanExecute(false);

            try
            {
                var entries = ((ICollectionView)parameter).SourceCollection.OfType<ListingEntry>();

                _workbook.UpdateSheet(entries.Cast<BaseEntry>().ToList(), typeof(ListingEntry), _marketplaceCode);

                var sourceEntries = ((ICollectionView)parameter).SourceCollection as ObservableCollection<ListingEntry>;

                sourceEntries.Clear();

                if (FixCompleted != null)
                {
                    FixCompleted(this, new EventArgs());
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message);
                SetCanExecute(true);
            }
        }

        private void SetCanExecute(bool canExecute)
        {
            _canExecute = canExecute;

            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, new EventArgs());
            }
        }
    }

    public abstract class PublishCommand : ICommand
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private bool _canExecute = false;
        private ICollectionView _view;

        public event EventHandler CanExecuteChanged;
        public event EventHandler PublishCompleted;

        public void ReadCompletedHandler(object e, EventArgs args)
        {
            SetCanExecute(true);
        }

        public void FixCompletedHandler(object e, EventArgs args)
        {
            SetCanExecute(false);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public void Execute(object parameter)
        {
            SetCanExecute(false);

            _view = ((ICollectionView)parameter);
            
            var entries = _view.SourceCollection.OfType<ListingEntry>();

            var pendingEntries = entries.Where(p => p.Status.Equals(StatusCode.Pending));

            Publish(pendingEntries);
        }

        public abstract void Publish(IEnumerable<ListingEntry> entries);

        protected void RaisePublishCompleted()
        {
            if (PublishCompleted != null)
            {
                PublishCompleted(this, new EventArgs());
            }

            _view.Refresh();
        }

        private void SetCanExecute(bool canExecute)
        {
            _canExecute = canExecute;

            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, new EventArgs());
            }
        }
        
    }

    public class ReadCommand : ICommand
    {
        protected string _marketplaceCode;
        private bool _canExecute = true;
        private ExcelWorkbook _workbook;
        private Type _entryType;

        public ReadCommand(ExcelWorkbook workbook, string marketplaceCode, Type entryType)
        {
            _workbook = workbook;
            _marketplaceCode = marketplaceCode;
            _entryType = entryType;
        }

        public event EventHandler CanExecuteChanged;
        public event EventHandler ReadCompleted;

        public void FixCompletedHandler(object e, EventArgs args)
        {
            SetCanExecute(true);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public async void Execute(object parameter)
        {
            SetCanExecute(false);

            var entriesView = ((ICollectionView)parameter).SourceCollection as ObservableCollection<ListingEntry>;

            try
            {
                var result = (await Task.Run<List<object>>(() => _workbook.ReadSheet(_entryType, _marketplaceCode))).Cast<ListingEntry>().Where(p => !string.IsNullOrWhiteSpace(p.Sku)).ToList();

                if (result.Count > 0)
                {

                    foreach (var entry in result)
                    {
                        entry.ClearMessages();
                        entriesView.Add(entry);
                    }

                    var addedEntries = await Task.Run<List<ListingEntry>>(() => _workbook.UpdateEntries(result, _entryType, _marketplaceCode));

                    addedEntries.ForEach(p => entriesView.Add(p));

                    if (ReadCompleted != null)
                    {
                        ReadCompleted(this, new EventArgs());
                    }

                    ((ICollectionView)parameter).Refresh();
                }
                else
                {
                    MessageBox.Show("No entry found");
                    entriesView.Clear();
                    SetCanExecute(true);
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message);
                SetCanExecute(true);
            }
            catch (FormatException x)
            {
                MessageBox.Show("Invalid columns");
                SetCanExecute(true);
            }

           
        }

        private void SetCanExecute(bool canExecute)
        {
            _canExecute = canExecute;

            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, new EventArgs());
            }
        }
    }
}
