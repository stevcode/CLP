using System.Collections.ObjectModel;
using System.Linq;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities.Demo;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>UserControl view model.</summary>
    public class MultipleChoiceCreationViewModel : ViewModelBase
    {
        #region Constructor

        /// <summary>Initializes a new instance of the <see cref="MultipleChoiceCreationViewModel" /> class.</summary>
        public MultipleChoiceCreationViewModel(MultipleChoice multipleChoice)
        {
            Model = multipleChoice;

            if (!ChoiceBubbles.Any())
            {
                ChoiceBubbles.Add(new ChoiceBubble(0, LabelType));
                ChoiceBubbles.Add(new ChoiceBubble(1, LabelType));
            }

            AddChoiceBubbleCommand = new Command(OnAddChoiceBubbleCommandExecute);
        }

        /// <summary>Gets the title of the view model.</summary>
        /// <value>The title.</value>
        public override string Title
        {
            get { return "MultipleChoiceCreationVM"; }
        }

        #endregion //Constructor

        #region Model

        /// <summary>Gets or sets the property value.</summary>
        [Model(SupportIEditableObject = false)]
        public MultipleChoice Model
        {
            get { return GetValue<MultipleChoice>(ModelProperty); }
            private set { SetValue(ModelProperty, value); }
        }

        public static readonly PropertyData ModelProperty = RegisterProperty("Model", typeof (MultipleChoice));

        /// <summary>List of the available choices for the Multiple Choice Box.</summary>
        [ViewModelToModel("Model")]
        public ObservableCollection<ChoiceBubble> ChoiceBubbles
        {
            get { return GetValue<ObservableCollection<ChoiceBubble>>(ChoiceBubblesProperty); }
            set { SetValue(ChoiceBubblesProperty, value); }
        }

        public static readonly PropertyData ChoiceBubblesProperty = RegisterProperty("ChoiceBubbles", typeof (ObservableCollection<ChoiceBubble>));

        #endregion //Model

        #region Bindings

        /// <summary>label type for the choice bubles.</summary>
        public MultipleChoiceLabelTypes LabelType
        {
            get { return GetValue<MultipleChoiceLabelTypes>(LabelTypeProperty); }
            set { SetValue(LabelTypeProperty, value); }
        }

        public static readonly PropertyData LabelTypeProperty = RegisterProperty("LabelType", typeof (MultipleChoiceLabelTypes), MultipleChoiceLabelTypes.Letters);

        #endregion // Bindings

        #region Commands

        /// <summary>Adds a zero value factor to the relation definition.</summary>
        public Command AddChoiceBubbleCommand { get; private set; }

        private void OnAddChoiceBubbleCommandExecute()
        {
            var nextIndex = ChoiceBubbles.Count;
            ChoiceBubbles.Add(new ChoiceBubble(nextIndex, LabelType));
        }

        #endregion //Commands
    }
}