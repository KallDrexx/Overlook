using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Overlook.Common.Data;
using Overlook.Gui.Services;

namespace Overlook.Gui.ViewModels
{
    public class WorkAreaViewModel : ViewModelBase
    {
        private readonly List<Metric> _allMetrics; 
        private readonly ObservableCollection<string> _metricDevices;
        private readonly ObservableCollection<string> _metricCategories;
        private readonly ObservableCollection<string> _metricNames;
        private readonly ObservableCollection<Metric> _queryMetrics;
        private readonly QueryService _queryService;
 
        private string _serverUrl;
        private string _selectedMetricDevice;
        private string _selectedMetricCategory;
        private string _selectedMetricName;
        private Metric _selectedQueryMetric;

        public WorkAreaViewModel(QueryService queryService)
        {
            _queryService = queryService;
            _allMetrics = new List<Metric>();
            _metricDevices = new ObservableCollection<string>();
            _metricCategories = new ObservableCollection<string>();
            _metricNames = new ObservableCollection<string>();
            _queryMetrics = new ObservableCollection<Metric>();

            if (IsInDesignMode)
            {
                SetupDesignData();
            }
            else
            {
                InitializeCommands();
            }
        }

        public IEnumerable<string> MetricDevices { get { return _metricDevices; } }
        public IEnumerable<string> MetricCategories { get { return _metricCategories; } }
        public IEnumerable<string> MetricNames { get { return _metricNames; } }
        public IEnumerable<Metric> QueryMetrics { get { return _queryMetrics; } }

        public RelayCommand AddMetricToQueryList { get; private set; }
        public RelayCommand RemoveMetricFromQueryList { get; private set; }
        public RelayCommand GetMetricsFromServerCommand { get; private set; }

        public bool MetricsLoaded { get { return _allMetrics.Count > 0; } }

        public string ServerUrl
        {
            get { return _serverUrl; }
            set { Set(() => ServerUrl, ref _serverUrl, value); }
        }

        public string SelectedMetricDevice
        {
            get { return _selectedMetricDevice; }
            set
            {
                Set(() => SelectedMetricDevice, ref _selectedMetricDevice, value);
                UpdateMetricCategories();
            }
        }

        public string SelectedMetricCategory
        {
            get { return _selectedMetricCategory; }
            set
            {
                Set(() => SelectedMetricCategory, ref _selectedMetricCategory, value);
                UpdateMetricNames();
            }
        }

        public string SelectedMetricName
        {
            get { return _selectedMetricName; }
            set { Set(() => SelectedMetricName, ref _selectedMetricName, value); }
        }

        public Metric SelectedQueryMetric
        {
            get { return _selectedQueryMetric; }
            set { Set(() => SelectedQueryMetric, ref _selectedQueryMetric, value); }
        }

        private void SetupDesignData()
        {
            _serverUrl = "http://some.url:4566/";
            _metricDevices.Add("Device1");
            _metricDevices.Add("Device2");
            _metricDevices.Add("Device3");

            _metricCategories.Add("Category1");
            _metricCategories.Add("Category2");
            _metricCategories.Add("Category3");

            _metricNames.Add("Name1");
            _metricNames.Add("Name2");
            _metricNames.Add("Name3");

            _queryMetrics.Add(new Metric("Device1", "Category1", "name1", ""));
            _queryMetrics.Add(new Metric("Device2", "Category2", "name2", ""));
        }

        private void InitializeCommands()
        {
            AddMetricToQueryList = new RelayCommand(() =>
            {
                var metric = _allMetrics.Where(x => x.Device == _selectedMetricDevice)
                                        .Where(x => x.Category == _selectedMetricCategory)
                                        .FirstOrDefault(x => x.Name == _selectedMetricName);

                if (metric != null && !_queryMetrics.Contains(metric))
                    _queryMetrics.Add(metric);
            });

            RemoveMetricFromQueryList = new RelayCommand(() =>
            {
                if (_selectedQueryMetric != null)
                    _queryMetrics.Remove(_selectedQueryMetric);
            });

            GetMetricsFromServerCommand = new RelayCommand(() =>
            {
                _allMetrics.Clear();
                _queryMetrics.Clear();
                _allMetrics.AddRange(_queryService.GetAvailableMetrics(ServerUrl));

                UpdateMetricDevices();
                RaisePropertyChanged(() => MetricsLoaded);
            });
        }

        private void UpdateMetricDevices()
        {
            var devices = _allMetrics.Select(x => x.Device)
                                     .Distinct()
                                     .OrderBy(x => x)
                                     .ToArray();

            _metricDevices.Clear();
            foreach (var device in devices)
                _metricDevices.Add(device);

            UpdateMetricCategories();
        }

        private void UpdateMetricCategories()
        {
            _metricCategories.Clear();

            var categories = _allMetrics.Where(x => x.Device == _selectedMetricDevice)
                                        .Select(x => x.Category)
                                        .OrderBy(x => x)
                                        .Distinct()
                                        .ToList();

            foreach (var category in categories)
                _metricCategories.Add(category);

            UpdateMetricNames();
        }

        private void UpdateMetricNames()
        {
            _metricNames.Clear();

            var names = _allMetrics.Where(x => x.Device == _selectedMetricDevice)
                                    .Where(x => x.Category == _selectedMetricCategory)
                                    .Select(x => x.Name)
                                    .OrderBy(x => x)
                                    .Distinct()
                                    .ToList();

            foreach (var name in names)
                _metricNames.Add(name);
        }
    }
}
