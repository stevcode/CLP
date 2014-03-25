﻿using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class FuzzyFactorCardStrategyTagType:  ModelBase, TagType
    {
        #region Constructors

        private FuzzyFactorCardStrategyTagType()
        {
            Name = "Fuzzy Factor Card Strategy";
            InElevatedMenu = false;
            AccessLevels = new ObservableCollection<Tag.AccessLevels>();
            AccessLevels.Add(Tag.AccessLevels.Teacher);
            AccessLevels.Add(Tag.AccessLevels.Researcher);

            ExclusiveValue = false;
            ValueOptions = new ObservableCollection<TagOptionValue>();
            ValueOptions.Add(new TagOptionValue("1's")); // e.g. 43 / 4 -> 4 x 1 | 4 x 1 | ... | 4 x 1
            //ValueOptions.Add(new TagOptionValue("10's")); // e.g. 93 / 4 -> 4 x 10 | 4 x 10 | 4 x 3
            ValueOptions.Add(new TagOptionValue("one array")); // Only one array snapped in
            ValueOptions.Add(new TagOptionValue("none")); // No arrays snapped in
            ValueOptions.Add(new TagOptionValue("not enough")); // Not enough arrays snapped in
            ValueOptions.Add(new TagOptionValue("other")); // None of the above*
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected FuzzyFactorCardStrategyTagType(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties


        public string Name
        {
            get { return GetValue<string>(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof(string), "Fuzzy Factor Card Strategy");

        public bool InElevatedMenu
        {
            get { return GetValue<bool>(InElevatedMenuProperty); }
            set { SetValue(InElevatedMenuProperty, value); }
        }

        public static readonly PropertyData InElevatedMenuProperty = RegisterProperty("InElevatedMenu", typeof(bool), false);

        public ObservableCollection<Tag.AccessLevels> AccessLevels
        {
            get { return GetValue<ObservableCollection<Tag.AccessLevels>>(AccessLevelsProperty); }
            set { SetValue(AccessLevelsProperty, value); }
        }

        public static readonly PropertyData AccessLevelsProperty = RegisterProperty("AccessLevels", typeof(ObservableCollection<Tag.AccessLevels>), new ObservableCollection<Tag.AccessLevels>());

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

        public static FuzzyFactorCardStrategyTagType Instance = new FuzzyFactorCardStrategyTagType();
    }
}