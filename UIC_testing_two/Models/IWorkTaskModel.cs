using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIC_testing_two
{
    interface IWorkTaskModel
    {
        Task UpdateModel(string facilityId);
        void FacilityChangeHandler(string oldId, string newId, string facGuid);
    }
}
