using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPFuzzyFactorCard : CLPFactorCard
    {

        #region Constructors

        public CLPFuzzyFactorCard(int rows, int columns, int dividend, ICLPPage page)
            : base(rows, columns, page)
        {
            Dividend = dividend;
            CurrentRemainder = dividend;
            IsGridOn = rows < 45 && columns < 45;
            IsAnswerVisible = true;
        }
        
        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPFuzzyFactorCard(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties
        public override string PageObjectType
        {
            get { return "CLPFuzzyFactorCard"; }
        }

        /// <summary>
        /// Whether or not the answer is displayed.
        /// </summary>
        public bool IsAnswerVisible
        {
            get
            {
                return GetValue<bool>(IsAnswerVisibleProperty);
            }
            set
            {
                SetValue(IsAnswerVisibleProperty, value);
            }
        }

        /// <summary>
        /// Register the IsAnswerVisible property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsAnswerVisibleProperty = RegisterProperty("IsAnswerVisible", typeof(bool), true);

        /// <summary>
        /// Value of the dividend.
        /// </summary>
        public int Dividend
        {
            get
            {
                return GetValue<int>(DividendProperty);
            }
            set
            {
                SetValue(DividendProperty, value);
            }
        }

        /// <summary>
        /// Register the Dividend property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DividendProperty = RegisterProperty("Dividend", typeof(int), null);

        /// <summary>
        /// The total number of groups (columns or rows) that have been subtracted so far.
        /// </summary>
        public int GroupsSubtracted
        {
            get
            {
                return GetValue<int>(GroupsSubtractedProperty);
            }
            set
            {
                SetValue(GroupsSubtractedProperty, value);
            }
        }

        /// <summary>
        /// Register the GroupsSubtracted property so it is known in the class.
        /// </summary>
        public static readonly PropertyData GroupsSubtractedProperty = RegisterProperty("GroupsSubtracted", typeof(int), 0);

        /// <summary>
        /// The area remaining in the array after subtracting the area of the snapped in arrays.
        /// </summary>
        public int CurrentRemainder
        {
            get
            {
                return GetValue<int>(CurrentRemainderProperty);
            }
            set
            {
                SetValue(CurrentRemainderProperty, value);
            }
        }

        /// <summary>
        /// Register the CurrentRemainder property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CurrentRemainderProperty = RegisterProperty("CurrentRemainder", typeof(int), null);

        /// <summary>
        /// Position of the last division in the FFC.
        /// </summary>
        public double LastDivisionPosition
        {
            get
            {
                return GetValue<double>(LastDivisionPositionProperty);
            }
            set
            {
                SetValue(LastDivisionPositionProperty, value);
            }
        }

        /// <summary>
        /// Register the LastDivisionPosition property so it is known in the class.
        /// </summary>
        public static readonly PropertyData LastDivisionPositionProperty = RegisterProperty("LastDivisionPosition", typeof(double), 0.0);

        #endregion //Properties

        #region Methods

        public void CreateVerticalDivisionAtPosition(double position, int value)
        {
            CLPArrayDivision divAbove = FindDivisionAbove(position, VerticalDivisions);
            CLPArrayDivision divBelow = FindDivisionBelow(position, VerticalDivisions);

            var addedDivisions = new List<CLPArrayDivision>();
            var removedDivisions = new List<CLPArrayDivision>();

            CLPArrayDivision topDiv;
            if(divAbove == null)
            {
                topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, 0, position, value);
            }
            else
            {
                topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, divAbove.Position, position - divAbove.Position, value);
                VerticalDivisions.Remove(divAbove);
                removedDivisions.Add(divAbove);
            }
            VerticalDivisions.Add(topDiv);
            addedDivisions.Add(topDiv);

            CLPArrayDivision bottomDiv;
            if(divBelow == null)
            {
                bottomDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, position, ArrayWidth - position, 0);
            }
            else
            {
                bottomDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, position, divBelow.Position - position, 0);
            }

            VerticalDivisions.Add(bottomDiv);
            addedDivisions.Add(bottomDiv);

            // Update the totals
            GroupsSubtracted += value;
            CurrentRemainder -= (value * Rows);
            LastDivisionPosition = position;
        }

        #endregion //Methods
    }
}
