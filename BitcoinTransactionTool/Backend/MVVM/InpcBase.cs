// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BitcoinTransactionTool.Backend.MVVM
{
    /// <summary>
    /// Base (abstract) class implementing <see cref="INotifyPropertyChanged"/>. 
    /// Could be used for both ViewModels and Models
    /// </summary>
    public abstract class InpcBase : INotifyPropertyChanged
    {
        public InpcBase()
        {
            PropertyDependencyMap = new Dictionary<string, List<string>>();

            foreach (PropertyInfo property in GetType().GetProperties())
            {
                foreach (DependsOnPropertyAttribute dependsAttr in property.GetCustomAttributes<DependsOnPropertyAttribute>())
                {
                    if (dependsAttr == null)
                    {
                        continue;
                    }

                    foreach (string dependence in dependsAttr.DependentProps)
                    {
                        if (!PropertyDependencyMap.ContainsKey(dependence))
                        {
                            PropertyDependencyMap.Add(dependence, new List<string>());
                        }
                        PropertyDependencyMap[dependence].Add(property.Name);
                    }
                }
            }
        }


        /// <summary>
        /// Dictonary of all properties which have a dependant property.
        /// </summary>
        protected Dictionary<string, List<string>> PropertyDependencyMap;

        /// <summary>
        /// The PropertyChanged Event to raise to any UI object
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The PropertyChanged Event to raise to any UI object
        /// The event is only invoked if data binding is used
        /// </summary>
        /// <param name="propertyName">The Name of the property that is changing.</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                // Raise the PropertyChanged event.
                handler(this, new PropertyChangedEventArgs(propertyName));

                // Raise the PropertyChanged event for dependant properties too.
                if (PropertyDependencyMap.ContainsKey(propertyName))
                {
                    foreach (string p in PropertyDependencyMap[propertyName])
                    {
                        handler(this, new PropertyChangedEventArgs(p));
                    }
                }
            }
        }

        /// <summary>
        /// Sets the value of a property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="propertyName">The Name of the property that is changing. If null it passes caller name in compile time.</param>
        /// <returns>Returs false if value didn't change.</returns>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }
            else
            {
                field = value;
                RaisePropertyChanged(propertyName);
                return true;
            }
        }
    }
}
