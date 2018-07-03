using System.Collections.ObjectModel;
using System.Linq;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace UIC_Edit_Workflow
{
    internal class WorkTask : BindableBase
    {
        public WorkTask(string activePanel, IsTaskCompelted completeCheck = null)
        {
            Items = new ObservableCollection<WorkTask>();
            ActivePanel = activePanel;
            if (completeCheck != null)
            {
                IsComplete += completeCheck;
            }
            else
            {
                IsComplete += AreChildrenComplete;
            }

            Complete = false;
            IsExpanded = true;
        }

        public IsTaskCompelted IsComplete;
        private string _title; 
        public string Title {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        private bool _complete;
        public bool Complete {
            get => _complete;
            set => SetProperty(ref _complete, value);
        }
        public string ActivePanel { get; set; }
        //public IsTaskCompelted IsCompelete { get; set; }

        private bool _isExpanded;
        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        bool _isSelected;
        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                SetProperty(ref _isSelected, value);
                if (_isSelected)
                {
                    QueuedTask.Run(() => {
                        Utils.RunOnUiThread(() =>
                        {
                            var pane = FrameworkApplication.DockPaneManager.Find(ActivePanel);
                            pane.Activate();
                        });
                    });
                }
            }
        }

        public bool CheckForCompletion()
        {
            var complete = IsComplete();
            Complete = complete;

            return complete;
        }

        public bool AreChildrenComplete()
        {
            return Items.All(child => child.Complete);
        }

        public ObservableCollection<WorkTask> Items { get; set; }
    }
}