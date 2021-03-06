﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class AnalysisCodeQuery : ASerializableBase, IQueryPart
    {
        #region Constructor

        public AnalysisCodeQuery()
        {
            Conditions.Add(new QueryCondition());
        }

        #endregion // Constructor

        #region Properties

        public string QueryName
        {
            get => GetValue<string>(QueryNameProperty);
            set
            {
                SetValue(QueryNameProperty, value);
                RaisePropertyChanged(nameof(LongFormattedValue));
            }
        }

        public static readonly PropertyData QueryNameProperty = RegisterProperty(nameof(QueryName), typeof(string), string.Empty);

        public string QueryDescription
        {
            get => GetValue<string>(QueryDescriptionProperty);
            set => SetValue(QueryDescriptionProperty, value);
        }

        public static readonly PropertyData QueryDescriptionProperty = RegisterProperty(nameof(QueryDescription), typeof(string), string.Empty);

        public ObservableCollection<QueryCondition> Conditions
        {
            get => GetValue<ObservableCollection<QueryCondition>>(ConditionsProperty);
            set => SetValue(ConditionsProperty, value);
        }

        public static readonly PropertyData ConditionsProperty =
            RegisterProperty(nameof(Conditions), typeof(ObservableCollection<QueryCondition>), () => new ObservableCollection<QueryCondition>());

        public QueryConditionals Conditional
        {
            get => GetValue<QueryConditionals>(ConditionalProperty);
            set
            {
                SetValue(ConditionalProperty, value);
                RaisePropertyChanged(nameof(LongFormattedValue));
            }
        }

        public static readonly PropertyData ConditionalProperty = RegisterProperty(nameof(Conditional), typeof(QueryConditionals), QueryConditionals.None);

        #region Calculated Properties

        public IQueryPart FirstCondition => !Conditions.Any() ? null : Conditions.First().QueryPart;

        public IQueryPart SecondCondition => Conditions.Count < 2 ? null : Conditions[1].QueryPart;

        #endregion // Calculated Properties

        #endregion // Properties

        #region IQueryPart Implementation

        public string LongFormattedValue
        {
            get
            {
                var firstConditionName = string.Empty;
                if (FirstCondition is AnalysisCodeQuery codeQuery)
                {
                    firstConditionName = codeQuery.QueryName;
                }
                else if (FirstCondition is AnalysisCode queryCondition)
                {
                    firstConditionName = queryCondition.LongFormattedValue;
                }

                var secondConditionName = string.Empty;
                if (SecondCondition is AnalysisCodeQuery codeQuery2)
                {
                    secondConditionName = codeQuery2.QueryName;
                }
                else if (SecondCondition is AnalysisCode queryCondition2)
                {
                    secondConditionName = queryCondition2.LongFormattedValue;
                }

                var result = firstConditionName;
                if (Conditional != QueryConditionals.None)
                {
                    var conditional = string.Empty;
                    switch (Conditional)
                    {
                        case QueryConditionals.And:
                            conditional = "and";
                            break;
                        case QueryConditionals.Or:
                            conditional = "or";
                            break;
                    }

                    result = $"{firstConditionName} {conditional} {secondConditionName}";
                }

                return $"{QueryName}: {result}";
            }
        }

        public string ButtonFormattedValue => QueryName;

        #endregion // IQueryPart Implementation
    }
}
