using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using UIC_Edit_Workflow.Models;

namespace UIC_Edit_Workflow
{
    internal class Module1 : Module
    {
        private static Module1 _this;
        private static FacilityModel _facility;
        private static WellModel _well;
        private static AuthorizationModel _authorization;
        private static FacilityInspectionModel _facilityInspection;
        private static WellInspectionModel _wellInspection;

        /// <summary>
        ///     Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ?? (_this =
                                             (Module1) FrameworkApplication.FindModule("UIC_Edit_Workflow_Module"));

        public static FacilityModel FacilityModel => _facility ?? (_facility = new FacilityModel());
        public static WellModel WellModel => _well ?? (_well = new WellModel());

        public static AuthorizationModel AuthorizationModel => _authorization ??
                                                               (_authorization = new AuthorizationModel());

        public static FacilityInspectionModel FacilityInspectionModel => _facilityInspection ??
                                                                         (_facilityInspection =
                                                                             new FacilityInspectionModel());

        public static WellInspectionModel WellInspectionModel => _wellInspection ??
                                                                 (_wellInspection = new WellInspectionModel());

        /// <summary>
        ///     Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            return true;
        }
    }
}
