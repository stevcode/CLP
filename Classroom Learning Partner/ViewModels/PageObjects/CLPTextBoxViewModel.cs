using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Windows.Media;
using Catel.MVVM;
using Catel.Data;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    public class CLPTextBoxViewModel : CLPPageObjectBaseViewModel
    {
        public CLPTextBoxViewModel(CLPTextBox textBox)
            : base()
        {
            PageObject = textBox;
        }

        public override string Title { get { return "TextBoxVM"; } }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public string Text
        {
            get { return GetValue<string>(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Register the Text property so it is known in the class.
        /// </summary>
        public static readonly PropertyData TextProperty = RegisterProperty("Text", typeof(string));
    }
}
