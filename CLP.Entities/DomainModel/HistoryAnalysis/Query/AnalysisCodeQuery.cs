using System;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class AnalysisCodeQuery : ASerializableBase, IQueryPart
    {
        #region Constructor

        public AnalysisCodeQuery() { }

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

        public IQueryPart FirstCondition
        {
            get => GetValue<IQueryPart>(FirstConditionProperty);
            set
            {
                SetValue(FirstConditionProperty, value);
                RaisePropertyChanged(nameof(LongFormattedValue));
            }
        }

        public static readonly PropertyData FirstConditionProperty = RegisterProperty(nameof(FirstCondition), typeof(IQueryPart), null);

        public IQueryPart SecondCondition
        {
            get => GetValue<IQueryPart>(SecondConditionProperty);
            set
            {
                SetValue(SecondConditionProperty, value);
                RaisePropertyChanged(nameof(LongFormattedValue));
            }
        }

        public static readonly PropertyData SecondConditionProperty = RegisterProperty(nameof(SecondCondition), typeof(IQueryPart), null);

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
                    var conditional = Conditional.ToString();
                    result = $"{firstConditionName} {conditional} {secondConditionName}";
                }

                return $"{QueryName}: {result}";
            }
        }

        public string ButtonFormattedValue => QueryName;

        #endregion // IQueryPart Implementation
    }
}
