﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing.Events;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using UIC_Edit_Workflow.Models;

namespace UIC_Edit_Workflow
{
    internal class WorkFlowPaneViewModel : DockPane
    {
        private const string DockPaneId = "UIC_Edit_Workflow_WorkFlowPane";

        private readonly List<BindableBase> _allModels;

        private RelayCommand _assignIdCmd;

        private bool _emptyFips;

        private RelayCommand _getSelectionCmd;

        /// <summary>
        ///     Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Workflow Progress";

        private bool _modelDirty;

        private RelayCommand _newSelectionCmd;

        private RelayCommand _saveModelsCmd;

        private string _selectedFips;
        private BasicFeatureLayer _selectedLayer;
        private string _uicSelection = "";
        private string _uicSuggestion = "";

        private RelayCommand _useFacilitySuggestion;
        public AuthorizationModel AuthModel = Module1.AuthorizationModel;
        public FacilityInspectionModel FacilityInspectionModel = Module1.FacilityInspectionModel;
        public FacilityModel FacilityModel = Module1.FacilityModel;
        public WellInspectionModel WellInspectionModel = Module1.WellInspectionModel;
        public WellModel WellModel = Module1.WellModel;

        public WorkFlowPaneViewModel()
        {
            _allModels = new List<BindableBase>();
            var facilityControlledModels = new List<IWorkTaskModel>
            {
                WellModel,
                AuthModel,
                FacilityInspectionModel
            };

            foreach (var model in facilityControlledModels)
            {
                FacilityModel.FacilityChanged += model.ControllingIdChangedHandler;

                var propertyModel = (INotifyPropertyChanged) model;
                propertyModel.PropertyChanged += CheckTaskItemsOnChange;

                _allModels.Add((BindableBase) model);
            }

            var wellControlledModels = new List<IWorkTaskModel>
            {
                WellInspectionModel
            };

            foreach (var model in wellControlledModels)
            {
                WellModel.WellChanged += model.ControllingIdChangedHandler;

                var propertyModel = (INotifyPropertyChanged) model;
                propertyModel.PropertyChanged += CheckTaskItemsOnChange;

                _allModels.Add((BindableBase) model);
            }
            FacilityModel.PropertyChanged += CheckTaskItemsOnChange;
            _allModels.Add(FacilityModel);

            TableTasks = new ObservableCollection<WorkTask>();
            var uicTaskRoot = new WorkTask("esri_editing_AttributesDockPane") {Title = "UIC Facility Workflow"};

            // Facility Task
            var facilityWork = new WorkTask("UIC_Edit_Workflow_FacilityAttributeEditor") {Title = "Facility"};
            facilityWork.Items.Add(new WorkTask("esri_editing_CreateFeaturesDockPane") {Title = "Add New Geometry"});
            facilityWork.Items.Add(new WorkTask("UIC_Edit_Workflow_FacilityAttributeEditor",
                                                FacilityModel.IsCountyFipsComplete) {Title = "Add county FIPS"});
            facilityWork.Items.Add(new WorkTask("UIC_Edit_Workflow_FacilityAttributeEditor",
                                                FacilityModel.AreAttributesComplete) {Title = "Populate attributes"});

            // Facility Task: Facility Inspection 
            var facilityInspectionWork =
                new WorkTask("UIC_Edit_Workflow_FacilityAttributeEditor") {Title = "Inspection"};
            facilityInspectionWork.Items.Add(new WorkTask("UIC_Edit_Workflow_FacilityAttributeEditor",
                                                          FacilityInspectionModel.IsInspectionAttributesComplete)
            {
                Title = "Populate attributes"
            });
            facilityWork.Items.Add(facilityInspectionWork);
            uicTaskRoot.Items.Add(facilityWork); // Add task to root task

            // Well Task
            var wellWork = new WorkTask("UIC_Edit_Workflow_WellAttributeEditor") {Title = "Wells"};
            wellWork.Items.Add(new WorkTask("esri_editing_CreateFeaturesDockPane") {Title = "Add New Geometry"});
            wellWork.Items.Add(new WorkTask("UIC_Edit_Workflow_WellAttributeEditor", WellModel.IsWellNameCorrect)
            {
                Title = "Well Name correct"
            });
            wellWork.Items.Add(new WorkTask("UIC_Edit_Workflow_WellAttributeEditor",
                                            WellModel.IsWellAttributesComplete) {Title = "Populate attributes"});

            // Well Task: Well Inspection
            var wellInspectionWork = new WorkTask("UIC_Edit_Workflow_WellAttributeEditor") {Title = "Inspection"};
            wellInspectionWork.Items.Add(new WorkTask("UIC_Edit_Workflow_WellAttributeEditor",
                                                      WellInspectionModel.IsInspectionAttributesComplete)
            {
                Title = "Populate attributes"
            });
            wellWork.Items.Add(wellInspectionWork);
            uicTaskRoot.Items.Add(wellWork); // Add task to root task

            // Authorization Task
            var authWork = new WorkTask("UIC_Edit_Workflow_AuthAttributeEditor") {Title = "Authorizations"};
            authWork.Items.Add(new WorkTask("UIC_Edit_Workflow_AuthAttributeEditor", () => true)
            {
                Title = "Populate attributes"
            });
            uicTaskRoot.Items.Add(authWork); // Add task to root task

            // Add root task to work tasks
            TableTasks.Add(uicTaskRoot);

            //Init the dockpanes in the framework. Loads data before and refernces.
            FrameworkApplication.DockPaneManager.Find("UIC_Edit_Workflow_WellAttributeEditor");
            FrameworkApplication.DockPaneManager.Find("UIC_Edit_Workflow_FacilityAttributeEditor");
            FrameworkApplication.DockPaneManager.Find("UIC_Edit_Workflow_AuthAttributeEditor");

            AreModelsDirty = false;
        }

