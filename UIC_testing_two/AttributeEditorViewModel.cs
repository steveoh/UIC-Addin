using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.ComponentModel;

namespace UIC_testing_two
{
    internal class AttributeEditorViewModel : DockPane
    {
        private const string _dockPaneID = "UIC_testing_two_AttributeEditor";
        private UICModel uicModel = UICModel.Instance;
        protected AttributeEditorViewModel()
        {
            //_countyFips = uicModel.CountyFips;
            //_naicsPrimary = uicModel.NaicsPrimary;
            //_facilityName = uicModel.FacilityName;
            //_facilityAddress = uicModel.FacilityAddress;
            //_facilityCity = uicModel.FacilityCity;
            //_facilityState = uicModel.FacilityState;
            //_facilityZip = uicModel.FacilityZip;
            //_facilityMilepost = uicModel.FacilityMilepost;
            //_comments = uicModel.Comments;
            //_facilityId = uicModel.UicFacilityId;
        }


        //private string _countyFips;
        //public string CountyFips
        //{
        //    get { return _countyFips; }
        //    set
        //    {
        //        SetProperty(ref _countyFips, value, () => CountyFips);
        //        uicModel.CountyFips = _countyFips;
        //    }
        //}
        //private string _naicsPrimary;
        //public string NaicsPrimary
        //{
        //    get { return _naicsPrimary; }
        //    set
        //    {
        //        SetProperty(ref _naicsPrimary, value, () => NaicsPrimary);
        //        uicModel.NaicsPrimary = _naicsPrimary;
        //    }
        //}
        //private string _facilityName;
        //public string FacilityName
        //{
        //    get { return _facilityName; }
        //    set
        //    {
        //        SetProperty(ref _facilityName, value, () => FacilityName);
        //        uicModel.FacilityName = _facilityName;
        //    }
        //}
        //private string _facilityAddress;
        //public string FacilityAddress
        //{
        //    get { return _facilityAddress; }
        //    set
        //    {
        //        SetProperty(ref _facilityAddress, value, () => FacilityAddress);
        //        uicModel.FacilityAddress = _facilityAddress;
        //    }
        //}
        //private string _facilityCity;
        //public string FacilityCity
        //{
        //    get { return _facilityCity; }
        //    set
        //    {
        //        SetProperty(ref _facilityCity, value, () => FacilityCity);
        //        uicModel.FacilityCity = _facilityCity;
        //    }
        //}
        //private string _facilityState;
        //public string FacilityState
        //{
        //    get { return _facilityState; }
        //    set
        //    {
        //        SetProperty(ref _facilityState, value, () => FacilityState);
        //        uicModel.FacilityState = _facilityState;
        //    }
        //}
        //private string _facilityZip;
        //public string FacilityZip
        //{
        //    get { return _facilityZip; }
        //    set
        //    {
        //        SetProperty(ref _facilityZip, value, () => FacilityZip);
        //        uicModel.FacilityZip = _facilityZip;
        //    }
        //}
        //private string _facilityMilepost;
        //public string FacilityMilepost
        //{
        //    get { return _facilityMilepost; }
        //    set
        //    {
        //        SetProperty(ref _facilityMilepost, value, () => FacilityMilepost);
        //        uicModel.FacilityMilepost = _facilityMilepost;
        //    }
        //}
        //private string _comments;
        //public string Comments
        //{
        //    get { return _comments; }
        //    set
        //    {
        //        SetProperty(ref _comments, value, () => Comments);
        //        uicModel.Comments = _comments;
        //    }
        //}
        //private string _facilityId;
        //public string FacilityId
        //{
        //    get { return _facilityId; }
        //    set
        //    {
        //        SetProperty(ref _facilityId, value, () => FacilityId);
        //        uicModel.UicFacilityId = _facilityId;
        //    }
        //}

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Facility Attributes";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class AttributeEditor_ShowButton : Button
    {
        protected override void OnClick()
        {
            AttributeEditorViewModel.Show();
        }
    }
}
