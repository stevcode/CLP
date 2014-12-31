using Catel.Data;
using Catel.MVVM;

namespace Classroom_Learning_Partner.ViewModels
{
    public class TextInputViewModel : ViewModelBase
    {
        public TextInputViewModel() { }

        #region Bindings

        /// <summary>
        /// Text Prompt for the InputText.
        /// </summary>
        public string TextPrompt
        {
            get { return GetValue<string>(TextPromptProperty); }
            set { SetValue(TextPromptProperty, value); }
        }

        public static readonly PropertyData TextPromptProperty = RegisterProperty("TextPrompt", typeof (string), "Input Text");

        /// <summary>
        /// Text input by user and returned.
        /// </summary>
        public string InputText
        {
            get { return GetValue<string>(InputTextProperty); }
            set { SetValue(InputTextProperty, value); }
        }

        public static readonly PropertyData InputTextProperty = RegisterProperty("InputText", typeof (string), string.Empty);

        #endregion //Bindings
    }
}
