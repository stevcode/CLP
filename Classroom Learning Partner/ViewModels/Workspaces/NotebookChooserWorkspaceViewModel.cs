using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Catel.Data;
using Catel.MVVM;
using Catel.Windows;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NotebookChooserWorkspaceViewModel : ViewModelBase
    {
        public struct NotebookName
        {
            public string Name { get; set; }
            public string ID { get; set; }
            public bool IsLocal { get; set; }
        }

        /// <summary>
        /// Initializes a new instance of the NotebookChooserWorkspaceViewModel class.
        /// </summary>
        public NotebookChooserWorkspaceViewModel()
        {
            SelectNotebookCommand = new Command<NotebookName>(OnSelectNotebookCommandExecute);

            // TODO: DATABASE - inject INotebookService that can grab the available notebook names?

            var localNames = MainWindowViewModel.AvailableLocalNotebookNames;
            foreach(var name in localNames.Select(localName => new NotebookName
                                                               {
                                                                   Name = localName.Split(';')[0],
                                                                   ID = localName.Split(';')[1],
                                                                   IsLocal = true
                                                               }))
            {
                NotebookNames.Add(name);
            }
        }

        public override string Title
        {
            get { return "NotebookChooserWorkspaceVM"; }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        // TODO: DATABASE - Use List of some Class that includes creation date, last saved date, whether it's from Cache or Database
        public ObservableCollection<NotebookName> NotebookNames
        {
            get { return GetValue<ObservableCollection<NotebookName>>(NotebookNamesProperty); }
            set { SetValue(NotebookNamesProperty, value); }
        }

        public static readonly PropertyData NotebookNamesProperty = RegisterProperty("NotebookNames", typeof(ObservableCollection<NotebookName>), () => new ObservableCollection<NotebookName>());

        /// <summary>
        /// Gets the SelectNotebookCommand command.
        /// </summary>
        public Command<NotebookName> SelectNotebookCommand { get; private set; }

        private void OnSelectNotebookCommandExecute(NotebookName notebookName)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            PleaseWaitHelper.Show(() => MainWindowViewModel.OpenNotebook(notebookName.Name + ";" + notebookName.ID), null, "Loading Notebook");
            stopWatch.Stop();
            Logger.Instance.WriteToLog("Time to LOAD notebook (In Seconds): " + stopWatch.ElapsedMilliseconds / 1000.0);
        }
    }
}