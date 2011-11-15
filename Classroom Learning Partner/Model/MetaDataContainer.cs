using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classroom_Learning_Partner.Model
{
    [Serializable]
    public class MetaDataContainer
    {
        public MetaDataContainer()
        {
        }

        private Dictionary<string, CLPAttributeValue> _hashMap = new Dictionary<string, CLPAttributeValue>();
        public Dictionary<string, CLPAttributeValue> HashMap
        {
            get
            {
                return _hashMap;
            }
        }

        public string GetValue(string key)
        {
            if (HashMap.ContainsKey(key))
            {
                return HashMap[key].SelectedValue;
            }
            else
            {
                return "NULL_KEY";
            }
        }

        public void SetValue(string key, string value)
        {
            if (HashMap.ContainsKey(key))
            {
                HashMap[key].SetAttributeValues(value);
            }
            else
            {
                HashMap.Add(key, new CLPAttributeValue(value));
            }
        }

        public void SetValue(string key, List<string> values)
        {
            if (HashMap.ContainsKey(key))
            {
                HashMap[key].SetAttributeValues(values);
            }
            else
            {
                HashMap.Add(key, new CLPAttributeValue(values));
            }
        }
    }
}
