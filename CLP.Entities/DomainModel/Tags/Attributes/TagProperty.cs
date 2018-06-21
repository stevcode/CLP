using System;

namespace CLP.Entities
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class TagProperty : Attribute
    {
        public TagProperty(string propertyName, string possibleValue)
        {
            PropertyName = propertyName;
            PossibleValue = possibleValue;
        }

        public string PropertyName { get; private set; }
        public string PossibleValue { get; private set; }
    }
}