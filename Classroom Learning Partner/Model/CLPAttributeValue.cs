using System;
using System.Collections.Generic;

namespace Classroom_Learning_Partner.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class CLPAttributeValue
    {

        #region Constructors

        public CLPAttributeValue(string attributeValue)
        {
            SetAttributeValues(attributeValue);
            IsSortable = false;
        }

        public CLPAttributeValue(List<string> attributeValues)
        {
            SetAttributeValues(attributeValues);
            IsSortable = false;
        }

        #endregion //Constructors

        #region Properties

        private List<string> _attributeValues;
        public List<string> AttributeValues
        {
            get
            {
                return _attributeValues;
            }
        }

        private int _selectedValueIndex;
        public int SelectedValueIndex
        {
            get
            {
                return _selectedValueIndex;
            }
            set
            {
                if (value < _attributeValues.Count && value >= 0)
                {
                    _selectedValueIndex = value;
                }
                else
                {
                    Console.WriteLine("Attempted to set SelectedValueIndex out of bounds, defaulted to 0");
                    _selectedValueIndex = 0;
                }
            }
        }

        public string SelectedValue
        {
            get
            {
                return AttributeValues[SelectedValueIndex];
            }
        }

        public bool IsSortable { get; set; }

        #endregion //Properties

        #region Methods

        public void SetAttributeValues(string value)
        {
            _attributeValues = new List<string>();
            _attributeValues.Add(value);
            SelectedValueIndex = 0;
        }

        public void SetAttributeValues(List<string> values)
        {
            _attributeValues = values;
            SelectedValueIndex = 0;
        }

        #endregion //Methods
    }
}
