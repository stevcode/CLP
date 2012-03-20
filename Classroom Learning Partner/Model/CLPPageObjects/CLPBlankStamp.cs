using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{

    [Serializable]
    public class CLPBlankStamp : CLPStampBase
    {
        public CLPBlankStamp() : base()
        {
            Height = 150;
            IsAnchored = true;
        }

        public CLPPageObjectBase Copy()
        {
            CLPBlankStamp newStamp = new CLPBlankStamp();
            //copy all metadata and create new unique ID/creation date for the moved stamp
            newStamp.IsAnchored = IsAnchored;
            newStamp.Parts = Parts;
            newStamp.Position = Position;
            newStamp.Height = Height;
            newStamp.Width = Width;
            foreach (var stringStroke in PageObjectStrokes)
            {
                newStamp.PageObjectStrokes.Add(stringStroke);
            }

            return newStamp;
        }

        public override CLPPageObjectBase Duplicate()
        {
            CLPBlankStamp newStamp = this.Clone() as CLPBlankStamp;
            newStamp.UniqueID = Guid.NewGuid().ToString();

            return newStamp;
        }

        public override string PageObjectType
        {
            get { return "CLPBlankStamp"; }
        }
    }
}
