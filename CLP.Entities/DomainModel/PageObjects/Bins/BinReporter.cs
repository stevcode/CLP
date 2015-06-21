using System;
using System.Linq;
using System.Runtime.Serialization;

namespace CLP.Entities
{
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
            Height = 65;
            Width = 90;
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
            get { return "BinReporter"; }
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

        public override IPageObject Duplicate()
        {
            var newBinReporter = Clone() as BinReporter;
            if (newBinReporter == null)
            {
                return null;
            }
            newBinReporter.CreationDate = DateTime.Now;
            newBinReporter.ID = Guid.NewGuid().ToCompactID();
            newBinReporter.VersionIndex = 0;
            newBinReporter.LastVersionIndex = null;
            newBinReporter.ParentPage = ParentPage;

            return newBinReporter;
        }

        #endregion //APageObjectBase Overrides

        #region IReporter Implementation

        private int NumberInBins
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

        private int NumberNotInBins
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

        public string FormattedReport
        {
            get { return string.Format("{0} in Bins\n" + "{1} not in Bins", NumberInBins, NumberNotInBins); }
        }

        public void UpdateReport() { RaisePropertyChanged("FormattedReport"); }

        #endregion //IReporter Implementation
    }
}