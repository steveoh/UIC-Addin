using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIC_Edit_Workflow
{
    sealed public class UicValidations : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return (string)value == "correct";
        }
    }
    //public override string FormatErrorMessage(string name)
    //{
    //    return String.Format(CultureInfo.CurrentCulture,
    //      ErrorMessageString, name, this.Mask);
    //}
}
