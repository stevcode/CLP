using System;
using System.Linq;
using System.Runtime.Serialization;

namespace CLP.Models
{
    [Serializable]
    public class CLPAnimationPage : ACLPPageBase
    {
        #region Constructors

        public CLPAnimationPage()
            : this(LANDSCAPE_HEIGHT, LANDSCAPE_WIDTH) {}

        public CLPAnimationPage(double pageHeight, double pageWidth)
            : base(pageHeight, pageWidth) {}
        
        protected CLPAnimationPage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructors

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
    }
}