        public bool EmptyFips
        {
            get => _emptyFips;

            set { SetProperty(ref _emptyFips, value, () => EmptyFips); }
        }

        public bool AreModelsDirty
        {
            get => _modelDirty;

            set { SetProperty(ref _modelDirty, value, () => AreModelsDirty); }
        }

        public string Heading
        {
            get => _heading;
            set { SetProperty(ref _heading, value, () => Heading); }
        }

        public ObservableCollection<WorkTask> TableTasks { get; }

        public long SelectedOid { get; set; }

        public string SelectedFips
        {
            get => _selectedFips;

            set
            {
                SetProperty(ref _selectedFips, value, () => SelectedFips);
                if (_selectedFips.Length == 5 && int.TryParse(_selectedFips, out int fips))
                {
                    EmptyFips = false;
                }
            }
        }

        public string UicSelection
        {
            get => _uicSelection;
            set
            {
                SetProperty(ref _uicSelection, value, () => UicSelection);
                //clear model if not dirty
                if (_uicSelection.Length > 6 && _uicSelection.Length < 14)
                {
                    CheckForSugestion(value);
                }
                else if (_uicSelection.Length == 14)
                {
                    UicSuggestion = null;
                    Debug.WriteLine("Work flow selection id set");
                    UpdateModel(value);
                    //wellTask.Title = String.Format("Wells Wells {0}", wellModel.WellIds.Count());
                }
            }
        }

        public string UicSuggestion
        {
            get => _uicSuggestion;
            set { SetProperty(ref _uicSuggestion, value, () => UicSuggestion); }
        }

        public BasicFeatureLayer SelectedLayer
        {
            get => _selectedLayer ?? (_selectedLayer = FindFacilities());
            set { SetProperty(ref _selectedLayer, value, () => SelectedLayer); }
        }

        public ICommand UseFacilitySuggestion
        {
            get
            {
                if (_useFacilitySuggestion == null)
                {
                    _useFacilitySuggestion = new RelayCommand(() => { UicSelection = UicSuggestion; },
                                                              () => !string.IsNullOrEmpty(UicSuggestion));
                }

                return _useFacilitySuggestion;
            }
        }

        public ICommand AssignId
        {
            get
            {
                if (_assignIdCmd == null)
                {
                    _assignIdCmd = new RelayCommand(AssignFacilityId, () => MapView.Active != null);
                }

                return _assignIdCmd;
            }
        }

        public ICommand GetSelectedFacility
        {
            get
            {
                if (_getSelectionCmd == null)
                {
                    _getSelectionCmd = new RelayCommand(GetSelectedFeature, () => MapView.Active != null);
                }

                return _getSelectionCmd;
            }
        }

        public ICommand SelectUicAndZoom
        {
            get
            {
                if (_newSelectionCmd == null)
                {
                    _newSelectionCmd = new RelayCommand(() => ModifyLayerSelection(SelectionCombinationMethod.New),
                                                        () => MapView.Active != null && SelectedLayer != null &&
                                                              !string.IsNullOrEmpty(UicSelection));
                }

                return _newSelectionCmd;
            }
        }

