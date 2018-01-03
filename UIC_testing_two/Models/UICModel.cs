using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Editing.Events;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace UIC_testing_two
{
    class UICModel: INotifyPropertyChanged
    {
        private static readonly UICModel _instance = new UICModel();
        private WellModel _wellModel = WellModel.Instance;
        public event FacilityChangeDelegate FacilityChanged;

        private UICModel()
        {
            _isDirty = false;

        }

        private bool _isDirty;
        private string _uicFacilityId = "";
        private string _facilityGuid;
        private string _countyFips;
        private string _naicsPrimary;
        private string _facilityName = "";
        private string _facilityAddress;
        private string _facilityCity;
        private string _facilityState;
        private string _facilityZip;
        private string _facilityMilepost;
        private string _comments;

        #region properties
        public static UICModel Instance
        {
            get
            {
                return _instance;
            }
        }
        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }

            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string UicFacilityId
        {
            get
            {
                return _uicFacilityId;
            }

            set
            {
                if (!_uicFacilityId.Equals(value))
                {
                    _uicFacilityId = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public string FacilityGuid
        {
            get
            {
                return _facilityGuid;
            }

            set
            {
                if (_facilityGuid != value)
                {
                    _facilityGuid = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public string CountyFips
        {
            get
            {
                return _countyFips;
            }

            set
            {
                if (_countyFips != value)
                {
                    _countyFips = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public string NaicsPrimary
        {
            get
            {
                return _naicsPrimary;
            }

            set
            {
                if (_naicsPrimary != value)
                {
                    _naicsPrimary = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public string FacilityName
        {
            get
            {
                return _facilityName;
            }

            set
            {
                if (!_facilityName.Equals(value))
                {
                    _facilityName = value;
                    this.IsDirty = true;
                    this.OnPropertyChanged();
                }
            }
        }
        public string FacilityAddress
        {
            get
            {
                return _facilityAddress;
            }

            set
            {
                if (_facilityAddress != value)
                {
                    _facilityAddress = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public string FacilityCity
        {
            get
            {
                return _facilityCity;
            }

            set
            {
                if (_facilityCity != value)
                {
                    _facilityCity = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public string FacilityState
        {
            get
            {
                return _facilityState;
            }

            set
            {
                if (_facilityState != value)
                {
                    _facilityState = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public string FacilityZip
        {
            get
            {
                return _facilityZip;
            }

            set
            {
                if (_facilityZip != value)
                {
                    _facilityZip = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public string FacilityMilepost
        {
            get
            {
                return _facilityMilepost;
            }

            set
            {
                if (_facilityMilepost != value)
                {
                    _facilityMilepost = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public string Comments
        {
            get
            {
                return _comments;
            }

            set
            {
                if (_comments != value)
                {
                    _comments = value;
                    this.OnPropertyChanged();
                }
            }
        }
        #endregion

        public async Task UpdateUicFacility(string facilityId)
        {
            if (this.UicFacilityId != facilityId)
            {
                string oldFacId = this.UicFacilityId;
                await QueuedTask.Run(() =>
                {
                    var map = MapView.Active.Map;
                    FeatureLayer uicFacilities = (FeatureLayer)map.FindLayers("UICFacility").First();
                    QueryFilter qf = new QueryFilter()
                    {
                        WhereClause = string.Format("FacilityID = '{0}'", facilityId)
                    };
                    using (RowCursor cursor = uicFacilities.Search(qf))
                    {
                        bool hasRow = cursor.MoveNext();
                        using (Row row = cursor.Current)
                        {
                            this.UicFacilityId = Convert.ToString(row["FacilityID"]);
                            this.CountyFips = Convert.ToString(row["CountyFIPS"]);
                            this.NaicsPrimary = Convert.ToString(row["NAICSPrimary"]);
                            this.FacilityName = Convert.ToString(row["FacilityName"]);
                            this.FacilityAddress = Convert.ToString(row["FacilityAddress"]);
                            this.FacilityCity = Convert.ToString(row["FacilityCity"]);
                            this.FacilityState = Convert.ToString(row["FacilityState"]);
                            this.FacilityZip = Convert.ToString(row["FacilityZip"]);
                            this.FacilityMilepost = Convert.ToString(row["FacilityMilePost"]);
                            this.Comments = Convert.ToString(row["Comments"]);
                            this.FacilityGuid = Convert.ToString(row["GUID"]);
                        }
                    }
                    this.IsDirty = false;
                });
                System.Diagnostics.Debug.WriteLine("uicmodel UpdateUicFacility");
                FacilityChanged(oldFacId, this.UicFacilityId, this._facilityGuid);
            }
        }

        public bool IsCountyFipsComplete()
        {
            return !String.IsNullOrEmpty(this.CountyFips) && this.CountyFips.Length == 5;
        }
        

        #region UicWell

        //private string _wellId;
        //private string _wellName;
        //private string _wellClass;
        //private string _wellSubClass;
        //private string _highPriority;
        //private string _wellSwpz;
        //private string _locationMethod;
        //private string _locationAccuracy;
        //private string _wellComments;

        //private string selectedWellId;

        //private readonly ObservableCollection<string> _facilityWellIds = new ObservableCollection<string>();
        //private readonly ReadOnlyObservableCollection<string> readOnlyWellIds;

        //public string WellId
        //{
        //    get
        //    {
        //        return _wellId;
        //    }

        //    set
        //    {
        //        _wellId = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //public string WellName
        //{
        //    get
        //    {
        //        return _wellName;
        //    }

        //    set
        //    {
        //        _wellName = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //public string WellClass
        //{
        //    get
        //    {
        //        return _wellClass;
        //    }

        //    set
        //    {
        //        _wellClass = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //public string WellSubClass
        //{
        //    get
        //    {
        //        return _wellSubClass;
        //    }

        //    set
        //    {
        //        _wellSubClass = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //public string HighPriority
        //{
        //    get
        //    {
        //        return _highPriority;
        //    }

        //    set
        //    {
        //        _highPriority = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //public string WellSwpz
        //{
        //    get
        //    {
        //        return _wellSwpz;
        //    }

        //    set
        //    {
        //        _wellSwpz = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //public string LocationMethod
        //{
        //    get
        //    {
        //        return _locationMethod;
        //    }

        //    set
        //    {
        //        _locationMethod = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //public string LocationAccuracy
        //{
        //    get
        //    {
        //        return _locationAccuracy;
        //    }

        //    set
        //    {
        //        _locationAccuracy = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //public string WellComments
        //{
        //    get
        //    {
        //        return _wellComments;
        //    }

        //    set
        //    {
        //        _wellComments = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //public ReadOnlyObservableCollection<string> WellIds => readOnlyWellIds;

        //public string SelectedWellId
        //{
        //    get
        //    {
        //        return selectedWellId;
        //    }

        //    set
        //    {

        //        selectedWellId = value;
        //        if (selectedWellId != null)
        //            UpdateUicWell(selectedWellId);
        //        this.OnPropertyChanged();
        //    }
        //}

        //public async Task GetWellsForFacility(string facilityId)
        //{
        //    await QueuedTask.Run(() =>
        //    {
        //            _facilityWellIds.Clear();
        //            var map = MapView.Active.Map;
        //            FeatureLayer uicWells = (FeatureLayer)map.FindLayers("UICWell").First();
        //            QueryFilter qf = new QueryFilter()
        //            {
        //                WhereClause = string.Format("Facility_FK = '{0}'", facilityId)
        //            };
        //            using (RowCursor cursor = uicWells.Search(qf))
        //            {
        //                while (cursor.MoveNext())
        //                {
        //                    using (Row row = cursor.Current)
        //                    {
        //                        _facilityWellIds.Add(Convert.ToString(row["WellID"]));
        //                    }
        //                }
        //            }
        //    });
        //}

        //public async Task UpdateUicWell(string wellId)
        //{
        //    await QueuedTask.Run(() =>
        //    {
        //        if (wellId == null || wellId == String.Empty)
        //        {
        //            this.WellId = "";
        //            this.WellName = "";
        //            this.WellClass = "";
        //            this.WellSubClass = "";
        //            this.HighPriority = "";
        //            this.WellSwpz = "";
        //            this.LocationMethod = "";
        //            this.LocationAccuracy = "";
        //        }
        //        else
        //        {
        //            var map = MapView.Active.Map;
        //            FeatureLayer uicWells = (FeatureLayer)map.FindLayers("UICWell").First();
        //            QueryFilter qf = new QueryFilter()
        //            {
        //                WhereClause = string.Format("WellID = '{0}'", wellId)
        //            };
        //            using (RowCursor cursor = uicWells.Search(qf))
        //            {
        //                bool hasRow = cursor.MoveNext();
        //                using (Row row = cursor.Current)
        //                {
        //                    this.WellId = Convert.ToString(row["WellID"]);
        //                    this.WellName = Convert.ToString(row["WellName"]);
        //                    this.WellClass = Convert.ToString(row["WellClass"]);
        //                    this.WellSubClass = Convert.ToString(row["WellSubClass"]);
        //                    this.HighPriority = Convert.ToString(row["HighPriority"]);
        //                    this.WellSwpz = Convert.ToString(row["WellSWPZ"]);
        //                    this.LocationMethod = Convert.ToString(row["LocationMethod"]);
        //                    this.LocationAccuracy = Convert.ToString(row["LocationAccuracy"]);
        //                }
        //            }
        //        }
        //    });
        //}

        //public bool IsWellAtributesComplete()
        //{
        //    return !String.IsNullOrEmpty(this.WellId) &&
        //           !String.IsNullOrEmpty(this.WellName) &&
        //           !String.IsNullOrEmpty(this.WellClass) &&
        //           !String.IsNullOrEmpty(this.WellSubClass) &&
        //           !String.IsNullOrEmpty(this.HighPriority) &&
        //           !String.IsNullOrEmpty(this.WellSwpz);
        //}
        #endregion UicWell

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
              this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion // INotifyPropertyChanged Members

    }
}
