using System;

namespace Nathanael.DI
{
    [AttributeUsage(AttributeTargets.Constructor)]
    public class DIConstructorAttribute : Attribute
    {

        public DIConstructorAttribute()
        {
        }
    }
}