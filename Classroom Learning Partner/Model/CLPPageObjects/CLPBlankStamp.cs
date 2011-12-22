using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    public interface ICLPStamp
    {
    }

    [Serializable]
    public class CLPBlankStamp : CLPPageObjectBase, ICLPStamp
    {
        public CLPBlankStamp() : base()
        {
            Height = 150;
            Width = 150;
            base.Position = new Point(10, 10);
            IsAnchored = true;

            MetaData.SetValue("IsAnchored", "True");
            MetaData.SetValue("Parts", "0");
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


        //returns true if stamp is anchor/placed by teacher
        //returns false if stamp is a copy of the anchor; moved by the student
        public bool IsAnchored
        {
            get
            {
                if (MetaData.GetValue("IsAnchored") == "True")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value)
                {
                    MetaData.SetValue("IsAnchored", "True");
                }
                else
                {
                    MetaData.SetValue("IsAnchored", "False");
                }
            }
        }

        public int Parts
        {
            get
            {
                if (MetaData.GetValue("Parts") == "")
                {
                    return 0;
                }
                else
                {
                    return Int32.Parse(MetaData.GetValue("Parts"));
                }  
            }
            set
            {
                MetaData.SetValue("Parts", value.ToString());
            }
        }
    }
}
