using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;

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
    public class MultipleChoiceBubble : AEntityBase
    {
        #region Constructors

        public MultipleChoiceBubble() { }

        public MultipleChoiceBubble(int index, MultipleChoiceLabelTypes labelType)
        {
            ChoiceBubbleIndex = index;
            ChoiceBubbleLabel = labelType == MultipleChoiceLabelTypes.Numbers ? (index + 1).ToString() : IntToUpperLetter(index + 1);
        }

        public MultipleChoiceBubble(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Index location of the choice bubble.</summary>
        public int ChoiceBubbleIndex
        {
            get { return GetValue<int>(ChoiceBubbleIndexProperty); }
            set { SetValue(ChoiceBubbleIndexProperty, value); }
        }

        public static readonly PropertyData ChoiceBubbleIndexProperty = RegisterProperty("ChoiceBubbleIndex", typeof (int), 0);

        /// <summary>Label for the choice bubble.</summary>
        public string ChoiceBubbleLabel
        {
            get { return GetValue<string>(ChoiceBubbleLabelProperty); }
            set { SetValue(ChoiceBubbleLabelProperty, value); }
        }

        public static readonly PropertyData ChoiceBubbleLabelProperty = RegisterProperty("ChoiceBubbleLabel", typeof (string), string.Empty);

        /// <summary>Signifies the Bubble represents a correct answer.</summary>
        public bool IsACorrectValue
        {
            get { return GetValue<bool>(IsACorrectValueProperty); }
            set { SetValue(IsACorrectValueProperty, value); }
        }

        public static readonly PropertyData IsACorrectValueProperty = RegisterProperty("IsACorrectValue", typeof (bool), false);

        /// <summary>Indicates the choice bubble has been marked by ink.</summary>
        public bool IsMarked
        {
            get { return GetValue<bool>(IsMarkedProperty); }
            set { SetValue(IsMarkedProperty, value); }
        }

        public static readonly PropertyData IsMarkedProperty = RegisterProperty("IsMarked", typeof (bool), false);

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
    public class MultipleChoiceBox : AStrokeAccepter
    {
        #region Constructor

        public MultipleChoiceBox() { }

        public MultipleChoiceBox(CLPPage parentPage, int numberOfChoices, string correctAnswerLabel, MultipleChoiceOrientations orientation, MultipleChoiceLabelTypes labelType)
            : base(parentPage)
        {
            Orientation = orientation;
            Height = orientation == MultipleChoiceOrientations.Horizontal ? ChoiceBubbleDiameter : ChoiceBubbleDiameter * numberOfChoices + 5 * (numberOfChoices - 1);
            Width = orientation == MultipleChoiceOrientations.Vertical ? ChoiceBubbleDiameter : ChoiceBubbleDiameter * numberOfChoices + 5 * (numberOfChoices - 1);

            foreach (var choiceBubble in Enumerable.Range(0, numberOfChoices).Select(i => new MultipleChoiceBubble(i, labelType)))
            {
                choiceBubble.IsACorrectValue = correctAnswerLabel == choiceBubble.ChoiceBubbleLabel;
                ChoiceBubbles.Add(choiceBubble);
            }
        }

        public MultipleChoiceBox(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructor

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

        /// <summary>List of the available choices for the Multiple Choice Box.</summary>
        public List<MultipleChoiceBubble> ChoiceBubbles
        {
            get { return GetValue<List<MultipleChoiceBubble>>(ChoiceBubblesProperty); }
            set { SetValue(ChoiceBubblesProperty, value); }
        }

        public static readonly PropertyData ChoiceBubblesProperty = RegisterProperty("ChoiceBubbles", typeof (List<MultipleChoiceBubble>), () => new List<MultipleChoiceBubble>());

        #endregion //Properties

        #region APageObjectBase Overrides

        public override string FormattedName
        {
            get { return "Multiple Choice Box"; }
        }

        public override string CodedName
        {
            get
            {
                return "MC";
            }
        }

        public override string CodedID
        {
            get
            {
                return "[]"; //marked letter, incorrect/correct
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