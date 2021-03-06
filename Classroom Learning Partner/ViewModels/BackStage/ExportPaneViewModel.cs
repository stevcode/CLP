﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Catel;
using Catel.IoC;
using Catel.MVVM;
using Catel.MVVM.Views;
using Catel.Windows;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views;
using CLP.Entities;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Image = System.Drawing.Image;
using Path = Catel.IO.Path;

namespace Classroom_Learning_Partner.ViewModels
{
    public class ExportPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public ExportPaneViewModel(IDataService dataService, IRoleService roleService)
            : base(dataService, roleService)
        {
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            ConvertNotebookToPDFCommand = new Command(OnConvertNotebookToPDFCommandExecute, OnNotebookExportCanExecute);
            ConvertPageSubmissionsToPDFCommand = new Command(OnConvertPageSubmissionsToPDFCommandExecute, OnNotebookExportCanExecute);
            ConvertAllSubmissionsToPDFCommand = new Command(OnConvertAllSubmissionsToPDFCommandExecute, OnNotebookExportCanExecute);
            ConvertDisplaysToPDFCommand = new Command(OnConvertDisplaysToPDFCommandExecute, OnNotebookExportCanExecute);
            CopyNotebookForNewOwnerCommand = new Command(OnCopyNotebookForNewOwnerCommandExecute);
        }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText => "Export";

        #endregion //Bindings

        #region Commands

        private bool OnNotebookExportCanExecute()
        {
            return true;
        }

        /// <summary>Converts Notebook Pages to PDF.</summary>
        public Command ConvertNotebookToPDFCommand { get; private set; }

        private void OnConvertNotebookToPDFCommandExecute()
        {
            var notebook = _dataService.CurrentNotebook;

            ConvertPagesToPDF(notebook.Pages, notebook);
        }

        /// <summary>Converts the Submissions of the currently selected page to PDF.</summary>
        public Command ConvertPageSubmissionsToPDFCommand { get; private set; }

        private void OnConvertPageSubmissionsToPDFCommandExecute()
        {
            var notebook = _dataService.CurrentNotebook;

            var submissions =
                _roleService.Role != ProgramRoles.Student ? _dataService.GetLoadedSubmissionsForPage(notebook.CurrentPage) : notebook.CurrentPage.Submissions.ToList();

            var sortedPages = submissions.OrderBy(page => page.Owner.FullName).ThenBy(page => page.VersionIndex);

            ConvertPagesToPDF(sortedPages, notebook, true);
        }

        /// <summary>Converts all Submissions in a notebook to PDF.</summary>
        public Command ConvertAllSubmissionsToPDFCommand { get; private set; }

        private void OnConvertAllSubmissionsToPDFCommandExecute()
        {
            var notebook = _dataService.CurrentNotebook;
            var allPages = new List<CLPPage>();
            CLPPage lastSubmissionAdded = null;
            foreach (var page in notebook.Pages)
            {
                var submissions = _roleService.Role != ProgramRoles.Student ? _dataService.GetLoadedSubmissionsForPage(page) : page.Submissions.ToList();

                foreach (var submission in submissions)
                {
                    if (submission.Owner != null)
                    {
                        lastSubmissionAdded = submission;
                        allPages.Add(submission);
                        continue;
                    }

                    CLogger.AppendToLog("Skipping Submission from Page #: " + submission.PageNumber);
                    if (lastSubmissionAdded != null)
                    {
                        CLogger.AppendToLog("Last Submission Page #: " + lastSubmissionAdded.PageNumber);
                        CLogger.AppendToLog("Last Submission Page Owner: " + lastSubmissionAdded.Owner.FullName);
                    }
                }
            }
            var allSortedPages = allPages.OrderBy(page => page.PageNumber).ThenBy(page => page.DifferentiationLevel).ThenBy(page => page.Owner.FullName).ThenBy(page => page.VersionIndex).ToList();

            ConvertPagesToPDF(allSortedPages, notebook, true);
        }

        /// <summary>Converts Notebook Displays to PDF.</summary>
        public Command ConvertDisplaysToPDFCommand { get; private set; }

