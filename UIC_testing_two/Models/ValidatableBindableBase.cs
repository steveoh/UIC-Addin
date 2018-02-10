using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UIC_Edit_Workflow
{
    class ValidatableBindableBase : BindableBase, INotifyDataErrorInfo
    {

        protected ValidatableBindableBase()
        {
            LoadHash = calculateFieldHash();
        }
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

        private string _loadHash;
        public string LoadHash
        {
            get
            {
                return _loadHash;
            }

            set
            {
                _loadHash = value;
            }
        }

        protected virtual string fieldValueString()
        {
            throw new NotImplementedException("not yet implemented");
        }
        protected string calculateFieldHash()
        {
            string fieldString = fieldValueString();
            // step 1, calculate MD5 hash from input

            MD5 md5 = MD5.Create();

            byte[] inputBytes = Encoding.ASCII.GetBytes(fieldString);

            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)

            {

                sb.Append(hash[i].ToString("X2"));

            }

            return sb.ToString();
        }

        public bool HasModelChanged()
        {
            return !LoadHash.Equals(calculateFieldHash());
        }
    }
}
