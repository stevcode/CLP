using System.Collections.Generic;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class MultipleChoiceBoxViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        public MultipleChoiceBoxViewModel(MultipleChoiceBox multipleChoiceBox) { PageObject = multipleChoiceBox; }

        #endregion //Constructor

        #region Model

        /// <summary>
        /// List of the available choices for the Multiple Choice Box.
        /// </summary>
        [ViewModelToModel("PageObject")] 
        public List<MultipleChoiceBubble> ChoiceBubbles
        {
            get { return GetValue<List<MultipleChoiceBubble>>(ChoiceBubblesProperty); }
            set { SetValue(ChoiceBubblesProperty, value); }
        }

        public static readonly PropertyData ChoiceBubblesProperty = RegisterProperty("ChoiceBubbles", typeof(List<MultipleChoiceBubble>)); 

        #endregion //Model

        #region Static Methods

        public static void AddMultipleChoiceBoxToPage(CLPPage page)
        {
            var multipleChoiceBox = new MultipleChoiceBox(page, 4, "C", MultipleChoiceOrientations.Horizontal, MultipleChoiceLabelTypes.Letters);
            ACLPPageBaseViewModel.AddPageObjectToPage(multipleChoiceBox);
        } 

        #endregion //Static Methods
    }
}