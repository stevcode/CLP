using System;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>UserControl view model.</summary>
    public class DivisionRelationDefinitionTagViewModel : ViewModelBase
    {
        #region Constructor

        /// <summary>Initializes a new instance of the <see cref="MultiplicationRelationDefinitionTagViewModel" /> class.</summary>
        public DivisionRelationDefinitionTagViewModel(DivisionRelationDefinitionTag divisionRelationDefinition)
        {
            Model = divisionRelationDefinition;

            Model.Dividend = new NumericValueDefinitionTag(Model.ParentPage, Model.Origin);
            Model.Divisor = new NumericValueDefinitionTag(Model.ParentPage, Model.Origin);

            InitializeCommands();
        }

        #endregion // Constructor

        #region Model

        /// <summary>Gets or sets the property value.</summary>
        [Model(SupportIEditableObject = false)]
        public DivisionRelationDefinitionTag Model
        {
            get { return GetValue<DivisionRelationDefinitionTag>(ModelProperty); }
            private set { SetValue(ModelProperty, value); }
        }

        public static readonly PropertyData ModelProperty = RegisterProperty("Model", typeof(DivisionRelationDefinitionTag));

        /// <summary>Type of division relationship the relation defines.</summary>
        [ViewModelToModel("Model")]
        public DivisionRelationDefinitionTag.RelationTypes RelationType
        {
            get { return GetValue<DivisionRelationDefinitionTag.RelationTypes>(RelationTypeProperty); }
            set { SetValue(RelationTypeProperty, value); }
        }

        public static readonly PropertyData RelationTypeProperty = RegisterProperty("RelationType", typeof(DivisionRelationDefinitionTag.RelationTypes));

        /// <summary>Quotient of the division relation.</summary>
        [ViewModelToModel("Model")]
        public double Quotient
        {
            get { return GetValue<double>(QuotientProperty); }
            set { SetValue(QuotientProperty, value); }
        }

        public static readonly PropertyData QuotientProperty = RegisterProperty("Quotient", typeof(double));

        /// <summary>Remainder of the division relation.</summary>
        [ViewModelToModel("Model")]
        public double Remainder
        {
            get { return GetValue<double>(RemainderProperty); }
            set { SetValue(RemainderProperty, value); }
        }

        public static readonly PropertyData RemainderProperty = RegisterProperty("Remainder", typeof(double));

        #endregion // Model

        #region Bindings

        /// <summary>Dividend of the division relation.</summary>
        public double Dividend
        {
            get { return GetValue<double>(DividendProperty); }
            set { SetValue(DividendProperty, value); }
        }

        public static readonly PropertyData DividendProperty = RegisterProperty("Dividend", typeof(double));

        /// <summary>Divisor of the division relation.</summary>
        public double Divisor
        {
            get { return GetValue<double>(DivisorProperty); }
            set { SetValue(DivisorProperty, value); }
        }

        public static readonly PropertyData DivisorProperty = RegisterProperty("Divisor", typeof(double));

        #endregion // Bindings

        #region Commands

        private void InitializeCommands()
        {
            CalculateQuotientCommand = new Command(OnCalculateQuotientCommandExecute);
        }

        /// <summary>Calculates the Quotient and Remainder given the Dividend and Divisor.</summary>
        public Command CalculateQuotientCommand { get; private set; }

        private void OnCalculateQuotientCommandExecute()
        {
            if (Math.Abs(Divisor) < 0.0001)
            {
                return;
            }

            Quotient = Math.Floor(Dividend / Divisor);
            Remainder = Dividend % Divisor;

            var dividend = new NumericValueDefinitionTag(Model.ParentPage, Model.Origin)
                           {
                               NumericValue = Dividend
                           };

            var divisor = new NumericValueDefinitionTag(Model.ParentPage, Model.Origin)
                          {
                              NumericValue = Divisor
                          };

            Model.Dividend = dividend;
            Model.Divisor = divisor;
        }

        #endregion // Commands
    }
}