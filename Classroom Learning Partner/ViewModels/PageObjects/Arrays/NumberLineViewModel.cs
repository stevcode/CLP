using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Catel.MVVM.Views;
using Classroom_Learning_Partner.Views;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Entities.DomainModel.PageObjects.Arrays;

namespace Classroom_Learning_Partner.ViewModels.PageObjects.Arrays
{
    public class NumberLineViewModel : APageObjectBaseViewModel
    {
        #region

        public NumberLineViewModel(NumberLine numberLine)
        {
            PageObject = numberLine;

            //Commands
            ShowKeyPadCommand = new Command(OnShowKeyPadCommandExecute);
        }

        #endregion //Constructor

        #region Model

        [ViewModelToModel("Pagebject")]
        public int NumberLength
        {
            get { return GetValue<int>(NumberLengthProperty); }
            set { SetValue(NumberLengthProperty, value); }
        }

        public static readonly PropertyData NumberLengthProperty = RegisterProperty("NumberLength", typeof (int));

        #endregion //Model

        #region Commands

        public Command ShowKeyPadCommand { get; private set; }

        private void OnShowKeyPadCommandExecute()
        {
            var numberLine = PageObject as NumberLine;
            if (numberLine == null ||
                (!App.MainWindowViewModel.IsAuthoring))
            {
                return;
            }

            var keyPad = new KeypadWindowView("How long would you want the number line?", 100)
                         {
                             Owner = Application.Current.MainWindow,
                             WindowStartupLocation = WindowStartupLocation.Manual,
                             Top = 100,
                             Left = 100
                         };
            keyPad.ShowDialog();
            if (keyPad.DialogResult != true ||
                keyPad.NumbersEntered.Text.Length <= 0)
            {
                return;
            }

            var numberLineLength = Int32.Parse(keyPad.NumbersEntered.Text);

        }

        #endregion //Commands
       


    }
}
