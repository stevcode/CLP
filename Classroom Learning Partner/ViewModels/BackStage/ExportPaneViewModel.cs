using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Catel.IoC;
using Catel.MVVM;
using Catel.MVVM.Views;
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
        private readonly INotebookService _notebookService;

        #region Constructor

        public ExportPaneViewModel()
        {
            _notebookService = DependencyResolver.Resolve<INotebookService>();
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            ConvertNotebookToPDFCommand = new Command(OnConvertNotebookToPDFCommandExecute, OnNotebookExportCanExecute);
            ConvertPageSubmissionsToPDFCommand = new Command(OnConvertPageSubmissionsToPDFCommandExecute, OnNotebookExportCanExecute);
            ConvertAllSubmissionsToPDFCommand = new Command(OnConvertAllSubmissionsToPDFCommandExecute, OnNotebookExportCanExecute);
            ConvertDisplaysToPDFCommand = new Command(OnConvertDisplaysToPDFCommandExecute, OnNotebookExportCanExecute);

            CopyNotebookForNewOwnerCommand = new Command(OnCopyNotebookForNewOwnerCommandExecute, OnNotebookExportCanExecute);
        }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText
        {
            get { return "Export"; }
        }

        #endregion //Bindings

        #region Commands

        private bool OnNotebookExportCanExecute() { return _notebookService.CurrentNotebook != null; }

        /// <summary>Converts Notebook Pages to PDF.</summary>
        public Command ConvertNotebookToPDFCommand { get; private set; }

        private void OnConvertNotebookToPDFCommandExecute()
        {
            var notebook = _notebookService.CurrentNotebook;

            ConvertPagesToPDF(notebook.Pages, notebook);
        }

        /// <summary>Converts the Submissions of the currently selected page to PDF.</summary>
        public Command ConvertPageSubmissionsToPDFCommand { get; private set; }

        private void OnConvertPageSubmissionsToPDFCommandExecute()
        {
            var notebook = _notebookService.CurrentNotebook;
            var sortedPages = notebook.CurrentPage.Submissions.ToList().OrderBy(page => page.Owner.FullName).ThenBy(page => page.VersionIndex);
            var pageNumber = "" + notebook.CurrentPage.PageNumber;
            if (notebook.CurrentPage.DifferentiationLevel != "0")
            {
                pageNumber += " " + notebook.CurrentPage.DifferentiationLevel;
            }

            ConvertPagesToPDF(sortedPages, notebook, "Page " + pageNumber + " Submissions");
        }

        /// <summary>Converts all Submissions in a notebook to PDF.</summary>
        public Command ConvertAllSubmissionsToPDFCommand { get; private set; }

        private void OnConvertAllSubmissionsToPDFCommandExecute()
        {
            var notebook = _notebookService.CurrentNotebook;
            var allPages = new List<CLPPage>();
            foreach (var page in notebook.Pages)
            {
                allPages.AddRange(page.Submissions);
            }
            var allSortedPages =
                allPages.OrderBy(page => page.PageNumber).ThenBy(page => page.DifferentiationLevel).ThenBy(page => page.Owner.FullName).ThenBy(page => page.VersionIndex);

            ConvertPagesToPDF(allSortedPages, notebook, "All Submissions");
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

        /// <summary>
        /// Copies the current notebook for a new owner.
        /// </summary>
        public Command CopyNotebookForNewOwnerCommand { get; private set; }

        private void OnCopyNotebookForNewOwnerCommandExecute()
        {
            MainWindowViewModel.CopyNotebookForNewOwner();
        }

        #endregion //Commands

        #region Methods

        private async void ConvertPagesToPDF(IEnumerable<CLPPage> pages, Notebook notebook, string fileNameSuffix = "", bool useLabels = true)
        {
            App.MainWindowViewModel.IsConvertingToPDF = true;

            var directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Notebooks - PDF");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var fileName = notebook.Name + " - " + notebook.Owner.FullName + " - " + fileNameSuffix + " [" + DateTime.Now.ToString("yyyy-M-d hh.mm.ss") + "].pdf";
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

                foreach (var page in pages)
                {
                    App.MainWindowViewModel.CurrentConvertingPage = null;
                    App.MainWindowViewModel.CurrentConvertingPage = page;

                    var currentPageViewModels = CLPServiceAgent.Instance.GetViewModelsFromModel(page);
                    var viewManager = Catel.IoC.ServiceLocator.Default.ResolveType<IViewManager>();

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

                    var screenshot = CLPServiceAgent.Instance.UIElementToImageByteArray(currentPagePreviewView, page.Width, dpi: 300);
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
                    bitmapImage.StreamSource = new MemoryStream(screenshot);
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    var pngEncoder = new PngBitmapEncoder();
                    pngEncoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                    using (var outputStream = new MemoryStream())
                    {
                        pngEncoder.Save(outputStream);
                        var image = Image.FromStream(outputStream);
                        var pdfImage = iTextSharp.text.Image.GetInstance(image, ImageFormat.Png);
                        var isPortrait = page.Height >= page.Width;
                        if (!isPortrait)
                        {
                            pdfImage.RotationDegrees = 270f;
                        }

                        pdfImage.ScaleToFit(doc.PageSize.Width - 74f, doc.PageSize.Height - 74f);

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
                                        Alignment = Element.ALIGN_CENTER
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