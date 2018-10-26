using CommonLibrary.CryptoEncoders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace CommonLibrary
{
    public class ValidatableBase : InpcBase, INotifyDataErrorInfo
    {
        private Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || !errors.ContainsKey(propertyName))
            {
                return null;
            }
            else
            {
                return errors[propertyName];
            }
        }

        public bool HasErrors
        {
            get
            {
                return errors.Count > 0;
            }
        }


        public void AddError(string propertyName, string error)
        {
            if (!errors.ContainsKey(propertyName))
            {
                errors[propertyName] = new List<string>();
            }
            if (!errors[propertyName].Contains(error))
            {
                errors[propertyName].Add(error);
                RaiseErrorsChanged(propertyName);
            }
        }

        public void RemoveError(string propertyName, string error)
        {
            if (errors.ContainsKey(propertyName))
            {
                errors.Remove(propertyName);
                RaiseErrorsChanged(propertyName);
            }
        }

        public void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public void Validate(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                AddError(nameof(address), "Address can not be empty!");
            }
            else if (address.StartsWith("1") || address.StartsWith("3"))
            {
                Base58 b58enc = new Base58();
                if (b58enc.IsValid(address))
                {
                    RemoveError(nameof(address), "");
                }
                else
                {
                    AddError(nameof(address), "Invalid Base58 encoded address!");
                }
            }
            else if (address.StartsWith("bc"))
            {
                Bech32 b32enc = new Bech32();
                if (b32enc.IsValid(address))
                {
                    RemoveError(nameof(address), "");
                }
                else
                {
                    AddError(nameof(address), "Invalid Bech32 encoded address!");
                }
            }
            else
            {
                AddError(nameof(address), "Invalid address format!");
            }
        }

    }
}
