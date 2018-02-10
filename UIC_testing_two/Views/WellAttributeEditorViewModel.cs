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
using ArcGIS.Desktop.Mapping.Events;

namespace UIC_Edit_Workflow
{
    internal class WellAttributeEditorViewModel : DockPane
    {
        private FacilityModel _facilityModel = FacilityModel.Instance;
        private WellModel _wellModel = WellModel.Instance;
        private WellInspectionModel _inspectionModel = WellInspectionModel.Instance;
        private FeatureLayer _wellLayer = null;

        private const string _dockPaneID = "UIC_Edit_Workflow_WellAttributeEditor";

        protected WellAttributeEditorViewModel()
        {
            MapSelectionChangedEvent.Subscribe(OnSelectionChanged);
        }

        private bool _newWellSelected = false;
        public bool NewWellSelected
        {
            get
            {
                return _newWellSelected;
            }

            set
            {
                SetProperty(ref _newWellSelected, value, () => NewWellSelected);
            }
        }

        private string _newWellClass;
        public string NewWellClass
        {
            get
            {
                return _newWellClass;
            }

            set
            {
                SetProperty(ref _newWellClass, value, () => NewWellClass);
            }
        }

        private RelayCommand _addSelectedWell;
        public ICommand AddWell
        {
            get
            {
                if (_addSelectedWell == null)
                {
                    _addSelectedWell = new RelayCommand(() => AddSelectedWell(), () => { return !String.IsNullOrWhiteSpace(NewWellClass); });
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


        private async void OnSelectionChanged(MapSelectionChangedEventArgs mse)
        {
            foreach (var kvp in mse.Selection)
            {
                if ((kvp.Key as BasicFeatureLayer) == null || kvp.Key.Name != "UICWell")
                    continue;
                BasicFeatureLayer selectedLayer = (BasicFeatureLayer)kvp.Key;
                //Is a feature selected? Is it an unassigned well feature?
                if (kvp.Value.Count > 0 && await IsUnassignedWell(selectedLayer))
                {
                    NewWellSelected = true;
                }
                else
                {
                    NewWellSelected = false;
                }
            }
         }

        public static Task<bool> IsUnassignedWell(BasicFeatureLayer selectedLayer)
        {
            return QueuedTask.Run(() => {
                bool noFacilityFk;
                bool noWellClass;
                var currentSelection = selectedLayer.GetSelection();
                using (RowCursor cursor = currentSelection.Search())
                {
                    bool hasrow = cursor.MoveNext();
                    using (Row row = cursor.Current)
                    {
                        noFacilityFk = String.IsNullOrWhiteSpace(Convert.ToString(row["Facility_FK"]));
                        noWellClass = String.IsNullOrWhiteSpace(Convert.ToString(row["WellClass"]));
                    }
                }
                return noFacilityFk && noWellClass;
            });
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
            string wellGuid = _wellModel.WellGuid;

            _inspectionModel.AddNew(wellGuid);
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
