using System;

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

        public override CLPPageObjectBase Copy()
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
    }
}
