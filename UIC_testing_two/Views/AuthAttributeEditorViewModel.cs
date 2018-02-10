using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Windows.Input;

namespace UIC_Edit_Workflow
{
    internal class AuthAttributeEditorViewModel : DockPane
    {
        private const string _dockPaneID = "UIC_Edit_Workflow_AuthAttributeEditor";
        private FacilityModel _facilityModel = FacilityModel.Instance;
        private AuthorizationModel _authModel = AuthorizationModel.Instance;

        protected AuthAttributeEditorViewModel() { }

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
        private string _heading = "Authorization";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        private RelayCommand _addNewRecord;
        public ICommand AddRecord
        {
            get
            {
                if (_addNewRecord == null)
                {
                    _addNewRecord = new RelayCommand(() => AddNewRecord(), () => { return true; });
                }
                return _addNewRecord;
            }
        }
        private void AddNewRecord()
        {
            string facGuid = _facilityModel.FacilityGuid;
            string facFips = _facilityModel.CountyFips;

            _authModel.AddNew(facGuid, facFips);
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class AuthAttributeEditor_ShowButton : Button
    {
        protected override void OnClick()
        {
            AuthAttributeEditorViewModel.Show();
        }
    }
}
