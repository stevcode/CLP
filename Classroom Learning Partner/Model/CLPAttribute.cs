using System;
using System.Collections.Generic;

namespace Classroom_Learning_Partner.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class CLPAttribute
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
        public string AttributeName
        {
            get
            {
                return _attributeName;
            }
        }

        private List<string> _attributeValues = new List<string>();
        public List<string> AttributeValues
        {
            get
            {
                return _attributeValues;
            }
        }

        public string SelectedValue { get; set; }

        public bool IsSortable { get; set; }

        #endregion //Properties
    }
}
