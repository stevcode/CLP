using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
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

        public ChoiceBubble(int index, MultipleChoiceLabelTypes labelType)
        {
            BubbleContent = labelType == MultipleChoiceLabelTypes.Numbers ? (index + 1).ToString() : IntToUpperLetter(index + 1);
        }

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
        public string BubbleContent
        {
            get { return GetValue<string>(BubbleContentProperty); }
            set { SetValue(BubbleContentProperty, value); }
        }

        public static readonly PropertyData BubbleContentProperty = RegisterProperty("BubbleContent", typeof (string), string.Empty);

        /// <summary>The answer associated with the bubble.</summary>
        public string Answer
        {
            get { return GetValue<string>(AnswerProperty); }
            set { SetValue(AnswerProperty, value); }
        }

        public static readonly PropertyData AnswerProperty = RegisterProperty("Answer", typeof (string), string.Empty);

        /// <summary>Extra, non-relevant answer data.</summary>
        public string AnswerLabel
        {
            get { return GetValue<string>(AnswerLabelProperty); }
            set { SetValue(AnswerLabelProperty, value); }
        }

        public static readonly PropertyData AnswerLabelProperty = RegisterProperty("AnswerLabel", typeof (string), string.Empty);

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
            get { return string.Format("{0} \"{1}\"", BubbleContent, Answer); }
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

        public MultipleChoice(CLPPage parentPage)
            : base(parentPage)
        {
            Height = ChoiceBubbleDiameter;
            Width = 165;
        }

        #endregion //Constructors

        #region Properties

        public double ChoiceBubbleDiameter
        {
            get { return 35.0; }
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

        #region Methods

        public void SetOffsets()
        {
            var segmentWidth = (Orientation == MultipleChoiceOrientations.Horizontal ? Width : Height) / ChoiceBubbles.Count;
            var offset = 0.0;
            foreach (var choiceBubble in ChoiceBubbles)
            {
                choiceBubble.Offset = offset;
                offset += segmentWidth;
            }
        } 

        #endregion // Methods

        #region APageObjectBase Overrides

        public override string FormattedName
        {
            get { return "Multiple Choice"; }
        }

        public override string CodedName
        {
            get { return "ANS MC"; }
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
                SetOffsets();
            }
            base.OnPropertyChanged(e);
        }

        #endregion //APageObjectBase Overrides

        #region AStrokeAccepter Overrides

        // TODO: Rename to IsStrokeAccepted
        public override bool IsStrokeOverPageObject(Stroke stroke)
        {
            var choiceBubbleStrokeIsOver = ChoiceBubbleStrokeIsOver(stroke);

            return choiceBubbleStrokeIsOver != null;
        }

        /// <summary>Stroke must be at least this percent contained by pageObject.</summary>
        public override int StrokeHitTestPercentage
        {
            get { return 30; }
        }

        #endregion //AStrokeAccepter Overrides

        #region AStrokeAccepter Helpers

        public ChoiceBubble ChoiceBubbleStrokeIsOver(Stroke stroke)
        {
            return (from choiceBubble in ChoiceBubbles
                    let x = Orientation == MultipleChoiceOrientations.Horizontal ? XPosition + choiceBubble.Offset : XPosition
                    let y = Orientation == MultipleChoiceOrientations.Vertical ? YPosition + choiceBubble.Offset : YPosition
                    let rect = new Rect(x, y, ChoiceBubbleDiameter, ChoiceBubbleDiameter)
                    where stroke.HitTest(rect, StrokeHitTestPercentage)
                    select choiceBubble).FirstOrDefault();
        }

        public List<Stroke> StrokesOverChoiceBubble(ChoiceBubble choiceBubble)
        {
            var x = Orientation == MultipleChoiceOrientations.Horizontal ? XPosition + choiceBubble.Offset : XPosition;
            var y = Orientation == MultipleChoiceOrientations.Vertical ? YPosition + choiceBubble.Offset : YPosition;
            var bubbleBoundary = new Rect(x, y, ChoiceBubbleDiameter, ChoiceBubbleDiameter);

            var strokesOverBubble = AcceptedStrokes.Where(s => s.HitTest(bubbleBoundary, StrokeHitTestPercentage)).ToList();
            return strokesOverBubble;
        } 

        #endregion // AStrokeAccepter Helpers
    }
}