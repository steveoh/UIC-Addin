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

namespace UIC_Edit_Workflow
{
    class WellModel : ValidatableBindableBase, IWorkTaskModel
    {
        private readonly object lockCollection = new object();
        private static readonly WellModel instance = new WellModel();
        public const string ID_FIELD = "WellID";
        public event ControllingIdChangeDelegate WellChanged;
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
            //LoadHash = calculateFieldHash();

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
        private string _guidValue;

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

        public string WellGuid
        {
            get
            {
                return _guidValue;
            }

            set
            {
                SetProperty(ref _guidValue, value);
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
            string oldWellGuid = WellGuid;
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
                            this.WellGuid = Convert.ToString(row["GUID"]);
                        }
                    }
                }
            });
            LoadHash = calculateFieldHash();
            WellChanged(oldWellGuid, this.WellGuid);
        }

        public async void AddNew(long objectId, string facilityGuid, string countyFips)
        {
            await QueuedTask.Run(() =>
            {
                //Create list of oids to update
                var oidSet = new List<long>() { objectId };
                //Create edit operation and update
                var op = new ArcGIS.Desktop.Editing.EditOperation();
                op.Name = "Update date";
                var insp = new ArcGIS.Desktop.Editing.Attributes.Inspector();
                var map = MapView.Active.Map;
                FeatureLayer uicWells = (FeatureLayer)map.FindLayers("UICWell").First();
                insp.Load(uicWells, oidSet);

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
        //Validation 
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
            sb.Append(Convert.ToString(WellComments));
            sb.Append(Convert.ToString(WellGuid));
            return sb.ToString();
        }

        //private string calculateFieldHash()
        //{
        //    string input = fieldValueString();
        //    // step 1, calculate MD5 hash from input

        //    MD5 md5 = MD5.Create();

        //    byte[] inputBytes = Encoding.ASCII.GetBytes(input);

        //    byte[] hash = md5.ComputeHash(inputBytes);

        //    // step 2, convert byte array to hex string

        //    StringBuilder sb = new StringBuilder();

        //    for (int i = 0; i < hash.Length; i++)

        //    {

        //        sb.Append(hash[i].ToString("X2"));

        //    }

        //    return sb.ToString();

        //}

        //public bool HasModelChanged()
        //{
        //    return LoadHash.Equals(calculateFieldHash());
        //}

        //Events
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

    }
}
