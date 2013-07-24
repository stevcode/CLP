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

        public List<ObservableCollection<ICLPPageObject>> CutObjects(double leftX, double rightX, double topY, double botY)
        {
            var lr = new List<ObservableCollection<ICLPPageObject>>();
            var c1 = new ObservableCollection<ICLPPageObject>();
            var c2 = new ObservableCollection<ICLPPageObject>();

            //TODO: Tim - This is fine for now, but you could have an instance where a really wide, but short rectangle is made
            // and a stroke could be made that was only a few pixels high, and quite wide, that would try to make a horizontal
            // cut instead of the vertical cut that was intended.
            if(Math.Abs(leftX - rightX) < Math.Abs(topY - botY))
            {
                double Ave = (rightX + leftX) / 2;
                foreach(ICLPPageObject pageObject in PageObjects)
                {
                    double otopYVal = pageObject.YPosition;
                    double obotYVal = otopYVal + pageObject.Height;
                    double oLeftXVal = pageObject.XPosition;
                    double oRightXVal = pageObject.XPosition + pageObject.Width;
                    if(pageObject.PageObjectType.Equals("CLPArray"))
                    {
                        if((oLeftXVal + CLPArray.LargeLabelLength <= leftX && oRightXVal - 2*CLPArray.SmallLabelLength >= rightX) &&
                           (topY - otopYVal - CLPArray.LargeLabelLength < 15 && obotYVal - 2*CLPArray.SmallLabelLength - botY < 15))
                        {
                            ObservableCollection<ICLPPageObject> c = pageObject.SplitAtX(Ave);
                            if(c.Count == 2){
                                foreach(ICLPPageObject no in c)
                                {
                                    c1.Add(no);
                                }
                                c2.Add(pageObject);
                            }
                        }
                    }
                    else if(pageObject.PageObjectType.Equals(CLPShape.Type))
                    {
                        CLPShape oc = (CLPShape)pageObject;
                        if(oc.ShapeType.Equals(CLPShape.CLPShapeType.Rectangle)){
                            if(((otopYVal >= topY) && (obotYVal <= botY)) &&
                             ((oLeftXVal <= leftX) && (oRightXVal >= rightX)))
                            {
                                ObservableCollection<ICLPPageObject> c = pageObject.SplitAtX(Ave);
                                foreach(ICLPPageObject no in c)
                                {
                                    c1.Add(no);
                                }
                                c2.Add(pageObject);
                            }
                        }
                    }
                }
            }
            else{ 
                double Ave = (topY + botY) / 2;
                foreach(ICLPPageObject o in this.PageObjects)
                {
                    double otopYVal = o.YPosition;
                    double obotYVal = otopYVal + o.Height;
                    double oLeftXVal = o.XPosition;
                    double oRightXVal = o.XPosition + o.Width;
                    if(o.PageObjectType.Equals("CLPArray"))
                    {
                        if((otopYVal + CLPArray.LargeLabelLength <= topY && obotYVal - 2*CLPArray.SmallLabelLength >= botY) &&
                           (leftX - oLeftXVal - CLPArray.LargeLabelLength < 15 && oRightXVal - 2*CLPArray.SmallLabelLength - rightX < 15))
                        {
                            ObservableCollection<ICLPPageObject> c = o.SplitAtY(Ave);
                            if(c.Count == 2){
                                foreach(ICLPPageObject no in c)
                                {
                                    c1.Add(no);
                                }
                                c2.Add(o);
                            }
                        }
                    }
                    else if(o.PageObjectType.Equals(CLPShape.Type))
                    {
                        CLPShape oc = (CLPShape)o;
                        if(oc.ShapeType.Equals(CLPShape.CLPShapeType.Rectangle)){
                            if(((otopYVal <= topY) && (obotYVal >= botY)) &&
                               ((oLeftXVal >= leftX) && (oRightXVal <= rightX))){
                                ObservableCollection<ICLPPageObject> c = o.SplitAtY(Ave);
                                foreach(ICLPPageObject no in c){
                                    c1.Add(no);
                                }
                                c2.Add(o);
                            }
                        }
                    }
               }  
            }
            lr.Add(c1);
            lr.Add(c2);
            return lr;
        }

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

