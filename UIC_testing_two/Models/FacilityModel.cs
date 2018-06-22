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
using System.ComponentModel.DataAnnotations;
using ArcGIS.Desktop.Core;

namespace UIC_Edit_Workflow
{
    class FacilityModel: ValidatableBindableBase, IWorkTaskModel
    {
        private static readonly FacilityModel _instance = new FacilityModel();
        public const string ID_FIELD = "FacilityID";
        private WellModel _wellModel = WellModel.Instance;
        public event ControllingIdChangeDelegate FacilityChanged;

        private FacilityModel()
        {
        }

        #region properties
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
                        var feature = (FeatureLayer)map.FindLayers("UICFacility").First();
                        return feature as FeatureLayer;
                    }).Result;
                }
                return _storeFeature;
            }
        }
        public static FacilityModel Instance
        {
            get
            {
                return _instance;
            }
        }

        #region tablefields
        private string _uicFacilityId = "";
        [Required]
        public string UicFacilityId
        {
            get
            {
                return _uicFacilityId;
            }

            set
            {
                SetProperty(ref _uicFacilityId, value);
            }
        }

        private string _facilityGuid;
        [Required]
        public string FacilityGuid
        {
            get
            {
                return _facilityGuid;
            }

            set
            {
                SetProperty(ref _facilityGuid, value);
            }
        }

        private string _countyFips;
        [Required]
        public string CountyFips
        {
            get
            {
                return _countyFips;
            }

            set
            {
                SetProperty(ref _countyFips, value);
            }
        }

        private string _naicsPrimary;
        [Required]
        public string NaicsPrimary
        {
            get
            {
                return _naicsPrimary;
            }

            set
            {
                SetProperty(ref _naicsPrimary, value);
            }
        }

        private string _facilityName = "";
        [Required]
        [NameTest]
        public string FacilityName
        {
            get
            {
                return _facilityName;
            }

            set
            {
                SetProperty(ref _facilityName, value);
            }
        }

        private string _facilityAddress;
        [Required]
        public string FacilityAddress
        {
            get
            {
                return _facilityAddress;
            }

            set
            {
                SetProperty(ref _facilityAddress, value);
            }
        }

        private string _facilityCity;
        [Required]
        public string FacilityCity
        {
            get
            {
                return _facilityCity;
            }

            set
            {
                SetProperty(ref _facilityCity, value);
            }
        }

        private string _facilityState;
        [Required]
        public string FacilityState
        {
            get
            {
                return _facilityState;
            }

            set
            {
                SetProperty(ref _facilityState, value);
            }
        }

        private string _facilityZip;
        public string FacilityZip
        {
            get
            {
                return _facilityZip;
            }

            set
            {
                SetProperty(ref _facilityZip, value);
            }
        }

        private string _facilityMilepost;
        public string FacilityMilepost
        {
            get
            {
                return _facilityMilepost;
            }

            set
            {
                SetProperty(ref _facilityMilepost, value);
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
        #endregion
        #endregion
        protected override string fieldValueString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Convert.ToString(UicFacilityId));
            sb.Append(Convert.ToString(FacilityGuid));
            sb.Append(Convert.ToString(CountyFips));
            sb.Append(Convert.ToString(NaicsPrimary));
            sb.Append(Convert.ToString(FacilityName));
            sb.Append(Convert.ToString(FacilityAddress));
            sb.Append(Convert.ToString(FacilityCity));
            sb.Append(Convert.ToString(FacilityState));
            sb.Append(Convert.ToString(FacilityZip));
            sb.Append(Convert.ToString(FacilityMilepost));
            sb.Append(Convert.ToString(Comments));
            return sb.ToString();
        }
  
        public async Task UpdateModel(string facilityId)
        {
            if (this.UicFacilityId != facilityId)
            {
                string oldFacId = this.FacilityGuid;
                await QueuedTask.Run(() =>
                {
                    var map = MapView.Active.Map;
                    QueryFilter qf = new QueryFilter()
                    {
                        WhereClause = string.Format("FacilityID = '{0}'", facilityId)
                    };
                    using (RowCursor cursor = StoreFeature.Search(qf))
                    {
                        bool hasRow = cursor.MoveNext();
                        using (Row row = cursor.Current)
                        {
                            this.SelectedOid = Convert.ToInt64(row["OBJECTID"]);
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
                });
                LoadHash = calculateFieldHash();
                FacilityChanged(oldFacId, this.FacilityGuid);
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

                insp["CountyFIPS"] = this.CountyFips;
                insp["NAICSPrimary"] = this.NaicsPrimary;
                insp["FacilityName"] = this.FacilityName;
                insp["FacilityAddress"] = this.FacilityAddress;
                insp["FacilityCity"] = this.FacilityCity;
                insp["FacilityState"] = this.FacilityState;
                insp["FacilityZip"] = this.FacilityZip;
                insp["FacilityMilePost"] = this.FacilityMilepost;
                insp["Comments"] = this.Comments;

                op.Modify(insp);
                op.Execute();
                Project.Current.SaveEditsAsync();
            });
            return t;
        }

        //Validation
        public bool IsCountyFipsComplete()
        {
            bool isFipsError = GetErrors("CountyFips") == null;
            return !String.IsNullOrEmpty(this.CountyFips) && this.CountyFips.Length == 5 && isFipsError;
        }
        public bool AreAttributesComplete()
        {
            return !this.HasErrors;
        }

        //Events
        public async void ControllingIdChangedHandler(string oldId, string facGuid)
        {
            await UpdateModel(facGuid);
        }
    }
}