        private void OnConvertDisplaysToPDFCommandExecute()
        {
            // TODO: Entities
            //var notebookWorkspaceViewModel = MainWindow.Workspace as NotebookWorkspaceViewModel;
            //if(notebookWorkspaceViewModel == null)
            //{
            //    return;
            //}

            //Catel.Windows.PleaseWaitHelper.Show(() =>
            //{
            //    var notebook = notebookWorkspaceViewModel.Notebook;
            //    var directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Displays - XPS\";
            //    if(!Directory.Exists(directoryPath))
            //    {
            //        Directory.CreateDirectory(directoryPath);
            //    }

            //    var fileName = notebook.Name + " - Displays.xps";
            //    var filePath = directoryPath + fileName;
            //    if(File.Exists(filePath))
            //    {
            //        File.Delete(filePath);
            //    }

            //    var document = new FixedDocument();
            //    document.DocumentPaginator.PageSize = new Size(96 * 11, 96 * 8.5);

            //    foreach(var display in notebook.Displays)
            //    {
            //        var currentDisplayView = new GridDisplayPreviewView
            //                                 {
            //                                     DataContext = display,
            //                                     Height = 96 * 8.5,
            //                                     Width = 96 * 11
            //                                 };
            //        currentDisplayView.UpdateLayout();
            //        var gridDisplay = display as CLPGridDisplay;
            //        var displayIndex = gridDisplay != null ? gridDisplay.DisplayNumber : 0;

            //        var grid = new Grid();
            //        grid.Children.Add(currentDisplayView);
            //        var pageIndexlabel = new Label
            //        {
            //            FontSize = 20,
            //            FontWeight = FontWeights.Bold,
            //            FontStyle = FontStyles.Oblique,
            //            HorizontalAlignment = HorizontalAlignment.Right,
            //            VerticalAlignment = VerticalAlignment.Top,
            //            Content = "Display " + displayIndex,
            //            Margin = new Thickness(0, 5, 5, 0)
            //        };
            //        grid.Children.Add(pageIndexlabel);
            //        grid.UpdateLayout();

            //        var transform = new TransformGroup();
            //        var rotate = new RotateTransform(90.0);
            //        var translate = new TranslateTransform(816, 0);
            //        transform.Children.Add(rotate);
            //        transform.Children.Add(translate);
            //        grid.RenderTransform = transform;

            //        var pageContent = new PageContent();
            //        var fixedPage = new FixedPage();
            //        fixedPage.Children.Add(grid);

            //        ((System.Windows.Markup.IAddChild)pageContent).AddChild(fixedPage);
            //        document.Pages.Add(pageContent);
            //    }

            //    //Save the document
            //    var xpsDocument = new XpsDocument(filePath, FileAccess.ReadWrite);
            //    var documentWriter = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
            //    documentWriter.Write(document);
            //    xpsDocument.Close();

            //}, null, "Converting Notebook Displays to XPS", 0.0 / 0.0);
        }

        /// <summary>SUMMARY</summary>
        public Command CopyNotebookForNewOwnerCommand { get; private set; }

        private void OnCopyNotebookForNewOwnerCommandExecute()
        {
            //if (_dataService == null ||
            //    _dataService.CurrentCacheInfo == null ||
            //    _dataService.CurrentNotebookInfo == null ||
            //    _dataService.CurrentNotebookInfo.Notebook == null)
            //{
            //    return;
            //}

            //var textInputViewModel = new TextInputViewModel
            //                         {
            //                             TextPrompt = "Student Name: "
            //                         };
            //var textInputView = new TextInputView(textInputViewModel);
            //textInputView.ShowDialog();

            //if (textInputView.DialogResult == null ||
            //    textInputView.DialogResult != true ||
            //    string.IsNullOrEmpty(textInputViewModel.InputText))
            //{
            //    return;
            //}

            //var person = new Person
            //             {
            //                 IsStudent = true,
            //                 FullName = textInputViewModel.InputText
            //             };

            //var copiedNotebook = _dataService.CurrentNotebookInfo.Notebook.CopyForNewOwner(person);
            //copiedNotebook.CurrentPage = copiedNotebook.Pages.FirstOrDefault();
            //var notebookComposite = NotebookNameComposite.ParseNotebook(copiedNotebook);
            //var notebookPath = Path.Combine(_dataService.CurrentCacheInfo.NotebooksFolderPath, notebookComposite.ToFolderName());
            //var notebookInfo = new NotebookInfo(_dataService.CurrentCacheInfo, notebookPath)
            //                   {
            //                       Notebook = copiedNotebook
            //                   };
            //PleaseWaitHelper.Show(() => _dataService.SaveNotebookLocally(notebookInfo, true), null, "Saving Notebook");
            //_dataService.SetCurrentNotebook(notebookInfo);

            //var person = new Person();
            //var personCreationView = new PersonView(new PersonViewModel(person));
            //personCreationView.ShowDialog();

            //if (personCreationView.DialogResult == null ||
            //    personCreationView.DialogResult != true)
            //{
            //    return;
            //}

            //var copiedNotebook = _dataService.CurrentNotebook.CopyForNewOwner(person);
            //copiedNotebook.CurrentPage = copiedNotebook.Pages.FirstOrDefault();

            //App.MainWindowViewModel.CurrentUser = person;
            //App.MainWindowViewModel.IsAuthoring = false;
            //App.MainWindowViewModel.Workspace = new BlankWorkspaceViewModel();
            //App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(copiedNotebook);
            //App.MainWindowViewModel.IsBackStageVisible = false;
        }

