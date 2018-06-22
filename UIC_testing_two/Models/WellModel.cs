using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using ArcGIS.Desktop.Core;

namespace UIC_Edit_Workflow
{
    class WellModel : ValidatableBindableBase, IWorkTaskModel
    {
        private readonly object lockCollection = new object();
        private static readonly WellModel instance = new WellModel();
        public const string ID_FIELD = "WellID";
        public event ControllingIdChangeDelegate WellChanged;
 
        private WellModel()
        {
            readOnlyWellIds = new ReadOnlyObservableCollection<string>(_facilityWellIds);
            Utils.RunOnUiThread(() =>
            {
                BindingOperations.EnableCollectionSynchronization(readOnlyWellIds, lockCollection);
            });
        }

        // Fields Not yet used, but they will be used eventually
        private string _createdOn;
        private string _modifiedOn;
        private string _editedBy;
        private string _surfaceElevation;

        private readonly ObservableCollection<string> _facilityWellIds = new ObservableCollection<string>();
        private readonly ReadOnlyObservableCollection<string> readOnlyWellIds;

        #region properties
        public ReadOnlyObservableCollection<string> WellIds => readOnlyWellIds;

        private string _selectedWellId;
        public string SelectedWellId
        {
            get
            {
                return _selectedWellId;
            }

            set
            {
                SetProperty(ref _selectedWellId, value);
                if (_selectedWellId != null)
                   UpdateModel(_selectedWellId);
            }
        }
        private long _selectedOid;
        public long SelectedOid
        {
            get
            {
                return _selectedOid;
            }

            set
            {
                _selectedOid = value;
            }
        }
        private FeatureLayer _storeFeature;
        public FeatureLayer StoreFeature
        {
            get
            {
                if (_storeFeature == null)
                {

                    _storeFeature = QueuedTask.Run(() =>
                    {
                        var map = MapView.Active.Map;
                        var feature = (FeatureLayer)map.FindLayers("UICWell").First();
                        return feature as FeatureLayer;
                    }).Result;
                }
                return _storeFeature;
            }
        }

        public static WellModel Instance
        {
            get
            {
                return instance;
            }
        }

        #region tablefields
        private string _wellId;
        [Required]
        public string WellId
        {
            get
            {
                return _wellId;
            }

            set
            {
                SetProperty(ref _wellId, value);
            }
        }

        private string _wellName;
        [Required]
        [UicValidations(ErrorMessage = "{0} is not correct")]
        public string WellName
        {
            get
            {
                return _wellName;
            }

            set
            {
                SetProperty(ref _wellName, value);
            }
        }

        private string _wellClass;
        [Required]
        public string WellClass
        {
            get
            {
                return _wellClass;
            }

            set
            {
                SetProperty(ref _wellClass, value);
            }
        }

        private string _wellSubClass;
        [Required]
        public string WellSubClass
        {
            get
            {
                return _wellSubClass;
            }

            set
            {
                SetProperty(ref _wellSubClass, value);
            }
        }

        private string _highPriority;
        [Required]
        public string HighPriority
        {
            get
            {
                return _highPriority;
            }

            set
            {
                SetProperty(ref _highPriority, value);
            }
        }

        private string _wellSwpz;
        [Required]
        public string WellSwpz
        {
            get
            {
                return _wellSwpz;
            }

            set
            {
                SetProperty(ref _wellSwpz, value);
            }
        }

        private string _locationMethod;
        [Required]
        public string LocationMethod
        {
            get
            {
                return _locationMethod;
            }

            set
            {
                SetProperty(ref _locationMethod, value);
            
            }
        }

        private string _locationAccuracy;
        [Required]
        public string LocationAccuracy
        {
            get
            {
                return _locationAccuracy;
            }

            set
            {
                SetProperty(ref _locationAccuracy, value);
            }
        }

        private string _comments;
        public string Comments
        {
            get
            {
                return _comments;
            }

            set
            {
                SetProperty(ref _comments, value);
            }
        }

        private string _wellGuid;
        public string WellGuid
        {
            get
            {
                return _wellGuid;
            }

            set
            {
                SetProperty(ref _wellGuid, value);
            }
        }

        #endregion // End tablefields
        #endregion

        public async Task AddIdsForFacility(string facilityId)
        {
            await QueuedTask.Run(() =>
            {
                _facilityWellIds.Clear();
                var map = MapView.Active.Map;
                QueryFilter qf = new QueryFilter()
                {
                    WhereClause = string.Format("Facility_FK = '{0}'", facilityId)
                };
                using (RowCursor cursor = StoreFeature.Search(qf))
                {
                    while (cursor.MoveNext())
                    {
                        using (Row row = cursor.Current)
                        {
                            _facilityWellIds.Add(Convert.ToString(row["WellID"]));
                        }
                    }
                }
            });
        }

