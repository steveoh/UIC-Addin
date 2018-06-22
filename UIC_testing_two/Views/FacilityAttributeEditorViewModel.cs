using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.ComponentModel;

namespace UIC_Edit_Workflow
{
    internal class FacilityAttributeEditorViewModel : DockPane
    {
        private const string _dockPaneID = "UIC_Edit_Workflow_FacilityAttributeEditor";
        private FacilityModel _facilityModel = FacilityModel.Instance;
        private FacilityInspectionModel _inspectionModel = FacilityInspectionModel.Instance;
        protected FacilityAttributeEditorViewModel()
        {
        }

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

        private RelayCommand _addNewInspection;
        public ICommand AddInspectionRecord
        {
            get
            {
                if (_addNewInspection == null)
                {
                    _addNewInspection = new RelayCommand(() => AddNewInspection(), () => { return true; });
                }
                return _addNewInspection;
            }
        }
        private void AddNewInspection()
        {
            string facGuid = _facilityModel.FacilityGuid;

            _inspectionModel.AddNew(facGuid);
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class AttributeEditor_ShowButton : Button
    {
        protected override void OnClick()
        {
            FacilityAttributeEditorViewModel.Show();
        }
    }
}
