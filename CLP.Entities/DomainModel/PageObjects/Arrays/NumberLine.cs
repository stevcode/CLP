using System;
using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class NumberLineTick : AEntityBase
    {

        #region Constructors

        public NumberLineTick() { }

        public NumberLineTick(int value, bool isNumberVisible)
        {
            TickValue = value; 
            IsNumberVisible = isNumberVisible; 
        }

        public NumberLineTick(SerializationInfo info, StreamingContext context)
            : base(info, context) { }


        #endregion //Constructors

        #region Properties

        public double TickHeight
        {
            get { return 25.0; }
        }

        /// <summary>
        /// Number of the Tick
        /// </summary>
        public int TickValue
        {
            get { return GetValue<int>(TickValueProperty); }
            set { SetValue(TickValueProperty, value); }
        }

        public static readonly PropertyData TickValueProperty = RegisterProperty("TickValue", typeof (int), 0);

        /// <summary>
        /// Is the tick marks number visible
        /// </summary>
        public bool IsNumberVisible
        {
            get { return GetValue<bool>(IsNumberVisibleProperty); }
            set { SetValue(IsNumberVisibleProperty, value); }
        }

        public static readonly PropertyData IsNumberVisibleProperty = RegisterProperty("IsNumberVisible", typeof (bool), false);

        /// <summary>
        /// Is the tick visible on the number line
        /// </summary>
        public bool IsTickVisible
        {
            get { return GetValue<bool>(IsTickVisibleProperty); }
            set { SetValue(IsTickVisibleProperty, value); }
        }

        public static readonly PropertyData IsTickVisibleProperty = RegisterProperty("IsTickVisible", typeof (bool), true);

        /// <summary>
        /// Has the user marked the tick mark
        /// </summary>
        public bool IsMarked
        {
            get { return GetValue<bool>(IsMarkedProperty); }
            set { SetValue(IsMarkedProperty, value); }
        }

        public static readonly PropertyData IsMarkedProperty = RegisterProperty("IsMarked", typeof (bool), false);

        #endregion //Properties
    }

    [Serializable]
    public class NumberLine : APageObjectBase
    {
        #region Constructors

        public NumberLine() { }

        public NumberLine(CLPPage parentPage, int numberLength)
            : base(parentPage)
        {
            NumberLineSize = numberLength;
            Height = 75;
            Width = 800;
        }

        public NumberLine(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public double ArrowLength
        {
            get { return 40.0; }
        }

        public double NumberLineLength
        {
            get { return Width - ArrowLength * 2; }
        }

        public double TickLength
        {
            get { return NumberLineLength / NumberLineSize; }
        }

        public override bool IsBackgroundInteractable
        {
            get { return true; }
        }

        /// <summary>Length of number line</summary>
        public int NumberLineSize
        {
            get { return GetValue<int>(NumberLineSizeProperty); }
            set { SetValue(NumberLineSizeProperty, value); }
        }

        public static readonly PropertyData NumberLineSizeProperty = RegisterProperty("NumberLineSize", typeof (int), 0, OnNumberLineSizeChanged);

        private static void OnNumberLineSizeChanged(object sender, AdvancedPropertyChangedEventArgs advancedPropertyChangedEventArgs)
        {
            var numberLine = sender as NumberLine;
            if (numberLine == null)
            {
                return;
            }
            numberLine.CreateTicks();
        }

        /// <summary>
        /// A collection of the ticks of the number line
        /// </summary>
        public ObservableCollection<NumberLineTick> Ticks
        {
            get { return GetValue<ObservableCollection<NumberLineTick>>(TicksProperty); }
            set { SetValue(TicksProperty, value); }
        }

        public static readonly PropertyData TicksProperty = RegisterProperty("Ticks", typeof (ObservableCollection<NumberLineTick>), () => new ObservableCollection<NumberLineTick>());

        #endregion //Properties

        #region Methods

        public void CreateTicks()
        {
            Ticks.Clear();
            int defaultInteger = NumberLineSize <= 10 ? 1 : 5;
            for (int i = 0; i <= NumberLineSize; i++)
            {
                bool labelVisible = false;
                if (i==0 || i==NumberLineSize)
                {
                    labelVisible = true;
                }
                else if (i % defaultInteger == 0)
                {
                    labelVisible = true;
                }
                Ticks.Add(new NumberLineTick(i,labelVisible));
            }

        }

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Width")
            {
                RaisePropertyChanged("TickLength");
                RaisePropertyChanged("NumberLineLength");
            }
            base.OnPropertyChanged(e);
        }


        public override IPageObject Duplicate()
        {
            var newNumberLine = Clone() as NumberLine;
            if (newNumberLine == null)
            {
                return null;
            }
            newNumberLine.CreationDate = DateTime.Now;
            newNumberLine.ID = Guid.NewGuid().ToCompactID();
            newNumberLine.VersionIndex = 0;
            newNumberLine.LastVersionIndex = null;
            newNumberLine.ParentPage = ParentPage;

            return newNumberLine;
        }

        #endregion //Methods
    }
}