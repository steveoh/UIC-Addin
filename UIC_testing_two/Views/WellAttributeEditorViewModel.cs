using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Windows.Input;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Data;

namespace UIC_Edit_Workflow
{
    internal class WellAttributeEditorViewModel : DockPane
    {
        private FacilityModel _facilityModel = FacilityModel.Instance;
        private WellModel _wellModel = WellModel.Instance;
        private FeatureLayer _wellLayer = null;

        private const string _dockPaneID = "UIC_Edit_Workflow_WellAttributeEditor";

        protected WellAttributeEditorViewModel()
        {

        }

        private RelayCommand _addSelectedWell;
        public ICommand AddWell
        {
            get
            {
                if (_addSelectedWell == null)
                {
                    _addSelectedWell = new RelayCommand(() => AddSelectedWell(), () => { return true; });
                }
                return _addSelectedWell;
            }
        }

        private Task AddSelectedWell()
        {
            Task t = QueuedTask.Run(() =>
            {
                if (_wellLayer == null)
                {
                    var map = MapView.Active.Map;
                    _wellLayer = (FeatureLayer)map.FindLayers("UICWell").First();
                }

                long selectedId;
                var currentselection = _wellLayer.GetSelection();
                using (RowCursor cursor = currentselection.Search())
                {
                    bool hasrow = cursor.MoveNext();
                    using (Row row = cursor.Current)
                    {
                        selectedId = Convert.ToInt64(row["OBJECTID"]);
                    }
                }
                //if (String.IsNullOrWhiteSpace(selectedId))
                //{
                //    if (String.IsNullOrWhiteSpace(selectedId))
                //    {
                //        EmptyFips = true;
                //    }
                //    else
                //    {
                //        //Create Id from fips and global
                //    }
                //}
                _wellModel.AddNew(selectedId, _facilityModel.FacilityGuid, _facilityModel.CountyFips);
            });
            return t;
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
        private string _heading = "My DockPane";
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
    internal class WellAttributeEditor_ShowButton : Button
    {
        protected override void OnClick()
        {
            WellAttributeEditorViewModel.Show();
        }
    }
}
