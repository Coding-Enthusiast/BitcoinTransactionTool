using System;

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
}