        public ICommand SaveModels
        {
            get
            {
                if (_saveModelsCmd == null)
                {
                    _saveModelsCmd = new RelayCommand(() => SaveDirtyModels(), () => true);
                }

                return _saveModelsCmd;
            }
        }

        protected BasicFeatureLayer FindFacilities()
        {
            var facFeature = QueuedTask.Run(() =>
            {
                var uicFacilities = (FeatureLayer) MapView.Active.Map.FindLayers("UICFacility").FirstOrDefault();

                if (uicFacilities == null)
                {
                   FrameworkApplication.AddNotification(new Notification
                   {
                       Message = "The facilities layer could not be found."
                   });

                    return null;
                }

                return (BasicFeatureLayer) uicFacilities;
            }).Result;

            return facFeature;
        }

        //  Behavior test method
        public void ChangeStuff()
        {
            SetTaskItemsComplete(TableTasks[0]);
        }

        /// <summary>
        ///     Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            var pane = FrameworkApplication.DockPaneManager.Find(DockPaneId);
            pane?.Activate();
        }

        internal static void SubRowEvent()
        {
            var pane = FrameworkApplication.DockPaneManager.Find(DockPaneId) as WorkFlowPaneViewModel;
            QueuedTask.Run(() =>
            {
                if (MapView.Active.GetSelectedLayers().Count == 0)
                {
                    MessageBox.Show("Select a feature class from the Content 'Table of Content' first.");
                    Utils.RunOnUiThread(() =>
                    {
                        var contentsPane = FrameworkApplication.DockPaneManager.Find("esri_core_contentsDockPane");
                        contentsPane.Activate();
                    });
                    return;
                }
                //Listen for row events on a layer
                var featLayer = MapView.Active.GetSelectedLayers().First() as FeatureLayer;
                pane.SelectedLayer = featLayer;
            });
        }

        protected void OnCreatedRowEvent(RowChangedEventArgs args)
        {
            Utils.RunOnUiThread(() =>
            {
                Show();

                var pane = FrameworkApplication.DockPaneManager.Find("esri_editing_AttributesDockPane");
                pane.Activate();
            });
        }

        public void SaveDirtyModels()
        {
            foreach (ValidatableBindableBase dataModel in _allModels)
            {
                if (!dataModel.HasModelChanged())
                {
                    continue;
                }

                var m = (IWorkTaskModel) dataModel;
                m.SaveChanges();
                Debug.WriteLine(m.GetType());
            }
        }

        public void CheckTaskItemsOnChange(object model, PropertyChangedEventArgs propertyInfo)
        {
            var areAllModelsClean = true;
            foreach (ValidatableBindableBase dataModel in _allModels)
            {
                if (dataModel.HasModelChanged())
                {
                    areAllModelsClean = false;
                }
            }

            AreModelsDirty = !areAllModelsClean;

            CheckTaskItems(TableTasks[0]);
        }

        private static void SetTaskItemsComplete(WorkTask workTask)
        {
            foreach (var item in workTask.Items)
            {
                SetTaskItemsComplete(item);
            }

            workTask.Complete = true;
        }

        private static void CheckTaskItems(WorkTask workTask)
        {
            foreach (var item in workTask.Items)
            {
                CheckTaskItems(item);
            }

            workTask.CheckForCompletion();
        }

        public void ShowPaneTest(string paneId)
        {
            Utils.RunOnUiThread(() =>
            {
                var pane = FrameworkApplication.DockPaneManager.Find(paneId);
                pane.Activate();
            });
        }

