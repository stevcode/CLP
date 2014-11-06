using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPTextBoxViewModel : APageObjectBaseViewModel
    {
        public CLPTextBoxViewModel(CLPTextBox textBox) { PageObject = textBox; }

        public override string Title
        {
            get { return "TextBoxVM"; }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public string Text
        {
            get { return GetValue<string>(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly PropertyData TextProperty = RegisterProperty("Text", typeof(string));

        #region Static Methods

        public static void AddTextBoxToPage(CLPPage page)
        {
            var textBox = new CLPTextBox(page, string.Empty);
            ACLPPageBaseViewModel.AddPageObjectToPage(textBox);
        }

        #endregion //Static Methods
    }
}