using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace CLP.Models
{
    [Serializable]
    public class CLPPage : ACLPPageBase
    {
        #region Constructor

        public CLPPage()
            : this(LANDSCAPE_HEIGHT, LANDSCAPE_WIDTH) {}

        public CLPPage(double pageHeight, double pageWidth)
            : base(pageHeight, pageWidth) {}

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPPage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructor

        #region ICLPPage Methods

        public override ICLPPage DuplicatePage()
        {
            var newPage = new CLPPage
                          {
                              ParentNotebookID = ParentNotebookID,
                              PageTags = PageTags,
                              GroupSubmitType = GroupSubmitType,
                              PageHeight = PageHeight,
                              PageWidth = PageWidth,
                              InitialPageAspectRatio = InitialPageAspectRatio,
                              ImagePool = ImagePool
                          };

            foreach(var s in InkStrokes.Select(stroke => stroke.Clone())) 
            {
                s.RemovePropertyData(StrokeIDKey);

                var newUniqueID = Guid.NewGuid().ToString();
                s.AddPropertyData(StrokeIDKey, newUniqueID);

                newPage.InkStrokes.Add(s);
            }
            newPage.SerializedStrokes = StrokeDTO.SaveInkStrokes(newPage.InkStrokes);

            foreach(var clonedPageObject in PageObjects.Select(pageObject => pageObject.Duplicate()))
            {
                clonedPageObject.ParentPage = newPage;
                clonedPageObject.ParentPageID = newPage.UniqueID;
                newPage.PageObjects.Add(clonedPageObject);
                clonedPageObject.RefreshStrokeParentIDs();
            }

            return newPage;
        }

        #endregion //ICLPPage Methods

        #region Methods
        public void updateProgress()
        {
            try
            {
                //CLPAnimationPage page = (CLPAnimationPage)(MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
                CLPProofHistory proofPageHistory1 = (CLPProofHistory)PageHistory;
                double FutureItemsNumber = proofPageHistory1.Future.Count;
                double pastItemsNumber = proofPageHistory1.MetaPast.Count;
                double totalItemsNumber = FutureItemsNumber + pastItemsNumber;

                if(totalItemsNumber == 0)
                {
                    
                    ProofPresent = "Hidden";
                    ProofProgressCurrent = 0;
                    SliderProgressCurrent = 0;
                    return;
                }
                else
                {
                    ProofPresent = "Visible";
                    ProofProgressCurrent =

                        (pastItemsNumber * PageWidth * 0.7175) /
                        totalItemsNumber;
                    SliderProgressCurrent = (pastItemsNumber * 100) /
                        totalItemsNumber;
                    
                }

                if(proofPageHistory1.ProofPageAction.Equals(CLPProofHistory.CLPProofPageAction.Record))
                {
                    ProofProgressVisible = "Hidden";
                }
                else {
                    ProofProgressVisible = "Visible";
                    
                }


            }catch(Exception e){
                Console.WriteLine(e.Message);
            }
        }

        #endregion
    }
}