        public async Task UpdateModel(string wellId)
        {
            string oldWellGuid = WellGuid;
            await QueuedTask.Run(() =>
            {

                if (wellId == null || wellId == String.Empty)
                {
                    this.SelectedOid = -1;
                    this.WellId = "";
                    this.WellName = "";
                    this.WellClass = "";
                    this.WellSubClass = "";
                    this.HighPriority = "";
                    this.WellSwpz = "";
                    this.LocationMethod = "";
                    this.LocationAccuracy = "";
                    this.Comments = "";
                    this.WellGuid = "";
                }
                else
                {
                    QueryFilter qf = new QueryFilter()
                    {
                        WhereClause = string.Format("WellID = '{0}'", wellId)
                    };
                    using (RowCursor cursor = StoreFeature.Search(qf))
                    {
                        bool hasRow = cursor.MoveNext();
                        using (Row row = cursor.Current)
                        {
                            this.SelectedOid = Convert.ToInt64(row["OBJECTID"]);
                            this.WellId = Convert.ToString(row["WellID"]);
                            this.WellName = Convert.ToString(row["WellName"]);
                            this.WellClass = Convert.ToString(row["WellClass"]);
                            this.WellSubClass = Convert.ToString(row["WellSubClass"]);
                            this.HighPriority = Convert.ToString(row["HighPriority"]);
                            this.WellSwpz = Convert.ToString(row["WellSWPZ"]);
                            this.LocationMethod = Convert.ToString(row["LocationMethod"]);
                            this.LocationAccuracy = Convert.ToString(row["LocationAccuracy"]);
                            this.Comments = Convert.ToString(row["Comments"]);
                            this.WellGuid = Convert.ToString(row["GUID"]);
                        }
                    }
                }
            });
            LoadHash = calculateFieldHash();
            WellChanged(oldWellGuid, this.WellGuid);
        }

        public Task SaveChanges()
        {
            Task t = QueuedTask.Run(() =>
            {
                //Create list of oids to update
                var oidSet = new List<long>() { SelectedOid };
                //Create edit operation and update
                var op = new ArcGIS.Desktop.Editing.EditOperation();
                op.Name = "Update Feature";
                var insp = new ArcGIS.Desktop.Editing.Attributes.Inspector();
                insp.Load(StoreFeature, oidSet);

                insp["WellName"] = this.WellName;
                insp["WellClass"] = this.WellClass;
                insp["WellSubClass"] = this.WellSubClass;
                insp["HighPriority"] = this.HighPriority;
                insp["WellSWPZ"] = this.WellSwpz;
                insp["LocationMethod"] = this.LocationMethod;
                insp["LocationAccuracy"] = this.LocationAccuracy;

                op.Modify(insp);
                op.Execute();
                Project.Current.SaveEditsAsync();
            });
            return t;
        }

        public async void AddNew(long objectId, string facilityGuid, string countyFips)
        {
            await QueuedTask.Run(() =>
            {
                //Create list contianing the new well OID
                var oidSet = new List<long>() { objectId };
                //Create edit operation and update
                var op = new ArcGIS.Desktop.Editing.EditOperation();
                op.Name = "Update date";
                var insp = new ArcGIS.Desktop.Editing.Attributes.Inspector();
                insp.Load(StoreFeature, oidSet);

                long fips;
                long.TryParse(countyFips, out fips);

                insp["Facility_FK"] = facilityGuid;

                Guid newGuid = Guid.NewGuid();
                string guidLast8 = newGuid.ToString();
                guidLast8 = guidLast8.Substring(guidLast8.Length - 8);
                insp["GUID"] = newGuid;

                string newWellId = String.Format("UTU{0}{1}{2}", countyFips.Substring(countyFips.Length - 2), insp["WellClass"], guidLast8).ToUpper();
                insp[WellModel.ID_FIELD] = newWellId;

                op.Modify(insp);
                op.Execute();
                _facilityWellIds.Add(newWellId);
                SelectedWellId = newWellId;

            });

        }
        #region validation 
        public bool IsWellAttributesComplete()
        {
            return !String.IsNullOrEmpty(this.WellId) &&
                   !String.IsNullOrEmpty(this.WellName) &&
                   !String.IsNullOrEmpty(this.WellClass) &&
                   !String.IsNullOrEmpty(this.WellSubClass) &&
                   !String.IsNullOrEmpty(this.HighPriority) &&
                   !String.IsNullOrEmpty(this.WellSwpz);
        }

        public bool IsWellNameCorrect()
        {
            bool isWellNameError = GetErrors("WellName") == null;
            return !String.IsNullOrEmpty(this.WellName) && isWellNameError;
        }
        #endregion

        protected override string fieldValueString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Convert.ToString(WellId));
            sb.Append(Convert.ToString(WellName));
            sb.Append(Convert.ToString(WellClass));
            sb.Append(Convert.ToString(WellSubClass));
            sb.Append(Convert.ToString(HighPriority));
            sb.Append(Convert.ToString(WellSwpz));
            sb.Append(Convert.ToString(LocationMethod));
            sb.Append(Convert.ToString(LocationAccuracy));
            sb.Append(Convert.ToString(Comments));
            sb.Append(Convert.ToString(WellGuid));
            return sb.ToString();
        }

        #region events
        public async void ControllingIdChangedHandler(string oldId, string facGuid)
        {
            await AddIdsForFacility(facGuid);
            if (WellIds.Count == 0)
            {
                //await UpdateUicWell(null);
                SelectedWellId = String.Empty;
            }
            else
            {
                //await UpdateUicWell(WellIds.First());
                SelectedWellId = WellIds.First();
            }

        }
        #endregion
    }
}
