using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class DomainInterpretationTagType : ModelBase, TagType
    {
        #region Constructors

        private DomainInterpretationTagType()
        {
            Name = "DomainInterpretation";
            InElevatedMenu = false;
            AccessLevels = new ObservableCollection<string>();
            AccessLevels.Add("Teacher");
            AccessLevels.Add("Student");

            ExclusiveValue = false;
            ValueOptions = new ObservableCollection<TagOptionValue>(); 
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected DomainInterpretationTagType(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// The Name of the tag type.
        /// </summary>
        public string Name
        {
            get { return GetValue<string>(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        /// <summary>
        /// Register the Name property
        /// </summary>
        public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof(string), "");

        /// <summary>
        /// Whether it shows up in the elevated menu or you have to click to expand it
        /// </summary>
        public bool InElevatedMenu
        {
            get { return GetValue<bool>(InElevatedMenuProperty); }
            set { SetValue(InElevatedMenuProperty, value); }
        }

        public static readonly PropertyData InElevatedMenuProperty = RegisterProperty("InElevatedMenu", typeof(bool), false);

        public ObservableCollection<string> AccessLevels
        {
            get { return GetValue<ObservableCollection<string>>(AccessLevelsProperty); }
            set { SetValue(AccessLevelsProperty, value); }
        }

        public static readonly PropertyData AccessLevelsProperty = RegisterProperty("AccessLevels", typeof(ObservableCollection<string>), new ObservableCollection<string>());

        public ObservableCollection<TagOptionValue> ValueOptions
        {
            get { return GetValue<ObservableCollection<TagOptionValue>>(ValueOptionsProperty); }
            set { SetValue(ValueOptionsProperty, value); }
        }

        public static readonly PropertyData ValueOptionsProperty = RegisterProperty("ValueOptions", typeof(ObservableCollection<TagOptionValue>), new ObservableCollection<TagOptionValue>());

        public bool ExclusiveValue
        {
            get { return GetValue<bool>(ExclusiveValueProperty); }
            set { SetValue(ExclusiveValueProperty, value); }
        }

        public static readonly PropertyData ExclusiveValueProperty = RegisterProperty("ExclusiveValue", typeof(bool), false);
        #endregion

        public static DomainInterpretationTagType Instance = new DomainInterpretationTagType();
    }
}