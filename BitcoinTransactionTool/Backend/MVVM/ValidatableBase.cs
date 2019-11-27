// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace BitcoinTransactionTool.Backend.MVVM
{
    /// <summary>
    /// Base (abstract) class implementing <see cref="INotifyDataErrorInfo"/> and inherits from <see cref="InpcBase"/>.
    /// Could be used for any model that requires validation.
    /// </summary>
    public abstract class ValidatableBase : InpcBase, INotifyDataErrorInfo
    {
        /// <summary>
        /// List of all errors with property name as its key.
        /// </summary>
        private readonly Dictionary<string, List<string>> errorList = new Dictionary<string, List<string>>();


        /// <summary>
        /// When overriden should implement validation logic specific to the model.
        /// <para/> Call <see cref="ClearErrors(string)"/> method at the start of validation to clear all errors of that property.
        /// </summary>
        public abstract void Validate();


        public void AddError(string propertyName, string error)
        {
            if (!errorList.ContainsKey(propertyName))
            {
                errorList[propertyName] = new List<string>();
            }
            if (!errorList[propertyName].Contains(error))
            {
                errorList[propertyName].Add(error);
                RaiseErrorsChanged(propertyName);
            }
        }

        public void ClearErrors(string propertyName)
        {
            if (errorList.ContainsKey(propertyName))
            {
                errorList.Remove(propertyName);
                RaiseErrorsChanged(propertyName);
            }
        }

        public void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }



        public bool HasErrors => errorList.Count > 0;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                errorList.TryGetValue(propertyName, out List<string> res);
                return res;
            }
            else
            {
                return null;
            }
        }
    }
}
