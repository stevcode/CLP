using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class SemanticEventPass : ASerializableBase
    {
        #region Constructor

        public SemanticEventPass(string passName, int passNumber, IEnumerable<ISemanticEvent> semanticEvents)
        {
            PassName = passName;
            PassNumber = passNumber;
            SemanticEvents = new ObservableCollection<ISemanticEvent>(semanticEvents);

            var eventIndex = 0;
            foreach (var semanticEvent in SemanticEvents)
            {
                semanticEvent.SemanticEventIndex = eventIndex;
                eventIndex++;
            }
        }

        #endregion // Constructor

        #region Properties

        /// <summary>Name of the pass.</summary>
        public string PassName
        {
            get => GetValue<string>(PassNameProperty);
            set => SetValue(PassNameProperty, value);
        }

        public static readonly PropertyData PassNameProperty = RegisterProperty(nameof(PassName), typeof(string), string.Empty);

        /// <summary>Number of the pass.</summary>
        public int PassNumber
        {
            get => GetValue<int>(PassNumberProperty);
            set => SetValue(PassNumberProperty, value);
        }

        public static readonly PropertyData PassNumberProperty = RegisterProperty(nameof(PassNumber), typeof(int), 0);

        /// <summary>Semantic Events that make up this pass</summary>
        public ObservableCollection<ISemanticEvent> SemanticEvents
        {
            get => GetValue<ObservableCollection<ISemanticEvent>>(SemanticEventsProperty);
            set => SetValue(SemanticEventsProperty, value);
        }

        public static readonly PropertyData SemanticEventsProperty =
            RegisterProperty(nameof(SemanticEvents), typeof(ObservableCollection<ISemanticEvent>), () => new ObservableCollection<ISemanticEvent>());

        #endregion // Properties
    }
}