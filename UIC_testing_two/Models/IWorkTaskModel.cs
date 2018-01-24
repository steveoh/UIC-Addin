﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIC_Edit_Workflow
{
    interface IWorkTaskModelFun
    {
        Task UpdateModel(string facilityId);
        void ControllingIdChangedHandler(string oldGuid, string newGuid);
    }
}
