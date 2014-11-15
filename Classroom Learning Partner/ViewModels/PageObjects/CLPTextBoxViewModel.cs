using Catel.Data;
using Catel.MVVM;
using CLP.CustomControls;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPTextBoxViewModel : APageObjectBaseViewModel
    {
        public CLPTextBoxViewModel(CLPTextBox textBox)
        {
            PageObject = textBox; 
            InitializeButtons();
        }

        private void InitializeButtons()
        {
            // Bold, Italics, Underline
            _contextButtons.Add(MajorRibbonViewModel.Separater);
            var toggleBoldButton = new ToggleRibbonButton(string.Empty, string.Empty, "pack://application:,,,/Images/Bold16.png", true)
            {
                IsChecked = false
            };
            //toggleBoldButton.Checked += jumpSizeVisibility_Checked;
            //toggleBoldButton.Unchecked += jumpSizeVisibility_Checked;
            _contextButtons.Add(toggleBoldButton);

            var toggleItalicsButton = new ToggleRibbonButton(string.Empty, string.Empty, "pack://application:,,,/Images/Italic16.png", true)
            {
                IsChecked = false
            };
            //toggleItalicsButton.Checked += allowDragging_Checked;
            //toggleItalicsButton.Unchecked += allowDragging_Checked;
            _contextButtons.Add(toggleItalicsButton);

            var toggleUnderlineButton = new ToggleRibbonButton(string.Empty, string.Empty, "pack://application:,,,/Images/Underline16.png", true)
            {
                IsChecked = false
            };
            //toggleUnderlineButton.Checked += allowDragging_Checked;
            //toggleUnderlineButton.Unchecked += allowDragging_Checked;
            _contextButtons.Add(toggleUnderlineButton);

            // Font Family, Size, Color
            _contextButtons.Add(MajorRibbonViewModel.Separater);
        }

        public override string Title
        {
            get { return "TextBoxVM"; }
        }

        #region Model

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

        #endregion //Model

        #region Static Methods

        public static void AddTextBoxToPage(CLPPage page)
        {
            var textBox = new CLPTextBox(page, string.Empty);
            ACLPPageBaseViewModel.AddPageObjectToPage(textBox);
        }

        #endregion //Static Methods
    }
}