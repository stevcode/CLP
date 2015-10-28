using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using Catel.MVVM;
using Classroom_Learning_Partner.Views;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public enum Panels
    {
        NotebookPages,
        StudentWork,
        Progress,
        Displays,
        PageInformation,
        Webcam
    }

    /// <summary>UserControl view model.</summary>
    public class RibbonViewModel : ViewModelBase
    {
        public MainWindowViewModel MainWindow
        {
            get { return App.MainWindowViewModel; }
        }

        public static CLPPage CurrentPage
        {
            get { return NotebookPagesPanelViewModel.GetCurrentPage(); }
        }

        /// <summary>Initializes a new instance of the <see cref="RibbonViewModel" /> class.</summary>
        public RibbonViewModel()
        {
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            //File Menu
            LogInAsNotebookOwnerCommand = new Command(OnLogInAsNotebookOwnerCommandExecute);
            RefreshNetworkCommand = new Command(OnRefreshNetworkCommandExecute);

            //Insert
            InsertStaticImageCommand = new Command<string>(OnInsertStaticImageCommandExecute);

            SendPageToStudentCommand = new Command(OnSendPageToStudentCommandExecute);
            MakeGroupsCommand = new Command(OnMakeGroupsCommandExecute);
            MakeExitTicketsCommand = new Command(OnMakeExitTicketsCommandExecute);
            MakeExitTicketsFromCurrentPageCommand = new Command(OnMakeExitTicketsFromCurrentPageCommandExecute);
        }

        #region Commands

        #region File Menu

        /// <summary>Sets CurrentUser to the owner of the opened notebook.</summary>
        public Command LogInAsNotebookOwnerCommand { get; private set; }

        private void OnLogInAsNotebookOwnerCommandExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.Workspace as NotebookWorkspaceViewModel;
            if (notebookWorkspaceViewModel == null)
            {
                return;
            }

            MainWindow.CurrentUser = notebookWorkspaceViewModel.Notebook.Owner;
        }

        /// <summary>Reconnects to the network.</summary>
        public Command RefreshNetworkCommand { get; private set; }

        private void OnRefreshNetworkCommandExecute() { CLPServiceAgent.Instance.NetworkReconnect(); }

        #endregion //File Menu

        #region Testing

        public Command MakeGroupsCommand { get; private set; }

        private void OnMakeGroupsCommandExecute()
        {
            //TODO CACHE
            //var groupCreationViewModel = new GroupCreationViewModel();
            //var groupCreationView = new GroupCreationView(groupCreationViewModel);
            //groupCreationView.Owner = Application.Current.MainWindow;
            //groupCreationView.WindowStartupLocation = WindowStartupLocation.Manual;
            //groupCreationView.Top = 0;
            //groupCreationView.ShowDialog();
            //if(groupCreationView.DialogResult == true)
            //{
            //    foreach(var group in groupCreationViewModel.Groups)
            //    {
            //        foreach(Person student in group.Members)
            //        {
            //            if(groupCreationViewModel.GroupType == "Temp")
            //            {
            //                student.TempDifferentiationGroup = group.Label;
            //            }
            //            else
            //            {
            //                student.CurrentDifferentiationGroup = group.Label;
            //            }
            //        }
            //    }
            //    try
            //    {
            //        App.MainWindowViewModel.CurrentClassPeriod.ClassInformation.SaveClassSubject(MainWindowViewModel.ClassCacheDirectory);
            //    }
            //    catch
            //    {
            //        Logger.Instance.WriteToLog("Failed to save class subject after making groups.");
            //    }
            //}
        }

        public Command SendPageToStudentCommand { get; private set; }

        private void OnSendPageToStudentCommandExecute()
        {
            if (!App.MainWindowViewModel.AvailableUsers.Any())
            {
                Logger.Instance.WriteToLog("No Students Found");
                return;
            }

            var studentSelectorViewModel = new StudentSelectorViewModel();
            var studentSelectorView = new StudentSelectorView(studentSelectorViewModel);
            studentSelectorView.Owner = Application.Current.MainWindow;
            studentSelectorView.ShowDialog();
            if (studentSelectorView.DialogResult != true)
            {
                return;
            }

            CurrentPage.History.ClearHistory();
            CurrentPage.SerializedStrokes = StrokeDTO.SaveInkStrokes(CurrentPage.InkStrokes);
            CurrentPage.History.SerializedTrashedInkStrokes = StrokeDTO.SaveInkStrokes(CurrentPage.History.TrashedInkStrokes);
            var serializedCurrentPage = CLPServiceAgent.Instance.Zip(ObjectSerializer.ToString(CurrentPage));
            Parallel.ForEach(studentSelectorViewModel.SelectedStudents,
                             student =>
                             {
                                 try
                                 {
                                     var binding = new NetTcpBinding
                                                   {
                                                       Security =
                                                       {
                                                           Mode = SecurityMode.None
                                                       }
                                                   };
                                     var studentProxy = ChannelFactory<IStudentContract>.CreateChannel(binding, new EndpointAddress(student.CurrentMachineAddress));
                                     studentProxy.AddNewPage(serializedCurrentPage, 999);
                                     (studentProxy as ICommunicationObject).Close();
                                 }
                                 catch (Exception ex)
                                 {
                                     Console.WriteLine(ex.Message);
                                 }
                             });
        }

        public Command MakeExitTicketsFromCurrentPageCommand { get; private set; }

        private void OnMakeExitTicketsFromCurrentPageCommandExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.Workspace as NotebookWorkspaceViewModel;
            if (notebookWorkspaceViewModel == null)
            {
                return;
            }

            var notebook = notebookWorkspaceViewModel.Notebook;

            var exitTicketCreationViewModel = new ExitTicketCreationViewModel(CurrentPage);
            var exitTicketCreationView = new ExitTicketCreationView(exitTicketCreationViewModel);
            exitTicketCreationView.Owner = Application.Current.MainWindow;

            //App.MainWindowViewModel.MajorRibbon.PageInteractionMode = PageInteractionModes.Draw;
            exitTicketCreationView.ShowDialog();
            if (exitTicketCreationView.DialogResult == true)
            {
                SendExitTickets(exitTicketCreationViewModel);
            }
        }

        public Command MakeExitTicketsCommand { get; private set; }

        private void OnMakeExitTicketsCommandExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.Workspace as NotebookWorkspaceViewModel;
            if (notebookWorkspaceViewModel == null)
            {
                return;
            }

            var notebook = notebookWorkspaceViewModel.Notebook;

            var exitTicketCreationViewModel = new ExitTicketCreationViewModel();
            var exitTicketCreationView = new ExitTicketCreationView(exitTicketCreationViewModel);
            exitTicketCreationView.Owner = Application.Current.MainWindow;

            //App.MainWindowViewModel.MajorRibbon.PageInteractionMode = PageInteractionModes.Draw;
            exitTicketCreationView.ShowDialog();
            if (exitTicketCreationView.DialogResult == true)
            {
                SendExitTickets(exitTicketCreationViewModel);
            }
        }

        private void SendExitTickets(ExitTicketCreationViewModel exitTicketCreationViewModel)
        {
            //TODO CACHE
            //var notebookWorkspaceViewModel = MainWindow.Workspace as NotebookWorkspaceViewModel;
            //if(notebookWorkspaceViewModel == null)
            //{
            //    return;
            //}

            //var notebook = notebookWorkspaceViewModel.Notebook;
            //for(int i = 0; i < exitTicketCreationViewModel.ExitTickets.Count; i++)
            //{
            //    var exitTicket = exitTicketCreationViewModel.ExitTickets[i];
            //    notebook.Pages.Add(exitTicket);
            //    exitTicket.History.ClearHistory();
            //    exitTicket.SerializedStrokes = StrokeDTO.SaveInkStrokes(exitTicket.InkStrokes);
            //    exitTicket.History.SerializedTrashedInkStrokes = StrokeDTO.SaveInkStrokes(exitTicket.History.TrashedInkStrokes);

            //    foreach(Person student in exitTicketCreationViewModel.GroupCreationViewModel.Groups[i].Members)
            //    {
            //        student.TempDifferentiationGroup = exitTicket.DifferentiationLevel;
            //    }
            //}
            //try
            //{
            //    App.MainWindowViewModel.CurrentClassPeriod.ClassInformation.SaveClassSubject(MainWindowViewModel.ClassCacheDirectory);
            //}
            //catch
            //{
            //    Logger.Instance.WriteToLog("Failed to save class subject after making exit tickets.");
            //}

            ////send exit tickets to projector
            //if(App.Network.ProjectorProxy != null)
            //{
            //    try
            //    {
            //        foreach(CLPPage exitTicket in exitTicketCreationViewModel.ExitTickets)
            //        {
            //            App.Network.ProjectorProxy.AddNewPage(CLPServiceAgent.Instance.Zip(ObjectSerializer.ToString(exitTicket)), 999);
            //        }
            //    }
            //    catch(Exception)
            //    {
            //    }
            //}

            ////send an exit ticket to each student
            //if(App.MainWindowViewModel.AvailableUsers.Any())
            //{
            //    Parallel.ForEach(App.MainWindowViewModel.AvailableUsers,
            //                        student =>
            //                        {
            //                            try
            //                            {
            //                                var binding = new NetTcpBinding
            //                                {
            //                                    Security =
            //                                    {
            //                                        Mode = SecurityMode.None
            //                                    }
            //                                };
            //                                var studentProxy = ChannelFactory<IStudentContract>.CreateChannel(binding, new EndpointAddress(student.CurrentMachineAddress));
            //                                CLPPage correctExitTicket = exitTicketCreationViewModel.ExitTickets.FirstOrDefault(x => x.DifferentiationLevel == student.TempDifferentiationGroup);
            //                                if(correctExitTicket == null)
            //                                {
            //                                    correctExitTicket = exitTicketCreationViewModel.ExitTickets.First();
            //                                }
            //                                //TODO: The number 999 is used in place of "infinity".
            //                                //Also I'm doing the serialization step per-student instead of per-exit-ticket which'll be somewhat slower.
            //                                studentProxy.AddNewPage(CLPServiceAgent.Instance.Zip(ObjectSerializer.ToString(correctExitTicket)), 999);
            //                                (studentProxy as ICommunicationObject).Close();
            //                            }
            //                            catch(Exception ex)
            //                            {
            //                                Console.WriteLine(ex.Message);
            //                            }
            //                        });
            //}
            //else
            //{
            //    Logger.Instance.WriteToLog("No Students Found");
            //}
        }

        #endregion //Testing

        /// <summary>Gets the InsertStaticImageCommand command.</summary>
        public Command<string> InsertStaticImageCommand { get; private set; }

        private void OnInsertStaticImageCommandExecute(string fileName)
        {
            // TODO: Entities
            //var uri = new Uri("pack://application:,,,/Classroom Learning Partner;component/Images/Money/" + fileName);
            //var info = Application.GetResourceStream(uri);
            //var memoryStream = new MemoryStream();
            //info.Stream.CopyTo(memoryStream);

            //byte[] byteSource = memoryStream.ToArray();

            //var ByteSource = new List<byte>(byteSource);

            //MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            //byte[] hash = md5.ComputeHash(byteSource);
            //string imageID = Convert.ToBase64String(hash);

            //var page = ((MainWindow.Workspace as NotebookWorkspaceViewModel).CurrentDisplay as CLPMirrorDisplay).CurrentPage;

            //if(!page.ImagePool.ContainsKey(imageID))
            //{
            //    page.ImagePool.Add(imageID, ByteSource);
            //}

            //var image = new CLPImage(imageID, page, 10, 10);

            //switch(fileName)
            //{
            //    case "penny.png":
            //        image.Height = 90;
            //        image.Width = 90;
            //        break;
            //    case "dime.png":
            //        image.Height = 80;
            //        image.Width = 80;
            //        break;
            //    case "nickel.png":
            //        image.Height = 100;
            //        image.Width = 100;
            //        break;
            //    case "quarter.png":
            //        image.Height = 120;
            //        image.Width = 120;
            //        break;
            //    default:
            //        image.Height = 128;
            //        image.Width = 300;
            //        break;
            //}

            //ACLPPageBaseViewModel.AddPageObjectToPage(image);
        }

        #endregion //Commands
    }
}