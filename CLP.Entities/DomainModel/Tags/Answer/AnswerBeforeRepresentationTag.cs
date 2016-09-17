﻿using System;
using System.Collections.Generic;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class AnswerBeforeRepresentationTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="AnswerChangedAfterRepresentationTag" /> from scratch.</summary>
        public AnswerBeforeRepresentationTag() { }

        /// <summary>Initializes <see cref="AnswerChangedAfterRepresentationTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="AnswerChangedAfterRepresentationTag" /> belongs to.</param>
        public AnswerBeforeRepresentationTag(CLPPage parentPage, Origin origin, List<IHistoryAction> historyActions)
            : base(parentPage, origin)
        {
            HistoryActionIDs = historyActions.Select(h => h.ID).ToList();
        }

        #endregion // Constructors

        #region Properties

        /// <summary>List of <see cref="IHistoryAction" /> IDs used to generate this Tag.</summary>
        public List<string> HistoryActionIDs
        {
            get { return GetValue<List<string>>(HistoryActionIDsProperty); }
            set { SetValue(HistoryActionIDsProperty, value); }
        }

        public static readonly PropertyData HistoryActionIDsProperty = RegisterProperty("HistoryActionIDs", typeof(List<string>), () => new List<string>());

        #region Calculated Properties

        public List<IHistoryAction> HistoryActions
        {
            get { return ParentPage.History.HistoryActions.Where(h => HistoryActionIDs.Contains(h.ID)).OrderBy(h => h.HistoryActionIndex).Distinct().ToList(); }
        }

        public IHistoryAction Answer
        {
            get { return HistoryActions.FirstOrDefault(Codings.IsAnswerObject); }
        }

        #endregion // Calculated Properties

        #endregion // Properties

        #region ATagBase Overrides

        public string AnalysisCode
        {
            get
            {
                var isAnswerCorrect = Codings.GetAnswerObjectCorrectness(Answer) == "COR";
                var answerContent = Codings.GetAnswerObjectContent(Answer);
                var analysisObjectCode = isAnswerCorrect ? Codings.ANALYSIS_COR_BEFORE_REP : Codings.ANALYSIS_INC_BEFORE_REP;
                return string.Format("{0} [{1}]", analysisObjectCode, answerContent);
            }
        }

        public override Category Category => Category.Answer;

        public override string FormattedName => "Answer Before Representation";

        public override string FormattedValue
        {
            get
            {
                if (Answer == null)
                {
                    return "[ERROR]: Tag generated with incorrect variables.";
                }

                var isAnswerCorrect = Codings.GetAnswerObjectCorrectness(Answer) == "COR";
                var analysisObjectCode = isAnswerCorrect ? Codings.ANALYSIS_COR_BEFORE_REP : Codings.ANALYSIS_INC_BEFORE_REP;

                var answerContents = Codings.GetAnswerObjectContent(Answer);

                var answerSet = string.Format("{0} {1} ({2})", Codings.FriendlyObjects[Answer.CodedObject], answerContents, isAnswerCorrect ? "correct" : "incorrect");

                var analysisCode = string.IsNullOrWhiteSpace(analysisObjectCode) ? string.Empty : analysisObjectCode;

                var representationsAdded =
                    HistoryActions.Where(h => Codings.IsRepresentationObject(h) && h.CodedObjectAction == Codings.ACTION_OBJECT_ADD && Codings.FriendlyObjects.ContainsKey(h.CodedObject))
                                  .Select(h => string.Format("{0} [{1}]", h.CodedObject, h.CodedObjectID));

                return string.Format("{0}\nRepresentations: {1}{2}",
                                     answerSet,
                                     string.Join(", ", representationsAdded),
                                     string.IsNullOrWhiteSpace(analysisCode) ? string.Empty : "\nCode: " + analysisCode);
            }
        }

        #endregion //ATagBase Overrides
    }
}