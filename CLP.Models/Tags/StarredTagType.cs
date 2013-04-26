﻿using System;
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
    public class StarredTagType : DataObjectBase, TagType
    {
        #region Constructors

        public StarredTagType()
        {
            Name = "Starred";
            InElevatedMenu = true;
            AccessLevels = new ObservableCollection<string>();
            AccessLevels.Add("Teacher");

            ExclusiveValue = true;
            ValueOptions = new ObservableCollection<TagOptionValue>(); 
            ValueOptions.Add(new TagOptionValue("Starred","..\\Images\\Starred.png"));
            ValueOptions.Add(new TagOptionValue("Unstarred","..\\Images\\Unstarred.png"));

        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected StarredTagType(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public string Name
        {
            get { return GetValue<string>(NameProperty); }
            set { SetValue(NameProperty, value); }
        }


        public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof(string), "");

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
    }
}