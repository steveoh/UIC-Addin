using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace UIC_testing_two
{
    internal class WellAttributeEditorViewModel : DockPane
    {
        private const string _dockPaneID = "UIC_testing_two_WellAttributeEditor";
        private UICModel uicModel = UICModel.Instance;
        private string selectedWell;

        protected WellAttributeEditorViewModel()
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
        private string _heading = "My DockPane";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        public string SelectedWell
        {
            get
            {
                return selectedWell;
            }

            set
            {
                selectedWell = value;
                uicModel.SelectedWellId = selectedWell;
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
