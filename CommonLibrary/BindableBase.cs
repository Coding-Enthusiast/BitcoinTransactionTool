﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CommonLibrary
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DependsOnPropertyAttribute : Attribute
    {
        /// <summary>
        /// Instantiates an instance with only 1 depending properties.
        /// </summary>
        /// <param name="dependingPropertyName">Name of the property to depend on.</param>
        public DependsOnPropertyAttribute(string dependingPropertyName)
        {
            DependentProps = new string[] { dependingPropertyName };
        }

        /// <summary>
        /// Instantiates an instance with an array of depending properties.
        /// </summary>
        /// <param name="dependingPropertyNames">Array of Names of the properties to depend on.</param>
        public DependsOnPropertyAttribute(string[] dependingPropertyNames)
        {
            DependentProps = dependingPropertyNames;
        }

        /// <summary>
        /// Array of all the properties that the property depends on!
        /// </summary>
        public readonly string[] DependentProps;
    }














    public class BindableBase : INotifyPropertyChanged
    {
        public BindableBase()
        {
            PropertyDependencyMap = new Dictionary<string, List<string>>();

            foreach (var property in GetType().GetProperties())
            {
                var attributes = property.GetCustomAttributes<DependsOnPropertyAttribute>();
                foreach (var dependsAttr in attributes)
                {
                    if (dependsAttr == null)
                    {
                        continue;
                    }

                    foreach (var dependence in dependsAttr.DependentProps)
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
        /// Dictonary of properties which have a dependant property.
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
                    foreach (var p in PropertyDependencyMap[propertyName])
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