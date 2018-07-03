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
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace UIC_Edit_Workflow.Models
{
    internal class AuthorizationModel : ValidatableBindableBase, IWorkTaskModel
    {
        private readonly object _lockCollection = new object();

        //private UICModel uicModel = null;

        public AuthorizationModel()
        {
            //uicModel = UICModel.Instance;
            //uicModel.FacilityChanged = new FacilityChangeDelegate(facChangeHandler);
            AuthIds = new ReadOnlyObservableCollection<string>(_facilityAuthIds);
            Utils.RunOnUiThread(() =>
            {
                BindingOperations.EnableCollectionSynchronization(AuthIds, _lockCollection);
            });
        }

        private string _authId;
        private string _authType;
        private string _sectorType;
        private string _startDate;
        private string _expirationDate;
        private string _comments;
        private string _selectedAuthId;

        private readonly ObservableCollection<string> _facilityAuthIds = new ObservableCollection<string>();

        public ReadOnlyObservableCollection<string> AuthIds { get; }

        public string SelectedAuthId
        {
            get => _selectedAuthId;

            set
            {
                SetProperty(ref _selectedAuthId, value);
                if (_selectedAuthId != null)
                {
                    // TODO: create method? this is bad. cannot be awaited
                    UpdateModel(_selectedAuthId);
                }
            }
        }

        public long SelectedOid { get; set; }

        private StandaloneTable _storeFeature;
        private bool _isDirty;

        public StandaloneTable StoreFeature
        {
            get
            {
                if (_storeFeature == null)
                {

                    _storeFeature = QueuedTask.Run(() =>
                    {
                        var map = MapView.Active.Map;
                        var feature = map.FindStandaloneTables("UICAuthorization").First();

                        return feature;
                    }).Result;
                }

                return _storeFeature;
            }
        }

        [Required]
        public string AuthId
        {
            get => _authId;
            set => SetProperty(ref _authId, value);
        }

        [Required]
        public string AuthType
        {
            get => _authType;
            set
            {
                SetProperty(ref _authType, value);
                _isDirty = true;
            }
        }

        [Required]
        public string SectorType
        {
            get => _sectorType;
            set => SetProperty(ref _sectorType, value);
        }

        [Required]
        public string StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        [Required]
        public string ExpirationDate
        {
            get => _expirationDate;
            set => SetProperty(ref _expirationDate, value);
        }

        [Required]
        public string Comments
        {
            get => _comments;
            set => SetProperty(ref _comments, value);
        }

        protected override string FieldValueString()
        {
            var sb = new StringBuilder();
            sb.Append(Convert.ToString(AuthId));
            sb.Append(Convert.ToString(AuthType));
            sb.Append(Convert.ToString(SectorType));
            sb.Append(Convert.ToString(StartDate));
            sb.Append(Convert.ToString(ExpirationDate));
            sb.Append(Convert.ToString(Comments));

            return sb.ToString();
        }

        public async Task AddIdsForFacility(string facilityId)
        {
            await QueuedTask.Run(() =>
            {
                _facilityAuthIds.Clear();
                var map = MapView.Active.Map;
                var uicAuth = map.FindStandaloneTables("UICAuthorization").First();
                var qf = new QueryFilter
                {
                    WhereClause = $"Facility_FK = '{facilityId}'"
                };

                using (var cursor = uicAuth.Search(qf))
                {
                    while (cursor.MoveNext())
                    {
                        using (var row = cursor.Current)
                        {
                            _facilityAuthIds.Add(Convert.ToString(row["AuthorizationID"]));
                        }
                    }
                }
            });
        }

        public async Task UpdateModel(string authId)
        {
            await QueuedTask.Run(() =>
            {
                if (string.IsNullOrEmpty(authId))
                {
                    SelectedOid = -1;
                    AuthId = "";
                    AuthType = "";
                    SectorType = "";
                    StartDate = "";
                    ExpirationDate = "";
                    Comments = "";
                }
                else
                {
                    var map = MapView.Active.Map;
                    var uicAuth = map.FindStandaloneTables("UICAuthorization").First();
                    var qf = new QueryFilter
                    {
                        WhereClause = $"AuthorizationID = '{authId}'"
                    };

                    using (var cursor = uicAuth.Search(qf))
                    {
                        var hasRow = cursor.MoveNext();
                        using (var row = cursor.Current)
                        {
                            SelectedOid = Convert.ToInt64(row["OBJECTID"]);
                            AuthId = Convert.ToString(row["AuthorizationID"]);
                            AuthType = Convert.ToString(row["AuthorizationType"]);
                            SectorType = Convert.ToString(row["OwnerSectorType"]);
                            StartDate = Convert.ToString(row["StartDate"]);
                            ExpirationDate = Convert.ToString(row["ExpirationDate"]);
                            Comments = Convert.ToString(row["Comments"]);
                        }
                    }
                }
            });

            LoadHash = CalculateFieldHash();
        }

        public bool IsWellAtributesComplete()
        {
            return !string.IsNullOrEmpty(AuthId) &&
                   !string.IsNullOrEmpty(AuthType) &&
                   !string.IsNullOrEmpty(SectorType) &&
                   !string.IsNullOrEmpty(StartDate) &&
                   !string.IsNullOrEmpty(ExpirationDate) &&
                   !string.IsNullOrEmpty(Comments);
        }

        //Events
        public async void ControllingIdChangedHandler(string oldId, string facGuid)
        {
            await AddIdsForFacility(facGuid);
            if (AuthIds.Count == 0)
            {
                //await UpdateUicWell(null);
                SelectedAuthId = string.Empty;
            }
            else
            {
                //await UpdateUicWell(WellIds.First());
                SelectedAuthId = AuthIds.First();
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
                var insp = new ArcGIS.Desktop.Editing.Attributes.Inspector();
                insp.Load(StoreFeature, oidSet);

                insp["AuthorizationID"] = AuthId;
                insp["AuthorizationType"] = AuthType;
                insp["OwnerSectorType"] = SectorType;
                insp["StartDate"] = StartDate;
                insp["ExpirationDate"] = ExpirationDate;
                insp["Comments"] = Comments;

                op.Modify(insp);
                op.Execute();

                Project.Current.SaveEditsAsync();
            });
        }
        public async void AddNew(string facilityGuid, string countyFips)
        {
            await QueuedTask.Run(() =>
            {
                var newGuid = Guid.NewGuid();
                var guidLast7 = newGuid.ToString();
                guidLast7 = guidLast7.Substring(guidLast7.Length - 7);

                var authId = $"UTU{countyFips.Substring(countyFips.Length - 2)}NO{guidLast7}".ToUpper();
                //Create list of oids to update
                var attributes = new Dictionary<string, object>
                {
                    {"Facility_FK", facilityGuid},
                    {"AuthorizationID", authId},
                    {"GUID", newGuid}
                };

                var map = MapView.Active.Map;
                var uicAuth = map.FindStandaloneTables("UICAuthorization").First();
                var createFeatures = new EditOperation
                {
                    Name = "Create Features"
                };

                createFeatures.Create(uicAuth, attributes);
                createFeatures.Execute();

                _facilityAuthIds.Add(authId);
                SelectedAuthId = authId;
            });
        }
    }
}
