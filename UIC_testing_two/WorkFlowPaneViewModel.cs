using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using ArcGIS.Desktop.Editing.Events;
using ArcGIS.Desktop.Framework.Dialogs;
using System.ComponentModel;
using ArcGIS.Core.Data;

namespace UIC_Edit_Workflow
{
    internal class WorkFlowPaneViewModel : DockPane
    {
        private const string _dockPaneID = "UIC_Edit_Workflow_WorkFlowPane";
        private ObservableCollection<WorkTask> _workTasks;
        private int selectedUicId;
        private List<IWorkTaskModel> models;
        public FacilityModel uicModel = FacilityModel.Instance;
        public WellModel wellModel = WellModel.Instance;
        public AuthorizationModel authModel =   AuthorizationModel.Instance;

        public WorkFlowPaneViewModel()
        {
            models = new List<IWorkTaskModel>() { wellModel, authModel };
            foreach (IWorkTaskModel model in models)
            {
                uicModel.FacilityChanged += model.FacilityChangeHandler;
                INotifyPropertyChanged propertyModel = (INotifyPropertyChanged)model;
                propertyModel.PropertyChanged += this.CheckTaskItemsOnChange;
            }
            uicModel.PropertyChanged += this.CheckTaskItemsOnChange;
            //wellModel.PropertyChanged += this.CheckTaskItemsOnChange;
            _workTasks = new ObservableCollection<WorkTask>();
            WorkTask uicTaskRoot = new WorkTask("esri_editing_AttributesDockPane") { Title = "UIC Workflow"};
            WorkTask facilityWork = new WorkTask("UIC_Edit_Workflow_AttributeEditor") { Title = "UIC facility"};
            facilityWork.Items.Add(new WorkTask("esri_editing_CreateFeaturesDockPane") { Title = "Create geometry"});
            facilityWork.Items.Add(new WorkTask("UIC_Edit_Workflow_AttributeEditor", uicModel.IsCountyFipsComplete) { Title = "Add county FIPS"});
            facilityWork.Items.Add(new WorkTask("UIC_Edit_Workflow_AttributeEditor") { Title = "Populate attributes"});
            uicTaskRoot.Items.Add(facilityWork);

            WorkTask wellWork = new WorkTask("UIC_Edit_Workflow_WellAttributeEditor") { Title = "UIC Well Point"};
            wellWork.Items.Add(new WorkTask("esri_editing_CreateFeaturesDockPane", () => !wellModel.HasErrors) { Title = "Create geometry" });
            wellWork.Items.Add(new WorkTask("UIC_Edit_Workflow_WellAttributeEditor", wellModel.IsWellAtributesComplete) { Title = "Populate attributes"});
            uicTaskRoot.Items.Add(wellWork);

            _workTasks.Add(uicTaskRoot);
            ModelDirty = false;
        }

        protected BasicFeatureLayer FindFacilities()
        {
            WorkFlowPaneViewModel pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID) as WorkFlowPaneViewModel;
            BasicFeatureLayer facFeature = QueuedTask.Run(() =>
            {
                //if (MapView.Active.GetSelectedLayers().Count == 0)
                //{
                //    MessageBox.Show("Select a feature class from the Content 'Table of Content' first.");
                //    Utils.RunOnUiThread(() => {
                //        var contentsPane = FrameworkApplication.DockPaneManager.Find("esri_core_contentsDockPane");
                //        contentsPane.Activate();
                //    });
                //    return;
                //}
                FeatureLayer uicWells = (FeatureLayer)MapView.Active.Map.FindLayers("UICFacility").First();
                var layerTable = uicWells.GetTable();
                return uicWells as BasicFeatureLayer;
            }).Result;
            return facFeature;
        }

        //  behavior test method
        public void ChangeStuff()
        {
            uicModel.FacilityName = "Hello there";
        }

