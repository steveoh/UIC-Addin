using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace UIC_Edit_Workflow
{
    internal class WellInspectionModel : ValidatableBindableBase, IWorkTaskModel
    {
        private readonly object _lockCollection = new object();
        public WellInspectionModel()
        {
            InspectionIds = new ReadOnlyObservableCollection<string>(_inspectionIds);
            Utils.RunOnUiThread(() =>
            {
                BindingOperations.EnableCollectionSynchronization(InspectionIds, _lockCollection);
            });
        }

        private string _createdOn;
        private string _modifiedOn;
        private string _editedBy;


        private readonly ObservableCollection<string> _inspectionIds = new ObservableCollection<string>();

        public ReadOnlyObservableCollection<string> InspectionIds { get; }

        private string _selectedInspectionId;
        public string SelectedInspectionId
        {
            get => _selectedInspectionId;
            set
            {
                SetProperty(ref _selectedInspectionId, value);
                if (_selectedInspectionId != null)
                {
                    // TODO: change to method
                    UpdateModel(_selectedInspectionId);
                }
            }
        }

        public long SelectedOid { get; set; }

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
                        var feature = map.FindStandaloneTables("UICInspection").First();

                        return feature;
                    }).Result;
                }
                return _storeFeature;
            }
        }

        private string _wellFk;
        public string WellFk
        {
            get => _wellFk;
            set => SetProperty(ref _wellFk, value);
        }

        private string _inspectionId;
        public string InspectionId
        {
            get => _inspectionId;
            set => SetProperty(ref _inspectionId, value);
        }

        private string _inspector;
        public string Inspector
        {
            get => _inspector;
            set => SetProperty(ref _inspector, value);
        }

        private string _inspectionType;
        public string InspectionType
        {
            get => _inspectionType;
            set => SetProperty(ref _inspectionType, value);
        }

        private string _inspectionDate;
        public string InspectionDate
        {
            get => _inspectionDate;
            set => SetProperty(ref _inspectionDate, value);
        }

        private string _comments;
        public string Comments
        {
            get => _comments;
            set => SetProperty(ref _comments, value);
        }

        protected override string FieldValueString()
        {
            var sb = new StringBuilder();
            sb.Append(Convert.ToString(InspectionId));
            sb.Append(Convert.ToString(Inspector));
            sb.Append(Convert.ToString(InspectionType));
            sb.Append(Convert.ToString(InspectionDate));
            sb.Append(Convert.ToString(Comments));

            return sb.ToString();
        }

        public async Task AddIdsForFacility(string facilityId)
        {
            await QueuedTask.Run(() =>
            {
                _inspectionIds.Clear();
                var qf = new QueryFilter
                {
                    WhereClause = $"Well_FK = '{facilityId}'"
                };
                using (var cursor = StoreFeature.Search(qf))
                {
                    while (cursor.MoveNext())
                    {
                        using (var row = cursor.Current)
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
                if (string.IsNullOrEmpty(inspectionId))
                {
                    SelectedOid = -1;
                    WellFk = "";
                    InspectionId = "";
                    Inspector = "";
                    InspectionType = "";
                    InspectionDate = "";
                    Comments = "";
                }
                else
                {
                    var qf = new QueryFilter
                    {
                        WhereClause = $"GUID = '{inspectionId}'"
                    };
                    using (var cursor = StoreFeature.Search(qf))
                    {
                        var hasRow = cursor.MoveNext();
                        using (var row = cursor.Current)
                        {
                            SelectedOid = Convert.ToInt64(row["OBJECTID"]);
                            WellFk = Convert.ToString(row["Well_FK"]);
                            InspectionId = Convert.ToString(row["GUID"]);
                            Inspector = Convert.ToString(row["Inspector"]);
                            InspectionType = Convert.ToString(row["InspectionType"]);
                            InspectionDate = Convert.ToString(row["InspectionDate"]);
                            Comments = Convert.ToString(row["Comments"]);
                        }
                    }
                }
            });

            LoadHash = CalculateFieldHash();
        }

        public bool IsInspectionAttributesComplete()
        {
            return !string.IsNullOrEmpty(WellFk) &&
                   !string.IsNullOrEmpty(InspectionId) &&
                   !string.IsNullOrEmpty(Inspector) &&
                   !string.IsNullOrEmpty(InspectionDate) &&
                   !string.IsNullOrEmpty(InspectionType) &&
                   !string.IsNullOrEmpty(Comments);
        }
        //Events
        public async void ControllingIdChangedHandler(string oldGuid, string newGuid)
        {
            await AddIdsForFacility(newGuid);
            if (InspectionIds.Count == 0)
            {
                SelectedInspectionId = string.Empty;
            }
            else
            {
                SelectedInspectionId = InspectionIds.First();
            }

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
                var op = new EditOperation
                {
                    Name = "Update Feature"
                };
                var insp = new Inspector();
                insp.Load(StoreFeature, oidSet);

                insp["Well_FK"] = WellFk;
                insp["GUID"] = InspectionId;
                insp["Inspector"] = InspectionId;
                insp["InspectionType"] = InspectionType;
                insp["InspectionDate"] = InspectionDate;
                insp["Comments"] = Comments;

                op.Modify(insp);
                op.Execute();
                Project.Current.SaveEditsAsync();
            });
        }

        public async void AddNew(string facilityGuid)
        {
            await QueuedTask.Run(() =>
            {
                var newGuid = Guid.NewGuid();
                // Create dictionary of attribute names and values
                var attributes = new Dictionary<string, object>
                {
                    {"Well_FK", facilityGuid},
                    { "GUID", newGuid}
                };

                var createFeatures = new EditOperation
                {
                    Name = "Create Features"
                };
                createFeatures.Create(StoreFeature, attributes);
                createFeatures.Execute();
                var guidString = "{" + newGuid.ToString().ToUpper() + "}";
                _inspectionIds.Add(guidString);
                SelectedInspectionId = guidString;
            });
        }

    }
}
