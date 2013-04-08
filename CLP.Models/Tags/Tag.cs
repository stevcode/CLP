using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace CLP.Models.Tags
{
    [Serializable]
    public class Tag : DataObjectBase
    {
           #region Constructors

             public Tag(string origin, TagType tagType, ObservableCollection<TagOptionValue> val)
             {
                 Origin = origin;
                 TagType = tagType;
                 Value = val;

             }

             public Tag(string origin, TagType tagType)
             {
                 Origin = origin;
                 TagType = tagType;
                 Value = new ObservableCollection<TagOptionValue>();

             }

                /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected Tag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

             #endregion //Constructors

             #region Properties

             /// <summary>
             /// The origin of the tag.
             /// </summary>
             public string Origin
             {
                 get { return GetValue<string>(OriginProperty); }
                 set { SetValue(OriginProperty, value); }
             }

             /// <summary>
             /// Register the Origin property so it is known in the class.
             /// </summary>
              public static readonly PropertyData OriginProperty = RegisterProperty("Origin", typeof(string), "");

              public string Name
              {
                  get { return GetValue<string>(NameProperty); }
                  set { SetValue(NameProperty, value); }
              }

              public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof(string), "");


              public TagType TagType
              {
                  get { return GetValue<TagType>(TagTypeProperty); }
                  set { SetValue(TagTypeProperty, value); }
              }

              public static readonly PropertyData TagTypeProperty = RegisterProperty("TagType", typeof(TagType), null);

 
              public ObservableCollection<TagOptionValue> Value
              {
                  get { return GetValue<ObservableCollection<TagOptionValue>>(ValueProperty); }
                  set { SetValue(ValueProperty, value); }
              }

              public static readonly PropertyData ValueProperty = RegisterProperty("Value", typeof(ObservableCollection<TagOptionValue>), new ObservableCollection<TagOptionValue>());
              #endregion //Properties

            #region Methods

            public void AddTagOptonValue(TagOptionValue t)
              {
                  if(TagType.ValueOptions.Contains(t))
                  {
                      if(TagType.ExclusiveValue == true)
                      {
                          Value.Clear();
                          
                      }
                      Value.Add(t);

                  }
              }

        #endregion

    }
}