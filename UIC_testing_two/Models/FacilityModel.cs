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

namespace UIC_Edit_Workflow
{
    class FacilityModel: ValidatableBindableBase
    {
        private static readonly FacilityModel _instance = new FacilityModel();
        public const string ID_FIELD = "FacilityID";
        private WellModel _wellModel = WellModel.Instance;
        public event ControllingIdChangeDelegate FacilityChanged;

        private FacilityModel()
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

        private bool _editReady;

        #region properties
        public static FacilityModel Instance
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
                SetProperty(ref _isDirty, value);
            }
        }

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
        [Required]
        public string FacilityName
        {
            get
            {
                return _facilityName;
            }

            set
            {
                SetProperty(ref _facilityName, value);
                IsDirty = true;
            }
        }
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

        public bool EditReady
        {
            get
            {
                return _editReady;
            }

            set
            {
                SetProperty(ref _editReady, value);
            }
        }
        #endregion

        public async Task UpdateModel(string facilityId)
        {
            if (this.UicFacilityId != facilityId)
            {
                string oldFacId = this.FacilityGuid;
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
                    if (this.CountyFips.Length == 5)
                    {
                        this.EditReady = true;
                    }
                    else
                    {
                        this.EditReady = false;
                    }
                    this.IsDirty = false;
                });
                System.Diagnostics.Debug.WriteLine("uicmodel UpdateUicFacility");
                FacilityChanged(oldFacId, this.FacilityGuid);
            }
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

    }
}
