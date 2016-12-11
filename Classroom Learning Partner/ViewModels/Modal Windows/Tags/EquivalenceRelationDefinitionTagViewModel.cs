using System;
using System.Linq;
using System.Windows;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class EquivalenceRelationDefinitionTagViewModel : ViewModelBase
    {
        public enum RelationTypes
        {
            Addition,
            Multiplication,
            Division
        }

        #region Constructor

        /// <summary>Initializes a new instance of the <see cref="EquivalenceRelationDefinitionTagViewModel" /> class.</summary>
        public EquivalenceRelationDefinitionTagViewModel(EquivalenceRelationDefinitionTag equivalenceRelationDefinition)
        {
            DefinitionTag = equivalenceRelationDefinition;

            InitializeCommands();
        }

        #endregion //Constructor

        #region Model

        /// <summary>Model of this ViewModel.</summary>
        [Model(SupportIEditableObject = false)]
        public EquivalenceRelationDefinitionTag DefinitionTag
        {
            get { return GetValue<EquivalenceRelationDefinitionTag>(DefinitionTagProperty); }
            set { SetValue(DefinitionTagProperty, value); }
        }

        public static readonly PropertyData DefinitionTagProperty = RegisterProperty("DefinitionTag", typeof(EquivalenceRelationDefinitionTag));
        
        #endregion // Model

        #region Bindings

        /// <summary>The left relation type.</summary>
        public RelationTypes LeftRelationType
        {
            get { return GetValue<RelationTypes>(LeftRelationTypeProperty); }
            set { SetValue(LeftRelationTypeProperty, value); }
        }

        public static readonly PropertyData LeftRelationTypeProperty = RegisterProperty("LeftRelationType", typeof(RelationTypes), RelationTypes.Addition);

        /// <summary>The right relation type.</summary>
        public RelationTypes RightRelationType
        {
            get { return GetValue<RelationTypes>(RightRelationTypeProperty); }
            set { SetValue(RightRelationTypeProperty, value); }
        }

        public static readonly PropertyData RightRelationTypeProperty = RegisterProperty("RightRelationType", typeof(RelationTypes), RelationTypes.Addition);

        /// <summary>First number in the left side.</summary>
        public int FirstLeftNumericValue
        {
            get { return GetValue<int>(FirstLeftNumericValueProperty); }
            set { SetValue(FirstLeftNumericValueProperty, value); }
        }

        public static readonly PropertyData FirstLeftNumericValueProperty = RegisterProperty("FirstLeftNumericValue", typeof(int));

        /// <summary>Signifies selected value is not a given value.</summary>
        public bool? IsFirstLeftUnknown
        {
            get { return GetValue<bool?>(IsFirstLeftUnknownProperty); }
            set { SetValue(IsFirstLeftUnknownProperty, value); }
        }

        public static readonly PropertyData IsFirstLeftUnknownProperty = RegisterProperty("IsFirstLeftUnknown", typeof(bool?), false);

        /// <summary>Second number in the left side.</summary>
        public int SecondLeftNumericValue
        {
            get { return GetValue<int>(SecondLeftNumericValueProperty); }
            set { SetValue(SecondLeftNumericValueProperty, value); }
        }

        public static readonly PropertyData SecondLeftNumericValueProperty = RegisterProperty("SecondLeftNumericValue", typeof(int));

        /// <summary>Signifies selected value is not a given value.</summary>
        public bool? IsSecondLeftUnknown
        {
            get { return GetValue<bool?>(IsSecondLeftUnknownProperty); }
            set { SetValue(IsSecondLeftUnknownProperty, value); }
        }

        public static readonly PropertyData IsSecondLeftUnknownProperty = RegisterProperty("IsSecondLeftUnknown", typeof(bool?), false);

        /// <summary>First number in the right side.</summary>
        public int FirstRightNumericValue
        {
            get { return GetValue<int>(FirstRightNumericValueProperty); }
            set { SetValue(FirstRightNumericValueProperty, value); }
        }

        public static readonly PropertyData FirstRightNumericValueProperty = RegisterProperty("FirstRightNumericValue", typeof(int));

        /// <summary>Signifies selected value is not a given value.</summary>
        public bool? IsFirstRightUnknown
        {
            get { return GetValue<bool?>(IsFirstRightUnknownProperty); }
            set { SetValue(IsFirstRightUnknownProperty, value); }
        }

        public static readonly PropertyData IsFirstRightUnknownProperty = RegisterProperty("IsFirstRightUnknown", typeof(bool?), false);

        /// <summary>Second number in the right side.</summary>
        public int SecondRightNumericValue
        {
            get { return GetValue<int>(SecondRightNumericValueProperty); }
            set { SetValue(SecondRightNumericValueProperty, value); }
        }

        public static readonly PropertyData SecondRightNumericValueProperty = RegisterProperty("SecondRightNumericValue", typeof(int));

        /// <summary>Signifies selected value is not a given value.</summary>
        public bool? IsSecondRightUnknown
        {
            get { return GetValue<bool?>(IsSecondRightUnknownProperty); }
            set { SetValue(IsSecondRightUnknownProperty, value); }
        }

        public static readonly PropertyData IsSecondRightUnknownProperty = RegisterProperty("IsSecondRightUnknown", typeof(bool?), false);

        /// <summary>Calculated equivalence of the 2 sides.</summary>
        public int? CalculatedEquivalence
        {
            get { return GetValue<int?>(CalculatedEquivalenceProperty); }
            set { SetValue(CalculatedEquivalenceProperty, value); }
        }

        public static readonly PropertyData CalculatedEquivalenceProperty = RegisterProperty("CalculatedEquivalence", typeof(int?));

        #endregion // Bindings

        #region Commands

        private bool _isCalculated;

        private void InitializeCommands()
        {
            CalculateEquivalenceCommand = new Command(OnCalculateEquivalenceCommandExecute);
            ConfirmChangesCommand = new Command(OnConfirmChangesCommandExecute);
            CancelChangesCommand = new Command(OnCancelChangesCommandExecute);
        }

        /// <summary>Validates the equivalence of the left and right sides.</summary>
        public Command CalculateEquivalenceCommand { get; private set; }

        private void OnCalculateEquivalenceCommandExecute()
        {
            var firstLeftNumericPart = new NumericValueDefinitionTag(DefinitionTag.ParentPage, DefinitionTag.Origin)
                                       {
                                           NumericValue = FirstLeftNumericValue,
                                           IsNotGiven = IsFirstLeftUnknown.Value
                                       };

            var secondLeftNumericPart = new NumericValueDefinitionTag(DefinitionTag.ParentPage, DefinitionTag.Origin)
                                        {
                                            NumericValue = SecondLeftNumericValue,
                                            IsNotGiven = IsSecondLeftUnknown.Value
                                        };

            IRelationPart leftRelationPart;
            switch (LeftRelationType)
            {
                case RelationTypes.Addition:
                    var additionRelationPart = new AdditionRelationDefinitionTag(DefinitionTag.ParentPage, DefinitionTag.Origin);
                    additionRelationPart.Addends.Add(firstLeftNumericPart);
                    additionRelationPart.Addends.Add(secondLeftNumericPart);
                    additionRelationPart.Sum = additionRelationPart.Addends.Select(x => x.RelationPartAnswerValue).Aggregate((x, y) => x + y);
                    leftRelationPart = additionRelationPart;
                    break;
                case RelationTypes.Multiplication:
                    var multiplicationRelationPart = new MultiplicationRelationDefinitionTag(DefinitionTag.ParentPage, DefinitionTag.Origin);
                    multiplicationRelationPart.Factors.Add(firstLeftNumericPart);
                    multiplicationRelationPart.Factors.Add(secondLeftNumericPart);
                    multiplicationRelationPart.Product = multiplicationRelationPart.Factors.Select(x => x.RelationPartAnswerValue).Aggregate((x, y) => x * y);
                    leftRelationPart = multiplicationRelationPart;
                    break;
                case RelationTypes.Division:
                    var divisionRelationPart = new DivisionRelationDefinitionTag(DefinitionTag.ParentPage, DefinitionTag.Origin);
                    divisionRelationPart.Dividend = firstLeftNumericPart;
                    divisionRelationPart.Divisor = secondLeftNumericPart;
                    divisionRelationPart.Quotient = Math.Floor(divisionRelationPart.Dividend.RelationPartAnswerValue / divisionRelationPart.Divisor.RelationPartAnswerValue);
                    divisionRelationPart.Remainder = divisionRelationPart.Dividend.RelationPartAnswerValue % divisionRelationPart.Divisor.RelationPartAnswerValue;
                    leftRelationPart = divisionRelationPart;
                    break;
                default:
                    return;
            }

            var firstRightNumericPart = new NumericValueDefinitionTag(DefinitionTag.ParentPage, DefinitionTag.Origin)
            {
                NumericValue = FirstRightNumericValue,
                IsNotGiven = IsFirstRightUnknown.Value
            };

            var secondRightNumericPart = new NumericValueDefinitionTag(DefinitionTag.ParentPage, DefinitionTag.Origin)
            {
                NumericValue = SecondRightNumericValue,
                IsNotGiven = IsSecondRightUnknown.Value
            };

            IRelationPart rightRelationPart;
            switch (RightRelationType)
            {
                case RelationTypes.Addition:
                    var additionRelationPart = new AdditionRelationDefinitionTag(DefinitionTag.ParentPage, DefinitionTag.Origin);
                    additionRelationPart.Addends.Add(firstRightNumericPart);
                    additionRelationPart.Addends.Add(secondRightNumericPart);
                    additionRelationPart.Sum = additionRelationPart.Addends.Select(x => x.RelationPartAnswerValue).Aggregate((x, y) => x + y);
                    rightRelationPart = additionRelationPart;
                    break;
                case RelationTypes.Multiplication:
                    var multiplicationRelationPart = new MultiplicationRelationDefinitionTag(DefinitionTag.ParentPage, DefinitionTag.Origin);
                    multiplicationRelationPart.Factors.Add(firstRightNumericPart);
                    multiplicationRelationPart.Factors.Add(secondRightNumericPart);
                    multiplicationRelationPart.Product = multiplicationRelationPart.Factors.Select(x => x.RelationPartAnswerValue).Aggregate((x, y) => x * y);
                    rightRelationPart = multiplicationRelationPart;
                    break;
                case RelationTypes.Division:
                    var divisionRelationPart = new DivisionRelationDefinitionTag(DefinitionTag.ParentPage, DefinitionTag.Origin);
                    divisionRelationPart.Dividend = firstRightNumericPart;
                    divisionRelationPart.Divisor = secondRightNumericPart;
                    divisionRelationPart.Quotient = Math.Floor(divisionRelationPart.Dividend.RelationPartAnswerValue / divisionRelationPart.Divisor.RelationPartAnswerValue);
                    divisionRelationPart.Remainder = divisionRelationPart.Dividend.RelationPartAnswerValue % divisionRelationPart.Divisor.RelationPartAnswerValue;
                    rightRelationPart = divisionRelationPart;
                    break;
                default:
                    return;
            }

            if (Math.Abs(leftRelationPart.RelationPartAnswerValue - rightRelationPart.RelationPartAnswerValue) > 0.0001)
            {
                MessageBox.Show("Equations are not equivalent.");
                CalculatedEquivalence = null;
                _isCalculated = false;
                return;
            }

            var trueCount = 0;
            if (IsFirstLeftUnknown.Value)
            {
                trueCount++;
            }
            if (IsSecondLeftUnknown.Value)
            {
                trueCount++;
            }
            if (IsFirstRightUnknown.Value)
            {
                trueCount++;
            }
            if (IsSecondRightUnknown.Value)
            {
                trueCount++;
            }

            if (trueCount > 1)
            {
                MessageBox.Show("Only one number can be checked as Unknown.");
                CalculatedEquivalence = null;
                _isCalculated = false;
                return;
            }

            CalculatedEquivalence = (int)leftRelationPart.RelationPartAnswerValue;
            DefinitionTag.LeftRelationPart = leftRelationPart;
            DefinitionTag.RightRelationPart = rightRelationPart;

            _isCalculated = true;
        }

        /// <summary>Validates and confirms changes to the person.</summary>
        public Command ConfirmChangesCommand { get; private set; }

        private async void OnConfirmChangesCommandExecute()
        {
            OnCalculateEquivalenceCommandExecute();

            if (!_isCalculated)
            {
                return;
            }

            await CloseViewModelAsync(true);
        }

        /// <summary>Cancels changes to the person.</summary>
        public Command CancelChangesCommand { get; private set; }

        private async void OnCancelChangesCommandExecute()
        {
            await CloseViewModelAsync(false);
        }

        #endregion // Commands
    }
}