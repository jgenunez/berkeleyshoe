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
        protected FixCommand _fixErrorsCommand;

        protected ObservableCollection<Entry> _entries = new ObservableCollection<Entry>();
        protected ExcelWorkbook _workbook;

        protected string _marketplaceCode;

        public PublisherViewModel(ExcelWorkbook workbook, string marketplaceCode)
        {
            _marketplaceCode = marketplaceCode;
            _workbook = workbook;

            this.Entries = CollectionViewSource.GetDefaultView(_entries);

            this.Entries.Filter = p => ((Entry)p).Status.Equals("error");

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
                                this.PropertyChanged(this, new PropertyChangedEventArgs("Progress"));
                            }
                        };
                    } 
                }

                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Progress"));
                }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICollectionView Entries { get; set; }

        public string Header { get { return _marketplaceCode; } }

        public string Progress
        {
            get
            {
                if (this.Entries != null)
                {
                    var entries = this.Entries.SourceCollection.OfType<Entry>();

                    string completed = entries.Where(p => p.Status.Equals("completed")).Count().ToString();
                    string waiting = entries.Where(p => p.Status.Equals("waiting")).Count().ToString();
                    string errors = entries.Where(p => p.Status.Equals("error")).Count().ToString();

                    return string.Format("{0} Pending\n{1} Completed\n{2} Errors", waiting, completed, errors);
                }
                else
                {
                    return "0 Pending\n0 Completed\n0 Errors";
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

        public ICommand FixCommand
        {
            get
            {
                return _fixErrorsCommand;
            }
        }
    }

    public class FixCommand : ICommand
    {
        private string _marketplacCode;
        private bool _canExecute = false;
        private ExcelWorkbook _workbook;

        public FixCommand(ExcelWorkbook workbook, string marketplaceCode)
        {
            _workbook = workbook;
            _marketplacCode = marketplaceCode;
        }

        public event EventHandler FixCompleted;
        public event EventHandler CanExecuteChanged;

        public void PublishCompletedHandler(object e, EventArgs args)
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
                var entries = ((ICollectionView)parameter).SourceCollection.OfType<Entry>();

                var errorEntries = entries.Where(p => p.Status.Equals("error")).ToList();

                if (errorEntries.Count > 0)
                {
                    _workbook.CreateErrorSheet(_marketplacCode + "(errors)", errorEntries);
                }

                var sourceEntries = ((ICollectionView)parameter).SourceCollection as ObservableCollection<Entry>;

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

        public event EventHandler CanExecuteChanged;
        public event EventHandler PublishCompleted;

        public void ReadCompletedHandler(object e, EventArgs args)
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

            var entries = ((ICollectionView)parameter).SourceCollection.OfType<Entry>();

            var pendingEntries = entries.Where(p => p.Status.Equals("waiting"));


            try
            {
                await Task.Run(() => Publish(pendingEntries));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());

                foreach (Entry entry in pendingEntries)
                {
                    entry.Message = e.Message;
                }
            }


            if (PublishCompleted != null)
            {
                PublishCompleted(this, new EventArgs());
            }

            ((ICollectionView)parameter).Refresh();
        }

        public abstract void Publish(IEnumerable<Entry> entries);

        private void SetCanExecute(bool canExecute)
        {
            _canExecute = canExecute;

            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, new EventArgs());
            }
        }
        
    }

    public abstract class ReadCommand : ICommand
    {
        protected string _marketplaceCode;
        private bool _canExecute = true;
        private ExcelWorkbook _workbook;

        public ReadCommand(ExcelWorkbook workbook, string marketplaceCode)
        {
            _workbook = workbook;
            _marketplaceCode = marketplaceCode;
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

            var entries = ((ICollectionView)parameter).SourceCollection as ObservableCollection<Entry>;

            try
            {
                var result = await Task.Run<List<Entry>>(() => _workbook.ReadEntry(this.EntryType, _marketplaceCode + "(errors)"));

                if (result.Count() == 0)
                {
                    result = await Task.Run<List<Entry>>(() => _workbook.ReadEntry(this.EntryType, _marketplaceCode));
                }

                result.ForEach(p => entries.Add(p));

                await Task.Run(() => UpdateEntries(entries));

                if (entries.Count != 0)
                {
                    if (ReadCompleted != null)
                    {
                        ReadCompleted(this, new EventArgs());
                    }

                    ((ICollectionView)parameter).Refresh();
                }
                else
                {
                    MessageBox.Show("No entry found");

                    entries.Clear();

                    SetCanExecute(true);
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message);
                SetCanExecute(true);
            }
        }

        public abstract void UpdateEntries(IEnumerable<Entry> entries);

        public abstract Type EntryType { get; }

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