        private bool _emptyFips = false;
        public bool EmptyFips
        {
            get
            {
                return _emptyFips;
            }

            set
            {
                SetProperty(ref _emptyFips, value, () => EmptyFips);
            }
        }
        private bool _modelDirty;
        public bool ModelDirty
        {
            get
            {
                return _modelDirty;
            }

            set
            {
                SetProperty(ref _modelDirty, value, () => ModelDirty);
            }
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
        internal static void subRowEvent()
        {
            WorkFlowPaneViewModel pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID) as WorkFlowPaneViewModel;
            QueuedTask.Run(() =>
            {
                if (MapView.Active.GetSelectedLayers().Count == 0)
                {
                    MessageBox.Show("Select a feature class from the Content 'Table of Content' first.");
                    Utils.RunOnUiThread(() => {
                        var contentsPane = FrameworkApplication.DockPaneManager.Find("esri_core_contentsDockPane");
                        contentsPane.Activate();
                    });
                    return;
                }
                //Listen for row events on a layer
                var featLayer = MapView.Active.GetSelectedLayers().First() as FeatureLayer;
                var layerTable = featLayer.GetTable();
                pane.SelectedLayer = featLayer as BasicFeatureLayer;

                //subscribe to row events
                var rowCreateToken = RowCreatedEvent.Subscribe(pane.onCreatedRowEvent, layerTable);
                var rowChangedToken = RowChangedEvent.Subscribe(pane.onRowChangedEvent, layerTable);
            });
        }
        protected void onCreatedRowEvent(RowChangedEventArgs args)
        {

            Utils.RunOnUiThread(() => {
                WorkFlowPaneViewModel.Show();
                var pane = FrameworkApplication.DockPaneManager.Find("esri_editing_AttributesDockPane");
                pane.Activate();
            });
        }
        protected void onRowChangedEvent(RowChangedEventArgs args)
        {
            var row = args.Row;
            //UpdateModel(Convert.ToString(row["FacilityID"]));
        }
        public void CheckTaskItemsOnChange (object model, PropertyChangedEventArgs propertyInfo)
        {
            CheckTaskItems(TableTasks[0]);
        }
        private void CheckTaskItems(WorkTask workTask)
        {
            foreach (WorkTask item in workTask.Items)
            {
                CheckTaskItems(item);
            }
            workTask.CheckForCompletion();
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Workflow Progress";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        public ObservableCollection<WorkTask> TableTasks
        {
            get { return _workTasks; }
        }

        public void showPaneTest(string paneId)
        {
            Utils.RunOnUiThread(() =>
            {
                var pane = FrameworkApplication.DockPaneManager.Find(paneId);
                pane.Activate();
            });

        }

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
                //SetProperty(ref _selectedFips, value, () => SelectedFips);
            }
        }
        private string _selectedFips;
        public string SelectedFips
        {
            get
            {
                return _selectedFips;
            }

            set
            {
                SetProperty(ref _selectedFips, value, () => SelectedFips);
                int fips;
                if (_selectedFips.Length == 5 && int.TryParse(_selectedFips, out fips))
                {
                    EmptyFips = false;
                }
            }
        }
        private string _uicSelection = "";
        public string UicSelection
        {
            get { return _uicSelection; }
            set
            {
                SetProperty(ref _uicSelection, value, () => UicSelection);
                //clear model if not dirty
                if (_uicSelection.Length > 6 && _uicSelection.Length < 14)
                {
                    checkForSugestion(value);
                }
                else if (_uicSelection.Length == 14)
                {
                    UicSuggestion = null;
                    System.Diagnostics.Debug.WriteLine("Work flow selection id set");
                    UpdateModel(value);
                    //wellTask.Title = String.Format("Wells Wells {0}", wellModel.WellIds.Count());
                }
                
            }
        }
        private string _uicSuggestion = "";
        public string UicSuggestion
        {
            get { return _uicSuggestion; }
            set
            {
                SetProperty(ref _uicSuggestion, value, () => UicSuggestion);
            }
        }
        private async void checkForSugestion (string partialId)
        {
            await QueuedTask.Run(() =>
            {
                string suggestedId = "";
                int rowCount = 0;
                var map = MapView.Active.Map;
                FeatureLayer uicWells = (FeatureLayer)map.FindLayers("UICFacility").First();
                QueryFilter qf = new QueryFilter()
                {
                    WhereClause = string.Format("FacilityID LIKE '{0}%'", partialId)
                };
                using (RowCursor cursor = uicWells.Search(qf))
                {
                    while (cursor.MoveNext())
                    {
                        using (Row row = cursor.Current)
                        {
                            suggestedId = Convert.ToString(row["FacilityID"]);
                            rowCount++;
                            if (rowCount > 1)
                            {
                                UicSuggestion = null;
                                return;
                            }
                        }
                    }
                }
                UicSuggestion = suggestedId;
            });
        }
        private BasicFeatureLayer _selectedLayer;
        public BasicFeatureLayer SelectedLayer
        {
            get
            {
                if (_selectedLayer == null)
                {
                    _selectedLayer = FindFacilities();
                }
                return _selectedLayer;
            }
            set
            {
                SetProperty(ref _selectedLayer, value, () => SelectedLayer);
            }
        }

        private RelayCommand _useFacilitySuggestion;
        public ICommand UseFacilitySuggestion
        {
            get
            {
                if (_useFacilitySuggestion == null)
                {
                    _useFacilitySuggestion = new RelayCommand(() => { UicSelection = UicSuggestion; },
                                                              () => { return UicSuggestion != null || UicSuggestion != String.Empty; });
                }
                return _useFacilitySuggestion;
            }
        }

        private RelayCommand _assignIdCmd;
        public ICommand AssignId
        {
            get
            {
                if (_assignIdCmd == null)
                {
                    _assignIdCmd = new RelayCommand(() => AssignFacilityId(), () => { return MapView.Active != null; });
                }
                return _assignIdCmd;
            }
        }

        private RelayCommand _getSelectionCmd;
        public ICommand GetSelectedFacility
        {
            get
            {
                if (_getSelectionCmd == null)
                {
                    _getSelectionCmd = new RelayCommand(() => GetSelectedFeature(), () => { return MapView.Active != null; });
                }
                return _getSelectionCmd;
            }
        }

        private RelayCommand _newSelectionCmd;
        public ICommand SelectUicAndZoom
        {
            get
            {
                if (_newSelectionCmd == null)
                {
                    _newSelectionCmd = new RelayCommand(() => ModifyLayerSelection(SelectionCombinationMethod.New), () => { return MapView.Active != null && SelectedLayer != null && UicSelection != ""; });
                }
                return _newSelectionCmd;
            }
        }

        private Task GetSelectedFeature()
        {
            Task t = QueuedTask.Run(() =>
            {
                ModelDirty = true;
                string selectedId;
                string countyFips;
                var currentSelection = SelectedLayer.GetSelection();
                using (RowCursor cursor = currentSelection.Search())
                {
                    bool hasRow = cursor.MoveNext();
                    using (Row row = cursor.Current)
                    {
                        selectedId = Convert.ToString(row[FacilityModel.ID_FIELD]);
                        countyFips = Convert.ToString(row["CountyFIPS"]);
                        SelectedOid = Convert.ToInt64(row["OBJECTID"]);
                    }
                }
                if (String.IsNullOrWhiteSpace(selectedId))
                {
                    if (String.IsNullOrWhiteSpace(countyFips))
                    {
                        EmptyFips = true;
                    }
                    else
                    {
                        //Create Id from fips and global
                    }
                }
 
                UicSelection = selectedId;
                SelectedFips = countyFips;

            });
            return t;
        }
        private Task ModifyLayerSelection(SelectionCombinationMethod method)
        {
            Task t = QueuedTask.Run(() =>
            {
                if (MapView.Active == null || SelectedLayer == null || UicSelection == null)
                    return;

                //if (!ValidateExpresion(false))
                //    return;
                string wc = string.Format("FacilityID = \"{0}\"", UicSelection);
                SelectedLayer.Select(new QueryFilter() { WhereClause = wc }, method);
                MapView.Active.ZoomToSelected();
            });
            return t;
        }
        private Task AssignFacilityId()
        {
            Task t = QueuedTask.Run(() =>
            {
                //Create list of oids to update
                var oidSet = new List<long>() { SelectedOid };
                //Create edit operation and update
                var op = new ArcGIS.Desktop.Editing.EditOperation();
                op.Name = "Update date";
                var insp = new ArcGIS.Desktop.Editing.Attributes.Inspector();
                insp.Load(SelectedLayer, oidSet);

                long fips;
                long.TryParse(SelectedFips, out fips);
                insp["CountyFIPS"] = fips;
                Guid newGuid = Guid.NewGuid();
                string guidLast8 = newGuid.ToString();
                guidLast8 = guidLast8.Substring(guidLast8.Length - 8);
                insp["GUID"] = newGuid;
                string newFacilityId = String.Format("UTU{0}F{1}", SelectedFips.Substring(SelectedFips.Length - 2), guidLast8).ToUpper();
                insp[FacilityModel.ID_FIELD] = newFacilityId;

                op.Modify(insp);
                op.Execute();
                UicSelection = newFacilityId;
            });
            return t;
        }

        private async void UpdateModel(string uicId)
        {
            await uicModel.UpdateModel(uicId);
            this.CheckTaskItems(TableTasks[0]);
        }

    }


    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class WorkFlowPane_ShowButton : Button
    {
        protected override void OnClick()
        {
            WorkFlowPaneViewModel.subRowEvent();
            WorkFlowPaneViewModel.Show();
        }
    }

    public delegate bool IsTaskCompelted();
    class WorkTask : BindableBase
    {
        public WorkTask(string activePanel, IsTaskCompelted completeCheck = null)
        {
            this.Items = new ObservableCollection<WorkTask>();
            this.ActivePanel = activePanel;
            if (completeCheck != null)
                this.IsComplete += completeCheck;
            else
                this.IsComplete += this.AreChildrenComplete;

            this.Complete = false;
        }

        public IsTaskCompelted IsComplete;
        private string _title; 
        public string Title {
            get
            {
                return _title;
            }
            set
            {
                SetProperty(ref _title, value);
            }
        }
        private bool _complete;
        public bool Complete {
            get
            {
                return _complete;
            }
            set
            {
                SetProperty(ref _complete, value);
            }
        }
        public string ActivePanel { get; set; }
        //public IsTaskCompelted IsCompelete { get; set; }

        bool _isExpanded;
        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                SetProperty(ref _isExpanded, value);
                // Expand all the way up to the root.
                //if (_isExpanded && _parent != null)
                //    _parent.IsExpanded = true;
            }
        }

        bool _isSelected;
        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                SetProperty(ref _isSelected, value);
                if (_isSelected)
                {
                    try
                    {
                        QueuedTask.Run(() => {
                            Utils.RunOnUiThread(() =>
                            {
                                var pane = FrameworkApplication.DockPaneManager.Find(ActivePanel);
                                pane.Activate();
                            });
                        });

                    }
                    catch (Exception)
                    {

                        throw;
                    }

                }
            }
        }

        public bool CheckForCompletion()
        {
            bool complete = this.IsComplete();
            if (complete)
            {
                this.Complete = true;
            }
            else
            {
                this.Complete = false;
            }
            return complete;
        }

        public bool AreChildrenComplete()
        {
            bool complete = true;
            foreach (WorkTask child in this.Items)
            {
                if (!child.Complete)
                {
                    return false;
                }
            }
            return complete;
        }

        public ObservableCollection<WorkTask> Items { get; set; }
    }
}
