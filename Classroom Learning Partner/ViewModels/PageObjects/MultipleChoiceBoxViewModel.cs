using System.Collections.Generic;
using System.Linq;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class MultipleChoiceBoxViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        public MultipleChoiceBoxViewModel(MultipleChoiceBox multipleChoiceBox) { PageObject = multipleChoiceBox; }

        #endregion //Constructor

        #region Bindings

        public List<string> ChoiceLabels
        {
            get
            {
                var choiceLabels = new List<string>();
                var multipleChoiceBox = PageObject as MultipleChoiceBox;
                if (multipleChoiceBox == null)
                {
                    return choiceLabels;
                }

                choiceLabels.AddRange(
                                      Enumerable.Range(1, multipleChoiceBox.NumberOfChoices)
                                                .Select(i => multipleChoiceBox.LabelType == MultipleChoiceLabelTypes.Numbers ? i.ToString() : ToUpperLetter(i)));

                return choiceLabels;
            }
        }

        #endregion //Bindings

        #region Methods

        private string ToUpperLetter(int index)
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
}