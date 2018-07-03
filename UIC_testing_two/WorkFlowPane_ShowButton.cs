using ArcGIS.Desktop.Framework.Contracts;

namespace UIC_Edit_Workflow
{
    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class WorkFlowPane_ShowButton : Button
    {
        protected override void OnClick()
        {
            //WorkFlowPaneViewModel.subRowEvent();
            WorkFlowPaneViewModel.Show();
        }
    }
}