using System;
using System.Drawing;
using Catel.IoC;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.Entities.Ann;

namespace Classroom_Learning_Partner.ViewModels
{
    public class OptionsPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public OptionsPaneViewModel() { InitializeCommands(); }

        private void InitializeCommands()
        {
            GenerateRandomMainColorCommand = new Command(OnGenerateRandomMainColorCommandExecute);
            ApplyRenameToCacheCommand = new Command(OnApplyRenameToCacheCommandExecute);
        }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText
        {
            get { return "Options"; }
        }

        #endregion //Bindings

        #region Commands

        /// <summary>Sets the DynamicMainColor of the program to a random color.</summary>
        public Command GenerateRandomMainColorCommand { get; private set; }

        private void OnGenerateRandomMainColorCommandExecute()
        {
            var randomGen = new Random();
            var names = (KnownColor[])Enum.GetValues(typeof (KnownColor));
            var randomColorName = names[randomGen.Next(names.Length)];
            var color = Color.FromKnownColor(randomColorName);
            MainWindowViewModel.ChangeApplicationMainColor(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        /// <summary>
        /// Completely renames a Person throughout the entire cache.
        /// </summary>
        public Command ApplyRenameToCacheCommand { get; private set; }

        private void OnApplyRenameToCacheCommandExecute()
        {
            var notebookService = DependencyResolver.Resolve<INotebookService>();
            if (notebookService == null)
            {
                return;
            }

            const string NEW_NAME = "Elvis Garzona";
            App.MainWindowViewModel.CurrentUser.FullName = NEW_NAME;
            var newPerson = App.MainWindowViewModel.CurrentUser;
            var notebook = notebookService.CurrentNotebook;
            notebook.Owner = newPerson;
            foreach (var page in notebook.Pages)
            {
                page.Owner = newPerson;
                foreach (var submission in page.Submissions)
                {
                    submission.Owner = newPerson;
                }
            }
        }

        #endregion //Commands
    }
}