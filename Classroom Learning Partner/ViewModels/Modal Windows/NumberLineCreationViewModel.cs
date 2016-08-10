using Catel.Data;
using Catel.MVVM;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NumberLineCreationViewModel : ViewModelBase
    {
        #region Constructor

        public NumberLineCreationViewModel() { }

        #endregion // Constructor

        #region Bindings

        /// <summary>End length of the number line.</summary>
        public int NumberLineSize
        {
            get { return GetValue<int>(NumberLineSizeProperty); }
            set { SetValue(NumberLineSizeProperty, value); }
        }

        public static readonly PropertyData NumberLineSizeProperty = RegisterProperty("NumberLineSize", typeof(int), 0);

        /// <summary>Is the number line going to use auto arcs.</summary>
        public bool IsUsingAutoArcs
        {
            get { return GetValue<bool>(IsUsingAutoArcsProperty); }
            set { SetValue(IsUsingAutoArcsProperty, value); }
        }

        public static readonly PropertyData IsUsingAutoArcsProperty = RegisterProperty("IsUsingAutoArcs", typeof(bool), false);

        #endregion // Bindings
    }
}