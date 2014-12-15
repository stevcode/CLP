using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum MultipleChoiceOrientations
    {
        Horizontal,
        Vertical
    }

    public enum MultipleChoiceLabelTypes
    {
        Numbers,
        Letters
    }

    [Serializable]
    public class MultipleChoiceBox : APageObjectBase//, IStrokeAccepter
    {
        #region Constructor

        public MultipleChoiceBox() { }

        public MultipleChoiceBox(CLPPage parentPage, int numberOfChoices, string correctAnswerLabel, MultipleChoiceOrientations orientation, MultipleChoiceLabelTypes labelType)
            : base(parentPage)
        {
            NumberOfChoices = numberOfChoices;
            CorrectAnswerLabel = correctAnswerLabel;
            Orientation = orientation;
            LabelType = labelType;
            Height = orientation == MultipleChoiceOrientations.Horizontal ? ChoiceBubbleDiameter : ChoiceBubbleDiameter * numberOfChoices + 5 * (numberOfChoices - 1);
            Width = orientation == MultipleChoiceOrientations.Vertical ? ChoiceBubbleDiameter : ChoiceBubbleDiameter * numberOfChoices + 5 * (numberOfChoices - 1);
        }

        public MultipleChoiceBox(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructor

        #region Properties

        public override int ZIndex
        {
            get { return 25; }
        }

        public override bool IsBackgroundInteractable
        {
            get { return false; }
        }

        public double ChoiceBubbleDiameter
        {
            get { return 35.0; }
        }

        /// <summary>
        /// Number of possible choices.
        /// </summary>
        public int NumberOfChoices
        {
            get { return GetValue<int>(NumberOfChoicesProperty); }
            set { SetValue(NumberOfChoicesProperty, value); }
        }

        public static readonly PropertyData NumberOfChoicesProperty = RegisterProperty("NumberOfChoices", typeof(int), 0);

        /// <summary>
        /// Value of the correct label.
        /// </summary>
        public string CorrectAnswerLabel
        {
            get { return GetValue<string>(CorrectAnswerLabelProperty); }
            set { SetValue(CorrectAnswerLabelProperty, value); }
        }

        public static readonly PropertyData CorrectAnswerLabelProperty = RegisterProperty("CorrectAnswerLabel", typeof (string), string.Empty);

        /// <summary>
        /// Orientation of box.
        /// </summary>
        public MultipleChoiceOrientations Orientation
        {
            get { return GetValue<MultipleChoiceOrientations>(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly PropertyData OrientationProperty = RegisterProperty("Orientation", typeof (MultipleChoiceOrientations), MultipleChoiceOrientations.Horizontal);

        /// <summary>
        /// Label types
        /// </summary>
        public MultipleChoiceLabelTypes LabelType
        {
            get { return GetValue<MultipleChoiceLabelTypes>(LabelTypeProperty); }
            set { SetValue(LabelTypeProperty, value); }
        }

        public static readonly PropertyData LabelTypeProperty = RegisterProperty("LabelType", typeof(MultipleChoiceLabelTypes), MultipleChoiceLabelTypes.Numbers);

        #endregion //Properties

        #region Methods

        public override IPageObject Duplicate()
        {
            var newMultipleChoiceBox = Clone() as MultipleChoiceBox;
            if (newMultipleChoiceBox == null)
            {
                return null;
            }
            newMultipleChoiceBox.CreationDate = DateTime.Now;
            newMultipleChoiceBox.ID = Guid.NewGuid().ToCompactID();
            newMultipleChoiceBox.VersionIndex = 0;
            newMultipleChoiceBox.LastVersionIndex = null;
            newMultipleChoiceBox.ParentPage = ParentPage;

            return newMultipleChoiceBox;
        }

        #endregion //Methods
    }
}