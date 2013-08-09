﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using CLP.Models;

namespace Classroom_Learning_Partner
{
    [ServiceContract]
    public interface IInstructorContract
    {
        [OperationContract]
        void AddStudentSubmission(List<StrokeDTO> serializedStrokes,
            List<ICLPPageObject> pageObjects, 
            Person submitter, Group groupSubmitter,
            string notebookID, string pageID, string submissionID, DateTime submissionTime,
            bool isGroupSubmission, double pageHeight);

        [OperationContract]
        void AddSerializedSubmission(string sPage, Person submitter, Group groupSubmitter, 
            DateTime submissionTime, bool isGroupSubmission, String notebookID, String submissionID);

        [OperationContract]
        void CollectStudentNotebook(string sNotebook, string studentName);

        [OperationContract]
        void StudentLogin(Person student);

        [OperationContract]
        void StudentLogout(Person student);
    }

    public class InstructorService : IInstructorContract
    {
        #region IInstructorContract Members

        public void AddStudentSubmission(List<StrokeDTO> serializedStrokes, 
            List<ICLPPageObject> pageObjects, 
            Person submitter, Group groupSubmitter, 
            string notebookID, string pageID, string submissionID, DateTime submissionTime,
            bool isGroupSubmission, double pageHeight)
        {
            if(App.Network.ProjectorProxy != null)
            {
                var t = new Thread(() =>
                {
                    try
                    {
                        App.Network.ProjectorProxy.AddStudentSubmission(serializedStrokes, pageObjects,
                            submitter, groupSubmitter,
                            notebookID, pageID, submissionID, submissionTime,
                            isGroupSubmission, pageHeight);
                    }
                    catch(Exception ex)
                    {
                        Logger.Instance.WriteToLog("Submit to Projector Error: " + ex.Message);
                    }
                }) {IsBackground = true};
                t.Start();
            }
            //TODO: Steve - add pages to a queue and send when a projector is found

            ICLPPage submission = null;
            CLPNotebook currentNotebook = null;

            foreach(var notebook in App.MainWindowViewModel.OpenNotebooks.Where(notebook => notebookID == notebook.UniqueID)) 
            {
                currentNotebook = notebook;
                var page = notebook.GetNotebookPageByID(pageID);
                if(page is CLPPage)
                {
                    submission = (page as CLPPage).Clone() as CLPPage;
                    break;
                }
                if(page is CLPAnimationPage)
                {
                    submission = (page as CLPAnimationPage).Clone() as CLPAnimationPage;
                    break;
                }
            }

            if(submission != null)
            {
                submission.SerializedStrokes = new ObservableCollection<StrokeDTO>(serializedStrokes);
                submission.InkStrokes = StrokeDTO.LoadInkStrokes(submission.SerializedStrokes);

                submission.SubmissionType = isGroupSubmission ? SubmissionType.Group : SubmissionType.Single;
                submission.SubmissionID = submissionID;
                submission.SubmissionTime = submissionTime;
                submission.Submitter = submitter;
                submission.GroupSubmitter = groupSubmitter;
                submission.PageHeight = pageHeight;

                foreach(ICLPPageObject pageObject in pageObjects)
                {
                    submission.PageObjects.Add(pageObject);
                }

                foreach(ICLPPageObject pageObject in submission.PageObjects)
                {
                    pageObject.ParentPage = submission;
                    if(pageObject is ISubmittable)
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (DispatcherOperationCallback)delegate(object arg)
                            {
                                (pageObject as ISubmittable).AfterSubmit(isGroupSubmission, currentNotebook);
                                return null;
                            }, null);
                    }
                }

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    try
                    {
                        CLPServiceAgent.Instance.AddSubmission(currentNotebook, submission);
                        //CLPServiceAgent.Instance.QuickSaveNotebook("RECIEVE-" + userName);
                    }
                    catch(Exception e)
                    {
                        Logger.Instance.WriteToLog("[ERROR] Recieved Submission from wrong notebook: " + e.Message);
                    }

                    return null;
                }, null);
            }

            //CLPServiceAgent.Instance.QuickSaveNotebook("RECIEVE-" + userName);
        }

        public void AddSerializedSubmission(string sPage, Person submitter, Group groupSubmitter, 
            DateTime submissionTime, bool isGroupSubmission, String notebookID, String submissionID)
        {
            if(App.Network.ProjectorProxy != null)
            {
                Thread t = new Thread(() =>
                {
                    try
                    {
                        App.Network.ProjectorProxy.AddSerializedSubmission(sPage, submitter, groupSubmitter,
            submissionTime, isGroupSubmission, notebookID, submissionID);
                    }
                    catch(Exception ex)
                    {
                        Logger.Instance.WriteToLog("Submit to Projector Error: " + ex.Message);
                    }
                });
                t.IsBackground = true;
                t.Start();
            }
            else
            {
                //TODO: Steve - add pages to a queue and send when a projector is found
                Console.WriteLine("Projector NOT Available");
            }

            var page = ObjectSerializer.ToObject(sPage);
            ICLPPage submission = null;
            if(page is CLPPage)
            {
                submission = (page as CLPPage).Clone() as CLPPage;
            }
            if(page is CLPAnimationPage)
            {
                submission = (page as CLPAnimationPage).Clone() as CLPAnimationPage;
            }
            submission.SubmissionType = isGroupSubmission ? SubmissionType.Group : SubmissionType.Single;
            submission.SubmissionID = submissionID;
            submission.SubmissionTime = submissionTime;
            submission.Submitter = submitter;
            submission.GroupSubmitter = groupSubmitter;
            submission.InkStrokes = StrokeDTO.LoadInkStrokes(submission.SerializedStrokes);

            foreach(ICLPPageObject pageObject in submission.PageObjects)
            {
                pageObject.ParentPage = submission;
            }

            CLPNotebook currentNotebook = null;

            foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
            {
                if(notebookID == notebook.UniqueID)
                {
                    currentNotebook = notebook;
                    break;
                }
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    try
                    {
                        CLPServiceAgent.Instance.AddSubmission(currentNotebook, submission);
                        //CLPServiceAgent.Instance.QuickSaveNotebook("RECIEVE-" + userName);
                    }
                    catch(Exception e)
                    {
                        Logger.Instance.WriteToLog("[ERROR] Recieved Submission from wrong notebook: " + e.Message);
                    }

                    return null;
                }, null);
        }

        public void CollectStudentNotebook(string sNotebook, string studentName)
        {
            CLPNotebook notebook = ObjectSerializer.ToObject(sNotebook) as CLPNotebook;
            
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\CollectedStudentNotebooks";

            if(!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            DateTime saveTime = DateTime.Now;
            string time = saveTime.Year + "." + saveTime.Month + "." + saveTime.Day;

            string filePathName = filePath + @"\" + time + "-" + studentName + "-" + notebook.NotebookName + @".clp";
            notebook.Save(filePathName);
        }

        public void StudentLogin(Person student)
        {
            App.Network.ClassList.Add(student);
            Logger.Instance.WriteToLog("Student Logged In: " + student.FullName);
        }

        public void StudentLogout(Person student)
        {
            Console.WriteLine("Logout");
        }

        #endregion
    }
}
