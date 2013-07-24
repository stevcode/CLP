using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Catel.Data;


namespace CLP.Models
{
    public interface TagType : IModel
    {
        String Name { get; set; }
        bool InElevatedMenu { get; set; }
        ObservableCollection<Tag.AccessLevels> AccessLevels { get; set; }
        bool ExclusiveValue { get; set; }
        ObservableCollection<TagOptionValue> ValueOptions { get; set; }
    }
}



   

