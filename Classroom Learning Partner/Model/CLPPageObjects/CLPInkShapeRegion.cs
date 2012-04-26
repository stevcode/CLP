﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.Resources;
using Catel.Data;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Windows.Ink;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    [Serializable]
    public class CLPInkShapeRegion : CLPInkRegion
    {
        #region Constructors

        public CLPInkShapeRegion() : base()
        {
            InkShapesString = "";
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPInkShapeRegion(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion // Constructors

        #region Properties

        public override string PageObjectType
        {
            get { return "CLPInkShapeRegion"; }
        }

        /// <summary>
        /// Stored interpreted answer.
        /// </summary>
        public string InkShapesString
        {
            get { return GetValue<string>(InkShapesStringProperty); }
            set { SetValue(InkShapesStringProperty, value); }
        }

        /// <summary>
        /// Register the InkShapesString property so it is known in the class.
        /// </summary>
        public static readonly PropertyData InkShapesStringProperty = RegisterProperty("InkShapesString", typeof(string), "");

        #endregion // Properties

        #region Methods

        public override void DoInterpretation()
        {
            ObservableCollection<string> StrokesNoDuplicates = new ObservableCollection<string>(PageObjectStrokes.Distinct().ToList());
            ContextNodeCollection shapes = InkInterpretation.InterpretShapes(CLPPage.StringsToStrokes(StrokesNoDuplicates));
            if (shapes != null)
            {
                Console.WriteLine(shapes.Count);
                //InkShapes = shapes;
                StringBuilder text = new StringBuilder();
                foreach (InkDrawingNode shape in shapes)
                {
                    text.AppendLine(shape.GetShapeName());
                }
                InkShapesString = text.ToString();
            }
        }

        #endregion // Methods
    }
}
