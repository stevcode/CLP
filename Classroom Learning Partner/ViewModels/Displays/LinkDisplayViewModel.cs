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
        public LinkedDisplayViewModel(CLPPageViewModel page)
            : base()
        {
            DisplayedPage = page;
            Console.WriteLine(Title + " created with pageVM" + DisplayedPage.Page.UniqueID);
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CLPPageViewModel DisplayedPage
        {
            get { return GetValue<CLPPageViewModel>(DisplayedPageProperty); }
            set { SetValue(DisplayedPageProperty, value);
            Console.WriteLine("DisplayPage changed to pageVM" + DisplayedPage.Page.UniqueID);
            }
        }

        /// <summary>
        /// Register the DisplayedPage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DisplayedPageProperty = RegisterProperty("DisplayedPage", typeof(CLPPageViewModel));

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


        public void AddPageToDisplay(CLPPageViewModel page)
        {
            DisplayedPage = page;
        }

    }
}