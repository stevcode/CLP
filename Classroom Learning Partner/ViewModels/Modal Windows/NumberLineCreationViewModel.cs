using System.Windows;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NumberLineCreationViewModel : ViewModelBase
    {
        #region Constructor

        public NumberLineCreationViewModel()
        {
            InitializeCommands();
        }

        #endregion // Constructor

        #region Bindings

        /// <summary>End point value of the number line.</summary>
        public string NumberLineEndPoint
        {
            get { return GetValue<string>(NumberLineEndPointProperty); }
            set { SetValue(NumberLineEndPointProperty, value); }
        }

        public static readonly PropertyData NumberLineEndPointProperty = RegisterProperty("NumberLineEndPoint", typeof(string), string.Empty);

        /// <summary>Is the number line going to use auto arcs.</summary>
        public bool IsUsingAutoArcs
        {
            get { return GetValue<bool>(IsUsingAutoArcsProperty); }
            set { SetValue(IsUsingAutoArcsProperty, value); }
        }

        public static readonly PropertyData IsUsingAutoArcsProperty = RegisterProperty("IsUsingAutoArcs", typeof(bool), false);

        #endregion // Bindings

        #region Commands

        private void InitializeCommands()
        {
            NumberPressCommand = new Command<string>(OnNumberPressCommandExecute);
            BackspacePressCommand = new Command(OnBackspacePressCommandExecute);
            CreateNumberLineCommand = new Command(OnCreateNumberLineCommandExecute);
            CancelCreationCommand = new Command(OnCancelCreationCommandExecute);
        }

        /// <summary>Adds digit to the endpoint value.</summary>
        public Command<string> NumberPressCommand { get; private set; }

        private void OnNumberPressCommandExecute(string numberValue)
        {
            NumberLineEndPoint += numberValue;
        }

        /// <summary>Removed digit from the endpoint value.</summary>
        public Command BackspacePressCommand { get; private set; }

        private void OnBackspacePressCommandExecute()
        {
            if (NumberLineEndPoint.Length > 0)
            {
                NumberLineEndPoint = NumberLineEndPoint.Substring(0, NumberLineEndPoint.Length - 1);
            }
        }

        /// <summary>Validates and then creates the number line.</summary>
        public Command CreateNumberLineCommand { get; private set; }

        private async void OnCreateNumberLineCommandExecute()
        {
            int partNum;
            var isNum = int.TryParse(NumberLineEndPoint, out partNum);
            if (NumberLineEndPoint.Length > 0 &&
                isNum &&
                partNum <= NumberLine.NUMBER_LINE_MAX_SIZE)
            {
                await CloseViewModelAsync(true);
            }
            else
            {
                const int LIMIT = NumberLine.NUMBER_LINE_MAX_SIZE + 1;
                MessageBox.Show("You need to end at a number less than " + LIMIT, "Oops");
            }
        }

        /// <summary>Cancels creation of the number line.</summary>
        public Command CancelCreationCommand { get; private set; }

        private async void OnCancelCreationCommandExecute()
        {
            await CloseViewModelAsync(false);
        }

        #endregion // Commands
    }
}