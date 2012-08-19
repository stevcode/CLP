using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Catel.Data;

namespace Classroom_Learning_Partner.Model
{
    [Serializable]
    public class CLPNamedInkSet : DataObjectBase<CLPNamedInkSet>
    {
        public CLPNamedInkSet(string shapeName, ObservableCollection<string> strokes)
        {
            InkShapeType = shapeName;
            InkShapeStrokes = strokes;
        }

        public CLPNamedInkSet()
        {
            InkShapeType = "";
            InkShapeStrokes = new ObservableCollection<string>();
        }

        protected CLPNamedInkSet(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        /// <summary>
        /// Strokes that make up the shape
        /// </summary>
        public ObservableCollection<string> InkShapeStrokes
        {
            get { return GetValue<ObservableCollection<string>>(InkShapeStrokesProperty); }
            set { SetValue(InkShapeStrokesProperty, value); }
        }

        /// <summary>
        /// Register the InkShapeStrokes property so it is known in the class.
        /// </summary>
        public static readonly PropertyData InkShapeStrokesProperty = RegisterProperty("InkShapesStrokes", typeof(ObservableCollection<string>), () => new ObservableCollection<string>());

        /// <summary>
        /// The type of the shape
        /// </summary>
        public string InkShapeType
        {
            get { return GetValue<string>(InkShapeTypeProperty); }
            set { SetValue(InkShapeTypeProperty, value); }
        }

        /// <summary>
        /// Register the InkShapeType property so it is known in the class.
        /// </summary>
        public static readonly PropertyData InkShapeTypeProperty = RegisterProperty("InkShapeType", typeof(string), "");
    }
}
