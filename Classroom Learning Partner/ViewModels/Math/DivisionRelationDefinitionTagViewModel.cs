using System;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>UserControl view model.</summary>
    public class DivisionRelationDefinitionTagViewModel : ViewModelBase
    {
        /// <summary>Initializes a new instance of the <see cref="MultiplicationRelationDefinitionTagViewModel" /> class.</summary>
        public DivisionRelationDefinitionTagViewModel(DivisionRelationDefinitionTag divisionRelationDefinition)
        {
            Model = divisionRelationDefinition;

            CalculateQuotientCommand = new Command(OnCalculateQuotientCommandExecute);
        }

        /// <summary>Gets the title of the view model.</summary>
        /// <value>The title.</value>
        public override string Title
        {
            get { return "MultiplicationRelationDefinitionTagVM"; }
        }

        /// <summary>Gets or sets the property value.</summary>
        [Model(SupportIEditableObject = false)]
        public DivisionRelationDefinitionTag Model
        {
            get { return GetValue<DivisionRelationDefinitionTag>(ModelProperty); }
            private set { SetValue(ModelProperty, value); }
        }

        public static readonly PropertyData ModelProperty = RegisterProperty("Model", typeof (DivisionRelationDefinitionTag));

        /// <summary>Dividend of the division relation.</summary>
        [ViewModelToModel("Model")]
        public double Dividend
        {
            get { return GetValue<double>(DividendProperty); }
            set { SetValue(DividendProperty, value); }
        }

        public static readonly PropertyData DividendProperty = RegisterProperty("Dividend", typeof (double));

        /// <summary>Divisor of the division relation.</summary>
        [ViewModelToModel("Model")]
        public double Divisor
        {
            get { return GetValue<double>(DivisorProperty); }
            set { SetValue(DivisorProperty, value); }
        }

        public static readonly PropertyData DivisorProperty = RegisterProperty("Divisor", typeof (double));

        /// <summary>Quotient of the division relation.</summary>
        [ViewModelToModel("Model")]
        public double Quotient
        {
            get { return GetValue<double>(QuotientProperty); }
            set { SetValue(QuotientProperty, value); }
        }

        public static readonly PropertyData QuotientProperty = RegisterProperty("Quotient", typeof (double));

        /// <summary>Remainder of the division relation.</summary>
        [ViewModelToModel("Model")]
        public double Remainder
        {
            get { return GetValue<double>(RemainderProperty); }
            set { SetValue(RemainderProperty, value); }
        }

        public static readonly PropertyData RemainderProperty = RegisterProperty("Remainder", typeof (double));

        /// <summary>Type of division relationship the relation defines.</summary>
        [ViewModelToModel("Model")]
        public DivisionRelationDefinitionTag.RelationTypes RelationType
        {
            get { return GetValue<DivisionRelationDefinitionTag.RelationTypes>(RelationTypeProperty); }
            set { SetValue(RelationTypeProperty, value); }
        }

        public static readonly PropertyData RelationTypeProperty = RegisterProperty("RelationType",
                                                                                    typeof (DivisionRelationDefinitionTag.RelationTypes));

        /// <summary>
        /// Calculates the Quotient and Remainder given the Dividend and Divisor.
        /// </summary>
        public Command CalculateQuotientCommand { get; private set; }

        private void OnCalculateQuotientCommandExecute()
        {
            if (Divisor == 0)
            {
                return;
            }

            Quotient = Math.Floor(Dividend / Divisor);
            Remainder = Dividend % Divisor;
        }
    }
}