using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    public class ChoiceBubble : AEntityBase
    {
        #region Constructors

        public ChoiceBubble() { }

        public ChoiceBubble(int index, MultipleChoiceLabelTypes labelType, string answer, string label = "")
        {
            BubbleContents = labelType == MultipleChoiceLabelTypes.Numbers ? (index + 1).ToString() : IntToUpperLetter(index + 1);
            Answer = answer;
            Label = label;
        }

        public ChoiceBubble(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Bubble's offset from initial x/y position.</summary>
        public double Offset
        {
            get { return GetValue<double>(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        public static readonly PropertyData OffsetProperty = RegisterProperty("Offset", typeof (double), 0.0);

        /// <summary>Label inside the choice bubble.</summary>
        public string BubbleContents
        {
            get { return GetValue<string>(BubbleContentsProperty); }
            set { SetValue(BubbleContentsProperty, value); }
        }

        public static readonly PropertyData BubbleContentsProperty = RegisterProperty("BubbleContents", typeof (string), string.Empty);

        /// <summary>The answer associated with the bubble.</summary>
        public string Answer
        {
            get { return GetValue<string>(AnswerProperty); }
            set { SetValue(AnswerProperty, value); }
        }

        public static readonly PropertyData AnswerProperty = RegisterProperty("Answer", typeof (string), string.Empty);

        /// <summary>Extra, non-relevant answer data.</summary>
        public string Label
        {
            get { return GetValue<string>(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly PropertyData LabelProperty = RegisterProperty("Label", typeof (string), string.Empty);

        /// <summary>Signifies the Bubble represents a correct answer.</summary>
        public bool IsACorrectValue
        {
            get { return GetValue<bool>(IsACorrectValueProperty); }
            set { SetValue(IsACorrectValueProperty, value); }
        }

        public static readonly PropertyData IsACorrectValueProperty = RegisterProperty("IsACorrectValue", typeof (bool), false);

        /// <summary>Indicates the choice bubble has been filled in by ink.</summary>
        public bool IsFilledIn
        {
            get { return GetValue<bool>(IsFilledInProperty); }
            set { SetValue(IsFilledInProperty, value); }
        }

        public static readonly PropertyData IsFilledInProperty = RegisterProperty("IsFilledIn", typeof (bool), false);

        #region Calculated Properties

        public string BubbleCodedID
        {
            get { return string.Format("{0} \"{1}\"", BubbleContents, Answer); }
        }

        #endregion // Calculated Properties

        #endregion //Properties

        #region Methods

        public static string IntToUpperLetter(int index)
        {
            if (index < 1 ||
                index > 26)
            {
                return "aa";
            }

            return "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[index - 1].ToString();
        }

        #endregion //Methods
    }

    [Serializable]
    public class MultipleChoice : AStrokeAccepter
    {
        #region Constructors

        public MultipleChoice() { }

        public MultipleChoice(int index, MultipleChoiceLabelTypes labelType) { }

        public MultipleChoice(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public double ChoiceBubbleDiameter
        {
            get { return 35.0; }
        }

        public double ChoiceBubbleGapLength
        {
            get { return ((Orientation == MultipleChoiceOrientations.Horizontal ? Width : Height) - ChoiceBubbleDiameter) / (ChoiceBubbles.Count - 1); }
        }

        /// <summary>Orientation of box.</summary>
        public MultipleChoiceOrientations Orientation
        {
            get { return GetValue<MultipleChoiceOrientations>(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly PropertyData OrientationProperty = RegisterProperty("Orientation", typeof (MultipleChoiceOrientations), MultipleChoiceOrientations.Horizontal);

        /// <summary>List of the available choices.</summary>
        public ObservableCollection<ChoiceBubble> ChoiceBubbles
        {
            get { return GetValue<ObservableCollection<ChoiceBubble>>(ChoiceBubblesProperty); }
            set { SetValue(ChoiceBubblesProperty, value); }
        }

        public static readonly PropertyData ChoiceBubblesProperty = RegisterProperty("ChoiceBubbles", typeof (ObservableCollection<ChoiceBubble>), () => new ObservableCollection<ChoiceBubble>());

        #endregion //Properties

        #region APageObjectBase Overrides

        public override string FormattedName
        {
            get { return "Multiple Choice"; }
        }

        public override string CodedName
        {
            get { return "MC"; }
        }

        public override string CodedID
        {
            get
            {
                var idParts = ChoiceBubbles.Where(b => b.IsACorrectValue).Select(b => b.BubbleCodedID).ToList();
                return string.Join(", ", idParts);
            }
        }

        public override int ZIndex
        {
            get { return 15; }
        }

        public override bool IsBackgroundInteractable
        {
            get { return false; }
        }

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Width" ||
                e.PropertyName == "Height")
            {
                RaisePropertyChanged("ChoiceBubbleGapLength");
            }
            base.OnPropertyChanged(e);
        }

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

        #endregion //APageObjectBase Overrides

        #region AStrokeAccepter Overrides

        /// <summary>Stroke must be at least this percent contained by pageObject.</summary>
        public override int StrokeHitTestPercentage
        {
            get { return 80; }
        }

        #endregion //AStrokeAccepter Overrides
    }
}