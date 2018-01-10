using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UIC_Edit_Workflow
{
    class ValidatableBindableBase : BindableBase, INotifyDataErrorInfo
    {
        private Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
            {
                return _errors[propertyName];
            }
            else
            {
                return null;
            }
        }

        public bool HasErrors
        {
            get
            {
                return _errors.Count > 0;
            }
        }

        protected override void SetProperty<T>(ref T member, T val, [CallerMemberName] string propertyName = null)
        {
            ValidateProperty(propertyName, val);
            base.SetProperty<T>(ref member, val, propertyName);
        }

        private void ValidateProperty<T>(string propertyName, T value)
        {
            var results = new List<ValidationResult>();
            ValidationContext context = new ValidationContext(this);
            context.MemberName = propertyName;
            Validator.TryValidateProperty(value, context, results);

            if (results.Any())
            {
                _errors[propertyName] = results.Select(c => c.ErrorMessage).ToList();
            }
            else
            {
                _errors.Remove(propertyName);
            }
            ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}
