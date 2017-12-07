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

namespace UIC_testing_two
{
    internal class WorkFlowPaneViewModel : DockPane
    {
        private const string _dockPaneID = "UIC_testing_two_WorkFlowPane";
        private ObservableCollection<WorkTask> _tableTasks;
        private int selectedUicId;
        public UICModel uicModel = UICModel.Instance;

        public WorkFlowPaneViewModel()
        {
            _tableTasks = new ObservableCollection<WorkTask>();
            WorkTask root = new WorkTask("esri_editing_AttributesDockPane") { Title = "UIC Workflow" };
            WorkTask childItem1 = new WorkTask("UIC_testing_two_AttributeEditor") { Title = "UIC facility" };
            childItem1.Items.Add(new WorkTask("esri_editing_CreateFeaturesDockPane") { Title = "Create geometry", IsCompelete = () => false });
            childItem1.Items.Add(new WorkTask("UIC_testing_two_AttributeEditor") { Title = "Add county FIPS", IsCompelete = uicModel.IsCountyFipsComplete });
            childItem1.Items.Add(new WorkTask("UIC_testing_two_AttributeEditor") { Title = "Populate attributes", IsCompelete = () => false });
            root.Items.Add(childItem1);
            WorkTask childItem2 = new WorkTask("UIC_testing_two_WellAttributeEditor") { Title = "UIC Well Point" };
            childItem2.Items.Add(new WorkTask("esri_editing_CreateFeaturesDockPane") { Title = "Create geometry", IsCompelete = () => false });
            childItem2.Items.Add(new WorkTask("UIC_testing_two_WellAttributeEditor") { Title = "Populate attributes", IsCompelete = uicModel.IsWellAtributesComplete });
            root.Items.Add(childItem2);
            //root.Items.Add(new WorkTask("esri_editing_CreateFeaturesDockPane") { Title = "Next one", Complete = true });
            _tableTasks.Add(root);
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
            UpdateModel(Convert.ToString(row["FacilityID"]));
        }
        private void CheckTaskItems(WorkTask workTask)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("CheckTaskItems: title: {0}, complete: {1}",
                                                             workTask.Title,
                                                             workTask.IsCompelete().ToString()));

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
            get { return _tableTasks; }
        }

        public void showPaneTest(string paneId)
        {
            Utils.RunOnUiThread(() =>
            {
                var pane = FrameworkApplication.DockPaneManager.Find(paneId);
                pane.Activate();
            });

        }
        private string _uicSelection = "";
        public string UicSelection
        {
            get { return _uicSelection; }
            set
            {
                SetProperty(ref _uicSelection, value, () => UicSelection);
                if (_uicSelection.Length > 6 && _uicSelection.Length < 14)
                {
                    System.Diagnostics.Debug.WriteLine(_uicSelection);
                    checkForSugestion(value);
                }
                else if (_uicSelection.Length == 14)
                {
                    UicSuggestion = null;
                    UpdateModel(value);
                    //uicModel.UpdateUicFacility(_uicSelection);
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
                System.Diagnostics.Debug.WriteLine(partialId);
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
                //var map = MapView.Active.Map;
                //FeatureLayer uicFacilities = (FeatureLayer)map.FindLayers("UICFacility").First();
                var currentSelection = SelectedLayer.GetSelection();
                using (RowCursor cursor = currentSelection.Search())
                {
                    bool hasRow = cursor.MoveNext();
                    using (Row row = cursor.Current)
                    {
                        System.Diagnostics.Debug.WriteLine(Convert.ToString(row["FacilityName"]));
                        selectedId = Convert.ToString(row["FacilityID"]);
                    }
                }
                UicSelection = selectedId;
            });
            return t;
        }
        private Task ModifyLayerSelection(SelectionCombinationMethod method)
        {
            Task t = QueuedTask.Run(() =>
            {
                System.Diagnostics.Debug.WriteLine("Mod Layer Selection");
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

        private async void UpdateModel(string uicId)
        {
            await uicModel.UpdateUicFacility(uicId);
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
    public class WorkTask : INotifyPropertyChanged
    {
        public WorkTask(string activePanel)
        {
            this.Items = new ObservableCollection<WorkTask>();
            this.ActivePanel = activePanel;
            this.IsCompelete = this.AreChildrenComplete;

            this.StatusColor = "Red";
            this.Complete = false;
        }

        public string Title { get; set; }
        public string StatusColor { get; set; }
        public bool Complete { get; set; }
        public string ActivePanel { get; set; }
        public IsTaskCompelted IsCompelete { get; set; }

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
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }

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
                if (value != _isSelected)
                {
                    _isSelected = value;
                    if (_isSelected)
                    {
                        try
                        {

                            //var pane = FrameworkApplication.DockPaneManager.Find("ProAppModule1_Dockpane1") as Dockpane1ViewModel;
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
                    this.OnPropertyChanged("IsSelected");

                }
            }
        }

        public bool CheckForCompletion()
        {
            bool complete = this.IsCompelete();
            System.Diagnostics.Debug.WriteLine(String.Format("CheckForCompletion: {0}",
                                               complete.ToString()));
            if (complete)
            {
                this.StatusColor = "Green";
                this.Complete = true;
            }
            else
            {
                this.Complete = false;
                this.StatusColor = "Black";
            }
            this.OnPropertyChanged("Complete");
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

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                System.Diagnostics.Debug.WriteLine(Title);
                System.Diagnostics.Debug.WriteLine(IsSelected);
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion // INotifyPropertyChanged Members

        public ObservableCollection<WorkTask> Items { get; set; }
    }
}
