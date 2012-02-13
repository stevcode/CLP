using Classroom_Learning_Partner.Model;
using Catel.MVVM;
using Catel.Data;
using System;

namespace Classroom_Learning_Partner.ViewModels.Displays
{
    public class LinkedDisplayViewModel : ViewModelBase, IDisplayViewModel
    {
        /// <summary>
        /// Initializes a new instance of the LinkedDisplayViewModel class.
        /// </summary>
        public LinkedDisplayViewModel(CLPPage page)
            : base()
        {
            DisplayedPage = page;
            Console.WriteLine("linked display created, isactive: " + IsActive.ToString());
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [Model]
        public CLPPage DisplayedPage
        {
            get { return GetValue<CLPPage>(DisplayedPageProperty); }
            set { SetValue(DisplayedPageProperty, value); }
        }

        /// <summary>
        /// Register the DisplayedPage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DisplayedPageProperty = RegisterProperty("DisplayedPage", typeof(CLPPage));

        public string DisplayName
        {
            get { return "LinkedDisplay"; }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsActive
        {
            get { return GetValue<bool>(IsActiveProperty); }
            set
            {
                SetValue(IsActiveProperty, value);
                Console.WriteLine("linkeddisplay IsActive set to: " + value.ToString());
            }
        }

        /// <summary>
        /// Register the IsActive property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsActiveProperty = RegisterProperty("IsActive", typeof(bool));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsOnProjector
        {
            get { return GetValue<bool>(IsOnProjectorProperty); }
            set { SetValue(IsOnProjectorProperty, value); }
        }

        /// <summary>
        /// Register the IsOnProjector property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsOnProjectorProperty = RegisterProperty("IsOnProjector", typeof(bool));

        public override string Title { get { return "LinkDisplayVM"; } }


        public void AddPageToDisplay(CLPPage page)
        {
            DisplayedPage = page;
        }

    }
}