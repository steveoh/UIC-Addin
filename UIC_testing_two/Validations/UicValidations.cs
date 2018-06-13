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
        // https://github.com/agrc/uic-etl/blob/master/domain.uic-etl/xml/ContactDetail.cs
        public override bool IsValid(object value)
        {
            return (string)value == "correct";
        }
        public override string FormatErrorMessage(string name)
        {
            return String.Format(System.Globalization.CultureInfo.CurrentCulture,
              ErrorMessageString, name);
        }
    }

    sealed public class NameTest : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return (string)value == "tester";
        }
        public override string FormatErrorMessage(string name)
        {
            return String.Format(System.Globalization.CultureInfo.CurrentCulture,
              ErrorMessageString, name);
        }
    }

}
