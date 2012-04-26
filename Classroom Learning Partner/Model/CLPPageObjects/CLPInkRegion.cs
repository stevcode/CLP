using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using Microsoft.Ink;
using Classroom_Learning_Partner.Resources;
using Catel.Data;
using System.Collections.ObjectModel;
using System.Threading;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{

    [Serializable]
    public abstract class CLPInkRegion : CLPPageObjectBase
    {

        #region Variables

        private Object interpretation_lock = new Object();

        #endregion //Variables

        #region Constructors

        public CLPInkRegion() : base()
        {
            CanAcceptStrokes = true;
            Position = new Point(100, 100);
            Height = 100;
            Width = 100;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPInkRegion(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Methods

        protected override void OnDeserialized()
        {
            lock (interpretation_lock)
            {
                base.OnDeserialized();
            }
        }

        [OnSerializing]
        void OnSerializing(StreamingContext sc)
        {
            InterpretStrokes();
        }

        public abstract void DoInterpretation();

        public void InterpretStrokes()
        {
            lock (interpretation_lock)
            {
                DoInterpretation();
            }
        }

        public override string PageObjectType
        {
            get { return "CLPInkRegion"; }
        }

        public override ICLPPageObject Duplicate()
        {
            lock (interpretation_lock)
            {
                CLPInkRegion newInkRegion = this.Clone() as CLPInkRegion;
                newInkRegion.UniqueID = Guid.NewGuid().ToString();
                return newInkRegion;
            }
        }

        public override void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
            this.ProcessStrokes(addedStrokes, removedStrokes);
            Thread t = new Thread(new ThreadStart(this.InterpretStrokes));
            t.Name = "Ink Interpretation Thread";
            t.Start();
        }

        #endregion

        
    }
}