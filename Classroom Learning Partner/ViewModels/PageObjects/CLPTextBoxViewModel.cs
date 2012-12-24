using System.Windows;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPTextBoxViewModel : ACLPPageObjectBaseViewModel
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

        public static readonly PropertyData TextProperty = RegisterProperty("Text", typeof(string));

        public override bool SetInkCanvasHitTestVisibility(string hitBoxTag, string hitBoxName, bool isInkCanvasHitTestVisibile, bool isMouseDown, bool isTouchDown, bool isPenDown)
        {
            if(IsBackground && App.MainWindowViewModel.IsAuthoring)
            {
                IsMouseOverShowEnabled = true;
                return false;
            }
            else
            {
                IsMouseOverShowEnabled = false;
                return true;
            }
        }
    }
}
