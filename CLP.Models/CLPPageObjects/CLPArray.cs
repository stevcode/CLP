using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPArray : CLPPageObjectBase
    {

        #region Constructors

        public CLPArray(int rows, int columns, CLPPage page)
            : base(page)
        {
            Rows = rows;
            Columns = columns;

            double Ratio = ((double)rows) / ((double)columns);

            XPosition = 10;
            YPosition = 10;
            
            Height = 700*Ratio;
            Width = 700;

            if(Height + YPosition > page.PageHeight - 150)
            {
                Height = page.PageHeight - YPosition - 150;
                Width = Height * ((double)Columns) / ((double)Rows);
            }

            double SquareSize = Width/columns;
            for(int i = 1; i < rows; i++)
            {
                HorizontalGridLines.Add(i * SquareSize);
            }
            for(int i = 1; i < columns; i++)
            {
                VerticalGridLines.Add(i * SquareSize);
            }


            ParentPage = page;
            CreationDate = DateTime.Now;
            UniqueID = Guid.NewGuid().ToString();
            CanAcceptStrokes = true;

            ApplyDistinctPosition(this);
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPArray(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override string PageObjectType
        {
            get { return "CLPArray"; }
        }

        /// <summary>
        /// Turns the grid on or off.
        /// </summary>
        public bool IsGridOn
        {
            get { return GetValue<bool>(IsGridOnProperty); }
            set { SetValue(IsGridOnProperty, value); }
        }

        public static readonly PropertyData IsGridOnProperty = RegisterProperty("IsGridOn", typeof(bool), false);

        /// <summary>
        /// Turns the division behavior on or off.
        /// </summary>
        public bool IsDivisionBehaviorOn
        {
            get { return GetValue<bool>(IsDivisionBehaviorOnProperty); }
            set { SetValue(IsDivisionBehaviorOnProperty, value); }
        }

        public static readonly PropertyData IsDivisionBehaviorOnProperty = RegisterProperty("IsDivisionBehaviorOn", typeof(bool), true);

        /// <summary>
        /// The number of rows in the array.
        /// </summary>
        public int Rows
        {
            get { return GetValue<int>(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public static readonly PropertyData RowsProperty = RegisterProperty("Rows", typeof(int), 1);

        /// <summary>
        /// The number of columns in the array.
        /// </summary>
        public int Columns
        {
            get { return GetValue<int>(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly PropertyData ColumnsProperty = RegisterProperty("Columns", typeof(int), 1);

        /// <summary>
        /// Gets or sets the HorizontalGridDivs value.
        /// </summary>
        public ObservableCollection<double> HorizontalGridLines
        {
            get { return GetValue<ObservableCollection<double>>(HorizontalGridLinesProperty); }
            set { SetValue(HorizontalGridLinesProperty, value); }
        }

        public static readonly PropertyData HorizontalGridLinesProperty = RegisterProperty("HorizontalGridLines", typeof(ObservableCollection<double>), () => new ObservableCollection<double>());

        /// <summary>
        /// Gets or sets the VerticalGridLines value.
        /// </summary>
        public ObservableCollection<double> VerticalGridLines
        {
            get { return GetValue<ObservableCollection<double>>(VerticalGridLinesProperty); }
            set { SetValue(VerticalGridLinesProperty, value); }
        }

        public static readonly PropertyData VerticalGridLinesProperty = RegisterProperty("VerticalGridLines", typeof(ObservableCollection<double>), () => new ObservableCollection<double>());

        #endregion //Properties

        #region Methods

        public override ICLPPageObject Duplicate()
        {
            CLPArray newArray = Clone() as CLPArray;
            newArray.UniqueID = Guid.NewGuid().ToString();
            newArray.ParentPage = ParentPage;
            return newArray;
        }

        #endregion //Methods
    }
}