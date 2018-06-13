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
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Core;

namespace UIC_Edit_Workflow
{
    class WellInspectionModel : ValidatableBindableBase, IWorkTaskModel
    {
        private readonly object lockCollection = new object();
        private static readonly WellInspectionModel instance = new WellInspectionModel();

        private WellInspectionModel()
        {
            readOnlyInspectionIds = new ReadOnlyObservableCollection<string>(_inspectionIds);
            Utils.RunOnUiThread(() =>
            {
                BindingOperations.EnableCollectionSynchronization(readOnlyInspectionIds, lockCollection);
            });
            _isDirty = false;
        }

        private string _selectedInspectionId;
        private bool _isDirty;

        private string _wellFk;
        private string _inspectionId;
        private string _inspector;
        private string _inspectionType;
        private string _inspectionDate;
        private string _comments;

        private string _createdOn;
        private string _modifiedOn;
        private string _editedBy;


        private readonly ObservableCollection<string> _inspectionIds = new ObservableCollection<string>();
        private readonly ReadOnlyObservableCollection<string> readOnlyInspectionIds;

        #region properties
        public ReadOnlyObservableCollection<string> InspectionIds => readOnlyInspectionIds;

        public string SelectedInspectionId
        {
            get
            {
                return _selectedInspectionId;
            }

            set
            {
                SetProperty(ref _selectedInspectionId, value);
                if (_selectedInspectionId != null)
                    UpdateModel(_selectedInspectionId);
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
        private StandaloneTable _storeFeature;
        public StandaloneTable StoreFeature
        {
            get
            {
                if (_storeFeature == null)
                {

                    _storeFeature = QueuedTask.Run(() =>
                    {
                        var map = MapView.Active.Map;
                        var feature = (StandaloneTable)map.FindStandaloneTables("UICInspection").First();
                        return feature as StandaloneTable;
                    }).Result;
                }
                return _storeFeature;
            }
        }

        public static WellInspectionModel Instance
        {
            get
            {
                return instance;
            }
        }

        #region tablefields
        public string WellFk
        {
            get
            {
                return _wellFk;
            }

            set
            {
                SetProperty(ref _wellFk, value);
            }
        }

        public string InspectionId
        {
            get
            {
                return _inspectionId;
            }

            set
            {
                SetProperty(ref _inspectionId, value);
            }
        }

        public string Inspector
        {
            get
            {
                return _inspector;
            }

            set
            {
                SetProperty(ref _inspector, value);
            }
        }

        public string InspectionType
        {
            get
            {
                return _inspectionType;
            }

            set
            {
                SetProperty(ref _inspectionType, value);
                _isDirty = true;
            }
        }

        public string InspectionDate
        {
            get
            {
                return _inspectionDate;
            }

            set
            {
                SetProperty(ref _inspectionDate, value);
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
                SetProperty(ref _comments, value);
            }
        }

        #endregion // End tablefields
        protected override string fieldValueString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Convert.ToString(InspectionId));
            sb.Append(Convert.ToString(Inspector));
            sb.Append(Convert.ToString(InspectionType));
            sb.Append(Convert.ToString(InspectionDate));
            sb.Append(Convert.ToString(Comments));
            return sb.ToString();
        }
        #endregion

        public async Task AddIdsForFacility(string facilityId)
        {
            await QueuedTask.Run(() =>
            {
                _inspectionIds.Clear();
                var map = MapView.Active.Map;
                StandaloneTable uicWells = (StandaloneTable)map.FindStandaloneTables("UICInspection").First();
                QueryFilter qf = new QueryFilter()
                {
                    WhereClause = string.Format("Well_FK = '{0}'", facilityId)
                };
                using (RowCursor cursor = uicWells.Search(qf))
                {
                    while (cursor.MoveNext())
                    {
                        using (Row row = cursor.Current)
                        {
                            _inspectionIds.Add(Convert.ToString(row["GUID"]));
                        }
                    }
                }
            });
        }

        public async Task UpdateModel(string inspectionId)
        {
            await QueuedTask.Run(() =>
            {

                if (inspectionId == null || inspectionId == String.Empty)
                {
                    this.SelectedOid = -1;
                    this.WellFk = "";
                    this.InspectionId = "";
                    this.Inspector = "";
                    this.InspectionType = "";
                    this.InspectionDate = "";
                    this.Comments = "";
                }
                else
                {
                    var map = MapView.Active.Map;
                    StandaloneTable uicInspection = (StandaloneTable)map.FindStandaloneTables("UICInspection").First();
                    QueryFilter qf = new QueryFilter()
                    {
                        WhereClause = string.Format("GUID = '{0}'", inspectionId)
                    };
                    using (RowCursor cursor = uicInspection.Search(qf))
                    {
                        bool hasRow = cursor.MoveNext();
                        using (Row row = cursor.Current)
                        {
                            this.SelectedOid = Convert.ToInt64(row["OBJECTID"]);
                            this.WellFk = Convert.ToString(row["Well_FK"]);
                            this.InspectionId = Convert.ToString(row["GUID"]);
                            this.Inspector = Convert.ToString(row["Inspector"]);
                            this.InspectionType = Convert.ToString(row["InspectionType"]);
                            this.InspectionDate = Convert.ToString(row["InspectionDate"]);
                            this.Comments = Convert.ToString(row["Comments"]);
                        }
                    }
                }
            });
            LoadHash = calculateFieldHash();
        }

        public bool IsInspectionAttributesComplete()
        {
            return !String.IsNullOrEmpty(this.WellFk) &&
                   !String.IsNullOrEmpty(this.InspectionId) &&
                   !String.IsNullOrEmpty(this.Inspector) &&
                   !String.IsNullOrEmpty(this.InspectionDate) &&
                   !String.IsNullOrEmpty(this.InspectionType) &&
                   !String.IsNullOrEmpty(this.Comments);
        }
        //Events
        public async void ControllingIdChangedHandler(string oldGuid, string newGuid)
        {
            //System.Diagnostics.Debug.WriteLine($"Old id {oldId}, New Id {newId}");
            await AddIdsForFacility(newGuid);
            if (InspectionIds.Count == 0)
            {
                //await UpdateUicWell(null);
                SelectedInspectionId = String.Empty;
            }
            else
            {
                //await UpdateUicWell(WellIds.First());
                SelectedInspectionId = InspectionIds.First();
            }

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

                insp["Well_FK"] = this.WellFk;
                insp["GUID"] = this.InspectionId;
                insp["Inspector"] = this.InspectionId;
                insp["InspectionType"] = this.InspectionType;
                insp["InspectionDate"] = this.InspectionDate;
                insp["Comments"] = this.Comments;

                op.Modify(insp);
                op.Execute();
                Project.Current.SaveEditsAsync();
            });
            return t;
        }

        public async void AddNew(string facilityGuid)
        {
            await QueuedTask.Run(() =>
            {
                Guid newGuid = Guid.NewGuid();
                // Create dictionary of attribute names and values
                var attributes = new Dictionary<string, object>();
                attributes.Add("Well_FK", facilityGuid);
                attributes.Add("GUID", newGuid);

                var createFeatures = new EditOperation();
                createFeatures.Name = "Create Features";
                createFeatures.Create(StoreFeature, attributes);
                createFeatures.Execute();
                string guidString = "{" + newGuid.ToString().ToUpper() + "}";
                _inspectionIds.Add(guidString);
                SelectedInspectionId = guidString;
            });
        }

    }
}
