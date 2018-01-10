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
    class AuthorizationModel : ValidatableBindableBase, IWorkTaskModel
    {
        private readonly object lockCollection = new object();
        private static readonly AuthorizationModel instance = new AuthorizationModel();
        //private UICModel uicModel = null;
        private bool _isDirty;

        private AuthorizationModel()
        {
            //uicModel = UICModel.Instance;
            //uicModel.FacilityChanged = new FacilityChangeDelegate(facChangeHandler);
            readOnlyAuthIds = new ReadOnlyObservableCollection<string>(_facilityAuthIds);
            Utils.RunOnUiThread(() =>
            {
                BindingOperations.EnableCollectionSynchronization(readOnlyAuthIds, lockCollection);
            });
            _isDirty = false;

        }

        private string _authId;
        private string _authType;
        private string _sectorType;
        private string _startDate;
        private string _expirationDate;
        private string _comments;

        private string _createdOn;
        private string _modifiedOn;
        private string _editedBy;


        private string _selectedAuthId;

        private readonly ObservableCollection<string> _facilityAuthIds = new ObservableCollection<string>();
        private readonly ReadOnlyObservableCollection<string> readOnlyAuthIds;

        #region properties
        #region tablefields
        [Required]
        public string AuthId
        {
            get
            {
                return _authId;
            }

            set
            {
                SetProperty(ref _authId, value);
            }
        }

        [Required]
        [UicValidations]
        public string AuthType
        {
            get
            {
                return _authType;
            }

            set
            {
                SetProperty(ref _authType, value);
                _isDirty = true;
            }
        }

        [Required]
        public string SectorType
        {
            get
            {
                return _sectorType;
            }

            set
            {
                SetProperty(ref _sectorType, value);
            }
        }

        [Required]
        public string StartDate
        {
            get
            {
                return _startDate;
            }

            set
            {
                SetProperty(ref _startDate, value);
            }
        }

        [Required]
        public string ExpirationDate
        {
            get
            {
                return _expirationDate;
            }

            set
            {
                SetProperty(ref _expirationDate, value);
            }
        }

        [Required]
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

        public ReadOnlyObservableCollection<string> AuthIds => readOnlyAuthIds;

        public string SelectedAuthId
        {
            get
            {
                return _selectedAuthId;
            }

            set
            {
                SetProperty(ref _selectedAuthId, value);
                if (_selectedAuthId != null)
                    UpdateModel(_selectedAuthId);
                //if (selectedWellId != value)
                //{
                //    selectedWellId = value;

                //   OnPropertyChanged();
                //}
            }
        }

        public static AuthorizationModel Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion

        public async Task AddIdsForFacility(string facilityId)
        {
            await QueuedTask.Run(() =>
            {
                _facilityAuthIds.Clear();
                var map = MapView.Active.Map;
                StandaloneTable uicWells = (StandaloneTable)map.FindStandaloneTables("UICAuthorization").First();
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

                if (authId == null || authId == String.Empty)
                {
                    this.AuthId = "";
                    this.AuthType = "";
                    this.SectorType = "";
                    this.StartDate = "";
                    this.ExpirationDate = "";
                    this.Comments = "";
                }
                else
                {
                    var map = MapView.Active.Map;
                    StandaloneTable uicAuth = (StandaloneTable)map.FindStandaloneTables("UICAuthorization").First();
                    QueryFilter qf = new QueryFilter()
                    {
                        WhereClause = string.Format("AuthorizationID = '{0}'", authId)
                    };
                    using (RowCursor cursor = uicAuth.Search(qf))
                    {
                        bool hasRow = cursor.MoveNext();
                        using (Row row = cursor.Current)
                        {
                            this.AuthId = Convert.ToString(row["AuthorizationID"]);
                            this.AuthType = Convert.ToString(row["AuthorizationType"]);
                            this.SectorType = Convert.ToString(row["OwnerSectorType"]);
                            this.StartDate = Convert.ToString(row["StartDate"]);
                            this.ExpirationDate = Convert.ToString(row["ExpirationDate"]);
                            this.Comments = Convert.ToString(row["Comments"]);
                        }
                    }
                }
            });
        }

        public bool IsWellAtributesComplete()
        {
            return !String.IsNullOrEmpty(this.AuthId) &&
                   !String.IsNullOrEmpty(this.AuthType) &&
                   !String.IsNullOrEmpty(this.SectorType) &&
                   !String.IsNullOrEmpty(this.StartDate) &&
                   !String.IsNullOrEmpty(this.ExpirationDate) &&
                   !String.IsNullOrEmpty(this.Comments);
        }

        //Events
        public async void FacilityChangeHandler(string oldId, string newId, string facGuid)
        {
            System.Diagnostics.Debug.WriteLine($"Old id {oldId}, New Id {newId}");
            await AddIdsForFacility(facGuid);
            if (AuthIds.Count == 0)
            {
                //await UpdateUicWell(null);
                SelectedAuthId = String.Empty;
            }
            else
            {
                //await UpdateUicWell(WellIds.First());
                SelectedAuthId = AuthIds.First();
            }

        }
    }
}