        #endregion //Commands

        #region Methods

        private async void ConvertPagesToPDF(IEnumerable<CLPPage> pages, Notebook notebook, bool submissions = false, bool useLabels = true)
        {
            var clpPages = pages as IList<CLPPage> ?? pages.ToList();
            if (pages == null ||
                !clpPages.Any())
            {
                MessageBox.Show("Something went wrong. No pages were passed to the converter!");
                return;
            }

            App.MainWindowViewModel.IsConvertingToPDF = true;

            var directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Notebooks - PDF");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var pageNumbers = clpPages.Select(p => p.PageNumber).Distinct(); //Selects only Whole numbers.
            var pageNumberRanges = RangeHelper.ParseIntNumbersToString(pageNumbers, true, true);

            var fileName = notebook.Name + ", " + notebook.Owner.FullName + " pp " + pageNumberRanges + (submissions ? " Submissions" : string.Empty) + " [" +
                           DateTime.Now.ToString("yyyy-M-dd hh.mm.ss") + "].pdf";
            var filePath = Path.Combine(directoryPath, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var doc = new Document();

            try
            {
                PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                foreach (var page in clpPages)
                {
                    App.MainWindowViewModel.CurrentConvertingPage = null;
                    App.MainWindowViewModel.CurrentConvertingPage = page;

                    var currentPageViewModels = page.GetAllViewModels();
                    var viewManager = ServiceLocator.Default.ResolveType<IViewManager>();

                    NonAsyncPagePreviewView currentPagePreviewView = null;
                    foreach (var views in currentPageViewModels.Select(viewManager.GetViewsOfViewModel))
                    {
                        currentPagePreviewView = views.FirstOrDefault(view => view is NonAsyncPagePreviewView) as NonAsyncPagePreviewView;
                    }

                    if (currentPagePreviewView == null)
                    {
                        continue;
                    }

                    await Task.Delay(1000);

                    var bitmapImage = currentPagePreviewView.ToBitmapImage(page.Width, dpi: 300);
                    var pngEncoder = new PngBitmapEncoder();
                    pngEncoder.Frames.Add(BitmapFrame.Create(bitmapImage));

                    var isPortrait = page.Height >= page.Width;
                    doc.SetPageSize(new Rectangle(0, 0, isPortrait ? 595.0f : 842.0f, isPortrait ? 842.0f : 595.0f));
                    //if (!isPortrait)
                    //{
                    //   // doc.SetPageSize()
                    //    //pdfImage.RotationDegrees = 270f;
                    //}

                    using (var outputStream = new MemoryStream())
                    {
                        pngEncoder.Save(outputStream);
                        var image = Image.FromStream(outputStream);
                        var pdfImage = iTextSharp.text.Image.GetInstance(image, ImageFormat.Png);

                        pdfImage.ScaleToFit(doc.PageSize.Width - 100f, doc.PageSize.Height - 100f);
                        pdfImage.Alignment = Element.ALIGN_CENTER;

                        pdfImage.Border = Rectangle.BOX;
                        pdfImage.BorderColor = BaseColor.BLACK;
                        pdfImage.BorderWidth = 1f;

                        var labelText = notebook.Name;
                        if (page.PageType == PageTypes.Animation)
                        {
                            labelText += ", [ANIMATION] Page ";
                        }
                        else
                        {
                            labelText += ", Page ";
                        }
                        labelText += page.PageNumber;
                        if (page.DifferentiationLevel != "0")
                        {
                            labelText += " " + page.DifferentiationLevel;
                        }
                        if (page.Owner == null)
                        {
                            page.Owner = App.MainWindowViewModel.CurrentUser;
                        }
                        labelText += ", Submission Time: " + page.SubmissionTime + ", Owner: " + page.Owner.FullName;
                        var label = new Paragraph(labelText)
                                    {
                                        Alignment = Element.ALIGN_CENTER,
                                    };

                        doc.NewPage();
                        doc.Add(label);
                        doc.Add(pdfImage);
                    }
                }
            }
            finally
            {
                doc.Close();
            }

            App.MainWindowViewModel.IsConvertingToPDF = false;
        }

        #endregion //Methods
    }
}