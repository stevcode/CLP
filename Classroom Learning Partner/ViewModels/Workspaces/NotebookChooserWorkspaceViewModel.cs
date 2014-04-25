using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Documents;
using Catel.Collections;
using Catel.Data;
using Catel.MVVM;
using Catel.Windows;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NotebookChooserWorkspaceViewModel : ViewModelBase
    {
        public class NotebookName
        {
            public string Name { get; set; }
            public string OwnerID { get; set; }
            public string OwnerName { get; set; }
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
            var notebookNames = (from localName in localNames
                                 select localName.Split(';')
                                 into nameAndID
                                 where nameAndID.Length == 4
                                 select new NotebookName
                                        {
                                            Name = nameAndID[0],
                                            ID = nameAndID[1],
                                            OwnerName = nameAndID[2],
                                            OwnerID = nameAndID[3],
                                            IsLocal = true
                                        }).ToList();
            var authorNotebookName = notebookNames.FirstOrDefault(x => x.OwnerID == Person.Author.ID);
            var emilyNotebookName = notebookNames.FirstOrDefault(x => x.OwnerID == Person.Emily.ID);
            notebookNames.RemoveAll(x => x.OwnerID == Person.Emily.ID || x.OwnerID == Person.Author.ID);
            var sortedNotebookNames = notebookNames.OrderBy(x => x.OwnerName);
            if(authorNotebookName != null)
            {
                NotebookNames.Add(authorNotebookName);
            }
            if(emilyNotebookName != null)
            {
                NotebookNames.Add(emilyNotebookName);
            }
            NotebookNames.AddRange(sortedNotebookNames);
        }

        public override string Title
        {
            get { return "NotebookChooserWorkspaceVM"; }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
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
            PleaseWaitHelper.Show(() => MainWindowViewModel.OpenNotebook(notebookName.Name + ";" + notebookName.ID + ";" + notebookName.OwnerName + ";" + notebookName.OwnerID), null, "Loading Notebook");
        }
    }
}