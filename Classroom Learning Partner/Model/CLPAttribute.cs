using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Classroom_Learning_Partner.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    class CLPAttribute
    {

        #region Constructors

        public CLPAttribute(string attributeName, string attributeValue)
        {
            _attributeName = attributeName;
            _attributeValues.Add(attributeValue);
            SelectedValue = _attributeValues[0];
            IsSortable = false;
        }

        public CLPAttribute(string attributeName, List<string> attributeValues)
        {
            _attributeName = attributeName;
            _attributeValues = attributeValues;
            SelectedValue = _attributeValues[0];
            IsSortable = false;
        }

        #endregion //Constructors

        #region Properties

        private string _attributeName;
        [DataMember]
        public string AttributeName
        {
            get
            {
                return _attributeName;
            }
        }

        private List<string> _attributeValues;
        [DataMember]
        public List<string> AttributeValues
        {
            get
            {
                return _attributeValues;
            }
        }

        [DataMember]
        public string SelectedValue { get; set; }

        [DataMember]
        public bool IsSortable { get; set; }

        #endregion //Properties
    }
}
