using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class SemanticEventEditorViewModel : ViewModelBase
    {
        private bool _isANSFIEvent = false;

        #region Constructor

        public SemanticEventEditorViewModel(ISemanticEvent semanticEvent)
        {
            SemanticEvent = semanticEvent;

            _isANSFIEvent = semanticEvent.CodedObject == Codings.OBJECT_FILL_IN;

            if (_isANSFIEvent)
            {
                ActualAnswer = semanticEvent.CodedObjectID;

                var studentAnswer = Codings.GetFinalAnswerEventStudentAnswer(semanticEvent);
                StudentAnswer = studentAnswer.Substring(1, studentAnswer.Length - 2); // Remove first and last quotes

                CodedCorrectness = Codings.GetFinalAnswerEventCorrectness(semanticEvent);
            }

            InitializeCommands();
        }

        #endregion // Constructor

        #region Model

        [Model]
        public ISemanticEvent SemanticEvent
        {
            get => GetValue<ISemanticEvent>(SemanticEventProperty);
            set => SetValue(SemanticEventProperty, value);
        }

        public static readonly PropertyData SemanticEventProperty = RegisterProperty(nameof(SemanticEvent), typeof(ISemanticEvent));

        #endregion // Model

        #region Bindings

        public string ActualAnswer
        {
            get => GetValue<string>(ActualAnswerProperty);
            set => SetValue(ActualAnswerProperty, value);
        }

        public static readonly PropertyData ActualAnswerProperty = RegisterProperty(nameof(ActualAnswer), typeof(string), string.Empty);

        public string StudentAnswer
        {
            get => GetValue<string>(StudentAnswerProperty);
            set => SetValue(StudentAnswerProperty, value);
        }

        public static readonly PropertyData StudentAnswerProperty = RegisterProperty(nameof(StudentAnswer), typeof(string), string.Empty);

        public string CodedCorrectness
        {
            get => GetValue<string>(CodedCorrectnessProperty);
            set => SetValue(CodedCorrectnessProperty, value);
        }

        public static readonly PropertyData CodedCorrectnessProperty = RegisterProperty(nameof(CodedCorrectness), typeof(string), string.Empty);

        #endregion // Bindings

        #region Commands

        private void InitializeCommands()
        {
            ConfirmChangesCommand = new TaskCommand(OnConfirmChangesCommandExecuteAsync);
            CancelChangesCommand = new TaskCommand(OnCancelChangesCommandExecuteAsync);
        }

        /// <summary>Validates and confirms changes to the Semantic Event.</summary>
        public TaskCommand ConfirmChangesCommand { get; private set; }

        private async Task OnConfirmChangesCommandExecuteAsync()
        {
            if (_isANSFIEvent)
            {
                var eventInfo = SemanticEvent.EventInformation;
                var delimiterIndex = eventInfo.LastIndexOf(',');
                var interpretationOfChangedStroke = new string(eventInfo.Take(delimiterIndex).ToArray());
                interpretationOfChangedStroke = interpretationOfChangedStroke.Split(';').First().Trim();

                SemanticEvent.EventInformation = $"{interpretationOfChangedStroke}; \"{StudentAnswer}\", {CodedCorrectness}";
            }

            await SaveViewModelAsync();
            await CloseViewModelAsync(true);
        }

        /// <summary>Cancels changes to the Semantic Event.</summary>
        public TaskCommand CancelChangesCommand { get; private set; }

        private async Task OnCancelChangesCommandExecuteAsync()
        {
            await CancelViewModelAsync();
            await CloseViewModelAsync(false);
        }

        #endregion // Commands
    }
}
