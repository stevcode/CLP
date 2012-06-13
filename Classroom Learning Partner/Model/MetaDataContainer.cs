using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Catel.Data;

namespace Classroom_Learning_Partner.Model
{
    //OBSOLETE CLASS - REMOVE WHEN ALL CONVERSIONS ARE MADE
    [Serializable]
    public class MetaDataContainer : DataObjectBase<MetaDataContainer>
    {
        public MetaDataContainer()
        {

        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected MetaDataContainer(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        /// <summary>
        /// Retrieves the actual data from the serialization info.
        /// </summary>
        /// <remarks>
        /// This method should only be implemented if backwards compatibility should be implemented for
        /// a class that did not previously implement the DataObjectBase class.
        /// </remarks>
        protected override void GetDataFromSerializationInfo(SerializationInfo info)
        {
            // Check if deserialization succeeded
            if(DeserializationSucceeded)
            {
                return;
            }

            // Deserialization did not succeed for any reason, so retrieve the values manually
            // Luckily there is a helper class (SerializationHelper) 
            // that eases the deserialization of "old" style objects
            //FirstName = SerializationHelper.GetString(info, "FirstName", FirstNameProperty.GetDefaultValue());
            //LastName = SerializationHelper.GetString(info, "LastName", LastNameProperty.GetDefaultValue());
        }
    }
}
