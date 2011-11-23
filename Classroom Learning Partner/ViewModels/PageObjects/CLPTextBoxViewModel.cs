using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Windows.Media;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    public class CLPTextBoxViewModel : CLPPageObjectBaseViewModel
    {
        public CLPTextBoxViewModel(CLPTextBox textBox)
        {
            PageObject = textBox;
            _CLPText = textBox.Text;
        }

        /// <summary>
        /// The <see cref="CLPText" /> property's name.
        /// </summary>
        public const string CLPTextPropertyName = "CLPText";

        private string _CLPText;

        /// <summary>
        /// Sets and gets the CLPText property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string CLPText
        {
            get
            {
                return _CLPText;
            }

            set
            {
                if (_CLPText == value)
                {
                    return;
                }

                _CLPText = value;
                (PageObject as CLPTextBox).Text = value;
                RaisePropertyChanged(CLPTextPropertyName);
            }
        }
    }
}
