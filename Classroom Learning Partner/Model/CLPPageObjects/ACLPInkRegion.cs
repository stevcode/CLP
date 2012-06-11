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
using System.Windows.Threading;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{

    [Serializable]
    public abstract class ACLPInkRegion : CLPPageObjectBase
    {

        #region Variables

        private Object interpretation_lock = new Object();

        #endregion //Variables

        #region Constructors

        public ACLPInkRegion() : base()
        {
            CanAcceptStrokes = true;
            Position = new Point(100, 100);
            Height = 100;
            Width = 100;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += new EventHandler(timer_Tick);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            //Thread t = new Thread(new ThreadStart(this.InterpretStrokes));
            //t.Name = "Ink Interpretation Thread";
            //t.Start();

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
            (DispatcherOperationCallback)delegate(object arg)
            {
                InterpretStrokes();
                return null;
            }, null);
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected ACLPInkRegion(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Methods

        protected override void OnDeserialized()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += new EventHandler(timer_Tick);
        }

        [OnSerializing]
        void OnSerializing(StreamingContext sc)
        {
            //timer.Stop();
            //lock (interpretation_lock)
            //{
                if (App.CurrentUserMode == App.UserMode.Student)
                {
                	DoInterpretation();
                }
            //}
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
                ACLPInkRegion newInkRegion = this.Clone() as ACLPInkRegion;
                newInkRegion.UniqueID = Guid.NewGuid().ToString();
                return newInkRegion;
            }
        }

        private DispatcherTimer timer = null;

        public override void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
            //timer.Stop();
            this.ProcessStrokes(addedStrokes, removedStrokes);
            //timer.Start();
        }

        #endregion

    }
}