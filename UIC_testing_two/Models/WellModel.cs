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

namespace UIC_Edit_Workflow
{
    class WellModel : ValidatableBindableBase, IWorkTaskModel
    {
        private readonly object lockCollection = new object();
        private static readonly WellModel instance = new WellModel();
        //private UICModel uicModel = null;
        private bool _isDirty;
 
        private WellModel()
        {
            //uicModel = UICModel.Instance;
            //uicModel.FacilityChanged = new FacilityChangeDelegate(facChangeHandler);
            readOnlyWellIds = new ReadOnlyObservableCollection<string>(_facilityWellIds);
            Utils.RunOnUiThread(() =>
            {
                BindingOperations.EnableCollectionSynchronization(readOnlyWellIds, lockCollection);
            });
            _isDirty = false;

        }

        private string _wellId;
        private string _wellName;
        private string _wellClass;
        private string _wellSubClass;
        private string _highPriority;
        private string _wellSwpz;
        private string _locationMethod;
        private string _locationAccuracy;
        private string _wellComments;

        private string _createdOn;
        private string _modifiedOn;
        private string _editedBy;
        private string _surfaceElevation;


        private string selectedWellId;

        private readonly ObservableCollection<string> _facilityWellIds = new ObservableCollection<string>();
        private readonly ReadOnlyObservableCollection<string> readOnlyWellIds;

        #region properties
        public ReadOnlyObservableCollection<string> WellIds => readOnlyWellIds;

        public string SelectedWellId
        {
            get
            {
                return selectedWellId;
            }

            set
            {
                SetProperty(ref selectedWellId, value);
                if (selectedWellId != null)
                    UpdateModel(selectedWellId);
                //if (selectedWellId != value)
                //{
                //    selectedWellId = value;

                //   OnPropertyChanged();
                //}
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
                _isDirty = true;
            }
        }

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

        public string WellComments
        {
            get
            {
                return _wellComments;
            }

            set
            {
                SetProperty(ref _wellComments, value);
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
                FeatureLayer uicWells = (FeatureLayer)map.FindLayers("UICWell").First();
                QueryFilter qf = new QueryFilter()
                {
                    WhereClause = string.Format("Facility_FK = '{0}'", facilityId)
                };
                using (RowCursor cursor = uicWells.Search(qf))
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
            await QueuedTask.Run(() =>
            {

                if (wellId == null || wellId == String.Empty)
                {
                    this.WellId = "";
                    this.WellName = "";
                    this.WellClass = "";
                    this.WellSubClass = "";
                    this.HighPriority = "";
                    this.WellSwpz = "";
                    this.LocationMethod = "";
                    this.LocationAccuracy = "";
                }
                else
                {
                    var map = MapView.Active.Map;
                    FeatureLayer uicWells = (FeatureLayer)map.FindLayers("UICWell").First();
                    QueryFilter qf = new QueryFilter()
                    {
                        WhereClause = string.Format("WellID = '{0}'", wellId)
                    };
                    using (RowCursor cursor = uicWells.Search(qf))
                    {
                        bool hasRow = cursor.MoveNext();
                        using (Row row = cursor.Current)
                        {
                            this.WellId = Convert.ToString(row["WellID"]);
                            this.WellName = Convert.ToString(row["WellName"]);
                            this.WellClass = Convert.ToString(row["WellClass"]);
                            this.WellSubClass = Convert.ToString(row["WellSubClass"]);
                            this.HighPriority = Convert.ToString(row["HighPriority"]);
                            this.WellSwpz = Convert.ToString(row["WellSWPZ"]);
                            this.LocationMethod = Convert.ToString(row["LocationMethod"]);
                            this.LocationAccuracy = Convert.ToString(row["LocationAccuracy"]);
                        }
                    }
                }
            });
        }

        public bool IsWellAtributesComplete()
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

        //Events
        public async void FacilityChangeHandler(string oldId, string newId, string facGuid)
        {
            System.Diagnostics.Debug.WriteLine($"Old id {oldId}, New Id {newId}");
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

    }
}