        private async void CheckForSugestion(string partialId)
        {
            await QueuedTask.Run(() =>
            {
                var suggestedId = "";
                var rowCount = 0;
                var map = MapView.Active.Map;
                var uicWells = (FeatureLayer) map.FindLayers("UICFacility").First();
                var qf = new QueryFilter
                {
                    WhereClause = $"FacilityID LIKE '{partialId}%'"
                };

                using (var cursor = uicWells.Search(qf))
                {
                    while (cursor.MoveNext())
                    {
                        using (var row = cursor.Current)
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

        private Task GetSelectedFeature()
        {
            var t = QueuedTask.Run(async () =>
            {
                string selectedId;
                string countyFips;
                Polygon facilityPoly;
                var currentSelection = SelectedLayer.GetSelection();

                using (var cursor = currentSelection.Search())
                {
                    using (var row = cursor.Current)
                    {
                        var facilityFeature = row as Feature;
                        facilityPoly = facilityFeature.GetShape() as Polygon;

                        selectedId = Convert.ToString(row[FacilityModel.IdField]);
                        countyFips = Convert.ToString(row["CountyFIPS"]);
                        SelectedOid = Convert.ToInt64(row["OBJECTID"]);
                    }
                }

                if (string.IsNullOrWhiteSpace(countyFips))
                {
                    // verify it is simple
                    var isSimple = GeometryEngine.Instance.IsSimpleAsFeature(facilityPoly);
                    // find the centroid
                    var facCentroid = GeometryEngine.Instance.Centroid(facilityPoly);
                    await AssignCountyFips(facCentroid);
                }
                else
                {
                    SelectedFips = countyFips;
                }

                if (string.IsNullOrWhiteSpace(selectedId))
                {
                    await AssignFacilityId();
                }
                else
                {
                    UicSelection = selectedId;
                }
            });

            return t;
        }

        private Task ModifyLayerSelection(SelectionCombinationMethod method)
        {
            var t = QueuedTask.Run(() =>
            {
                if (MapView.Active == null || SelectedLayer == null || UicSelection == null)
                {
                    return;
                }


                var wc = $"FacilityID = \"{UicSelection}\"";
                SelectedLayer.Select(new QueryFilter
                {
                    WhereClause = wc
                }, method);

                MapView.Active.ZoomToSelected();
            });

            return t;
        }

        private Task AssignFacilityId()
        {
            var t = QueuedTask.Run(() =>
            {
                //Create list of oids to update
                var oidSet = new List<long>
                {
                    SelectedOid
                };
                //Create edit operation and update
                var op = new EditOperation
                {
                    Name = "Update id"
                };
                var insp = new Inspector();
                insp.Load(SelectedLayer, oidSet);

                var fips = Convert.ToInt64(insp["CountyFIPS"]);
                //long.TryParse(SelectedFips, out fips);
                //insp["CountyFIPS"] = fips;

                var currentGuid = Convert.ToString(insp["GUID"]);
                var hasGuid = Guid.TryParse(currentGuid, out Guid facGuid);
                if (!hasGuid)
                {
                    facGuid = Guid.NewGuid();
                    insp["GUID"] = facGuid;
                }

                var guidLast8 = facGuid.ToString();
                guidLast8 = guidLast8.Substring(guidLast8.Length - 8);
                var newFacilityId = $"UTU{SelectedFips.Substring(SelectedFips.Length - 2)}F{guidLast8}".ToUpper();
                insp[FacilityModel.IdField] = newFacilityId;
                //insp.ApplyAsync();

                op.Modify(insp);
                op.Execute();
                UicSelection = newFacilityId;

                Project.Current.SaveEditsAsync();
            });

            return t;
        }

        public Task AssignCountyFips(MapPoint facCentroid)
        {
            return QueuedTask.Run(async () =>
            {
                var map = MapView.Active.Map;
                var foundFips = "foundFips";
                var counties = (FeatureLayer) map.FindLayers("Counties").First();
                // Using a spatial query filter to find all features which have a certain district name and lying within a given Polygon.
                var spatialQueryFilter = new SpatialQueryFilter
                {
                    FilterGeometry = facCentroid,
                    SpatialRelationship = SpatialRelationship.Within
                };

                using (var cursor = counties.Search(spatialQueryFilter))
                {
                    while (cursor.MoveNext())
                    {
                        using (var feature = (Feature) cursor.Current)
                        {
                            // Process the feature. For example..
                            SelectedFips = Convert.ToString(feature["FIPS_STR"]);
                        }
                    }
                }
                var oidSet = new List<long>
                {
                    SelectedOid
                };
                //Create edit operation and update
                var op = new EditOperation
                {
                    Name = "Update fips"
                };
                var insp = new Inspector();
                insp.Load(SelectedLayer, oidSet);
                insp["CountyFIPS"] = SelectedFips;
                await insp.ApplyAsync();

                await Project.Current.SaveEditsAsync();
            });
        }

        private async void UpdateModel(string uicId)
        {
            await FacilityModel.UpdateModel(uicId);
            AreModelsDirty = false;
            CheckTaskItems(TableTasks[0]);
        }
    }

    public delegate bool IsTaskCompelted();
}
