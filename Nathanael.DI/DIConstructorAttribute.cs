using System;

namespace Nathanael.DI
{
    /// <summary>
    ///  This attribute is used to mark the constructor that should be used by the service provider to generate an instance of the class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public class DIConstructorAttribute : Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DIConstructorAttribute()
        {
        }
    }
}