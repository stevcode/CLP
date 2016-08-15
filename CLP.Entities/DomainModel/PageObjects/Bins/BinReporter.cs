using System;
using System.Linq;
using System.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class BinReporter : APageObjectBase, IReporter
    {
        #region Constructors

        /// <summary>Initializes <see cref="BinReporter" /> from scratch.</summary>
        public BinReporter() { }

        /// <summary>Initializes <see cref="BinReporter" /> from</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="BinReporter" /> belongs to.</param>
        public BinReporter(CLPPage parentPage)
            : base(parentPage)
        {
            Height = 90;
            Width = 145;
            XPosition = parentPage.Width - Width - 20;
            YPosition = 20;
            PageObjectFunctionalityVersion = "1";
        }

        /// <summary>Initializes <see cref="BinReporter" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public BinReporter(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region APageObjectBase Overrides

        public override string FormattedName
        {
            get { return string.Format("Bin Reporter reporting: {0}", FormattedReport); }
        }

        public override string CodedName
        {
            get { return "BIN REPORTER"; }
        }

        public override string CodedID
        {
            get { return "A"; }
        }

        public override int ZIndex
        {
            get { return 70; }
        }

        /// <summary>Determines whether the <see cref="IPageObject" /> has properties can be changed by a <see cref="Person" /> anyone at any time.</summary>
        public override bool IsBackgroundInteractable
        {
            get { return false; }
        }

        #endregion //APageObjectBase Overrides

        #region IReporter Implementation

        private int MarksInBins
        {
            get
            {
                if (ParentPage == null)
                {
                    return 0;
                }

                var marksOnPage = ParentPage.PageObjects.OfType<Mark>().ToList();
                var binsOnPage = ParentPage.PageObjects.OfType<Bin>().ToList();
                var marksInBins = marksOnPage.Where(m => binsOnPage.Any(b => b.AcceptedPageObjectIDs.Contains(m.ID))).ToList();

                return marksInBins.Count;
            }
        }

        private int InkMarksInBins
        {
            get
            {
                if (ParentPage == null)
                {
                    return 0;
                }

                return ParentPage.PageObjects.OfType<Bin>().Aggregate(0, (sum, b) => sum + b.AcceptedStrokes.Count);
            }
        }

        private int MarksNotInBins
        {
            get
            {
                if (ParentPage == null)
                {
                    return 0;
                }

                var marksOnPage = ParentPage.PageObjects.OfType<Mark>().ToList();
                var binsOnPage = ParentPage.PageObjects.OfType<Bin>().ToList();
                var marksInBins = marksOnPage.Where(m => binsOnPage.Any(b => b.AcceptedPageObjectIDs.Contains(m.ID))).ToList();

                return marksOnPage.Count - marksInBins.Count;
            }
        }

        private int NumberOfBins
        {
            get
            {
                var binsOnPage = ParentPage.PageObjects.OfType<Bin>().ToList();
                return binsOnPage.Count;
            }
        }

        public string FormattedReport
        {
            get
            {
                switch (PageObjectFunctionalityVersion)
                {
                    case "0":
                        return string.Format("{0} in Bins\n" + "{1} not in Bins", MarksInBins, MarksNotInBins);
                    case "1":
                        var notInBinsReporter = MarksNotInBins + MarksInBins > 0 ? string.Format("{0} not in Bins", MarksNotInBins) : string.Empty;
                        var totalMarks = MarksInBins + InkMarksInBins;
                        return string.Format("{0} in Bins\n" + "{1}\n" + "{2} Bins", totalMarks, notInBinsReporter, NumberOfBins);
                }

                return "[ERROR] Invalid PageObjectFunctionalityVersion";
            }
        }

        public void UpdateReport() { RaisePropertyChanged("FormattedReport"); }

        #endregion //IReporter Implementation
    }
}