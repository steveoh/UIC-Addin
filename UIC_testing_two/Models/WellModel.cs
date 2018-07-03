using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace UIC_Edit_Workflow.Models
{
    internal class WellModel : ValidatableBindableBase, IWorkTaskModel
    {
        private readonly object _lockCollection = new object();
        public const string IdField = "WellID";
        public event ControllingIdChangeDelegate WellChanged;

        public WellModel()
        {
            WellIds = new ReadOnlyObservableCollection<string>(_facilityWellIds);
            Utils.RunOnUiThread(() =>
            {
                BindingOperations.EnableCollectionSynchronization(WellIds, _lockCollection);
            });
        }

        // Fields not yet used, but they will be used eventually
        private string _createdOn;
        private string _modifiedOn;
        private string _editedBy;
        private string _surfaceElevation;

        private readonly ObservableCollection<string> _facilityWellIds = new ObservableCollection<string>();

        #region properties
        public ReadOnlyObservableCollection<string> WellIds { get; }

        private string _selectedWellId;
        public string SelectedWellId
        {
            get => _selectedWellId;

            set
            {
                SetProperty(ref _selectedWellId, value);
                if (_selectedWellId != null)
                {
                    // TODO change to method
                    UpdateModel(_selectedWellId);
                }
            }
        }

        public long SelectedOid { get; set; }

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

                        return feature;
                    }).Result;
                }

                return _storeFeature;
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
                var qf = new QueryFilter
                {
                    WhereClause = $"Facility_FK = '{facilityId}'"
                };
                using (var cursor = StoreFeature.Search(qf))
                {
                    while (cursor.MoveNext())
                    {
                        using (var row = cursor.Current)
                        {
                            _facilityWellIds.Add(Convert.ToString(row["WellID"]));
                        }
                    }
                }
            });
        }

        public async Task UpdateModel(string wellId)
        {
            var oldWellGuid = WellGuid;
            await QueuedTask.Run(() =>
            {

                if (string.IsNullOrEmpty(wellId))
                {
                    SelectedOid = -1;
                    WellId = "";
                    WellName = "";
                    WellClass = "";
                    WellSubClass = "";
                    HighPriority = "";
                    WellSwpz = "";
                    LocationMethod = "";
                    LocationAccuracy = "";
                    Comments = "";
                    WellGuid = "";
                }
                else
                {
                    var qf = new QueryFilter
                    {
                        WhereClause = $"WellID = '{wellId}'"
                    };
                    using (var cursor = StoreFeature.Search(qf))
                    {
                        var hasRow = cursor.MoveNext();
                        using (var row = cursor.Current)
                        {
                            SelectedOid = Convert.ToInt64(row["OBJECTID"]);
                            WellId = Convert.ToString(row["WellID"]);
                            WellName = Convert.ToString(row["WellName"]);
                            WellClass = Convert.ToString(row["WellClass"]);
                            WellSubClass = Convert.ToString(row["WellSubClass"]);
                            HighPriority = Convert.ToString(row["HighPriority"]);
                            WellSwpz = Convert.ToString(row["WellSWPZ"]);
                            LocationMethod = Convert.ToString(row["LocationMethod"]);
                            LocationAccuracy = Convert.ToString(row["LocationAccuracy"]);
                            Comments = Convert.ToString(row["Comments"]);
                            WellGuid = Convert.ToString(row["GUID"]);
                        }
                    }
                }
            });

            LoadHash = CalculateFieldHash();
            WellChanged(oldWellGuid, this.WellGuid);
        }

        public Task SaveChanges()
        {
            return QueuedTask.Run(() =>
            {
                //Create list of oids to update
                var oidSet = new List<long>
                {
                    SelectedOid
                };
                //Create edit operation and update
                var op = new ArcGIS.Desktop.Editing.EditOperation
                {
                    Name = "Update Feature"
                };
                var insp = new ArcGIS.Desktop.Editing.Attributes.Inspector();
                insp.Load(StoreFeature, oidSet);

                insp["WellName"] = WellName;
                insp["WellClass"] = WellClass;
                insp["WellSubClass"] = WellSubClass;
                insp["HighPriority"] = HighPriority;
                insp["WellSWPZ"] = WellSwpz;
                insp["LocationMethod"] = LocationMethod;
                insp["LocationAccuracy"] = LocationAccuracy;

                op.Modify(insp);
                op.Execute();
                Project.Current.SaveEditsAsync();
            });
        }

        public async void AddNew(long objectId, string facilityGuid, string countyFips)
        {
            await QueuedTask.Run(() =>
            {
                //Create list contianing the new well OID
                var oidSet = new List<long>
                {
                    objectId
                };
                //Create edit operation and update
                var op = new ArcGIS.Desktop.Editing.EditOperation
                {
                    Name = "Update date"
                };
                var insp = new ArcGIS.Desktop.Editing.Attributes.Inspector();
                insp.Load(StoreFeature, oidSet);

                long.TryParse(countyFips, out long fips);

                insp["Facility_FK"] = facilityGuid;

                var newGuid = Guid.NewGuid();
                var guidLast8 = newGuid.ToString();
                guidLast8 = guidLast8.Substring(guidLast8.Length - 8);
                insp["GUID"] = newGuid;

                var newWellId = $"UTU{countyFips.Substring(countyFips.Length - 2)}{insp["WellClass"]}{guidLast8}".ToUpper();
                insp[IdField] = newWellId;

                op.Modify(insp);
                op.Execute();
                _facilityWellIds.Add(newWellId);
                SelectedWellId = newWellId;

            });

        }
        #region validation 
        public bool IsWellAttributesComplete()
        {
            return !string.IsNullOrEmpty(WellId) &&
                   !string.IsNullOrEmpty(WellName) &&
                   !string.IsNullOrEmpty(WellClass) &&
                   !string.IsNullOrEmpty(WellSubClass) &&
                   !string.IsNullOrEmpty(HighPriority) &&
                   !string.IsNullOrEmpty(WellSwpz);
        }

        public bool IsWellNameCorrect()
        {
            var isWellNameError = GetErrors("WellName") == null;

            return !string.IsNullOrEmpty(WellName) && isWellNameError;
        }
        #endregion

        protected override string FieldValueString()
        {
            var sb = new StringBuilder();
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
                SelectedWellId = string.Empty;
            }
            else
            {
                SelectedWellId = WellIds.First();
            }

        }
        #endregion
    }
}
