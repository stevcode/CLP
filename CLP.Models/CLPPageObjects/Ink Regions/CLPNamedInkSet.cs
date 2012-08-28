using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPNamedInkSet : DataObjectBase<CLPNamedInkSet>
    {
        public CLPNamedInkSet(string shapeName, ObservableCollection<byte[]> strokes)
        {
            InkShapeType = shapeName;
            InkShapeStrokes = strokes;
        }

        public CLPNamedInkSet()
        {
            InkShapeType = "";
            InkShapeStrokes = new ObservableCollection<byte[]>();
        }

        protected CLPNamedInkSet(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        /// <summary>
        /// Strokes that make up the shape
        /// </summary>
        public ObservableCollection<byte[]> InkShapeStrokes
        {
            get { return GetValue<ObservableCollection<byte[]>>(InkShapeStrokesProperty); }
            set { SetValue(InkShapeStrokesProperty, value); }
        }

        /// <summary>
        /// Register the InkShapeStrokes property so it is known in the class.
        /// </summary>
        public static readonly PropertyData InkShapeStrokesProperty = RegisterProperty("InkShapesStrokes", typeof(ObservableCollection<byte[]>), () => new ObservableCollection<byte[]>());

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
