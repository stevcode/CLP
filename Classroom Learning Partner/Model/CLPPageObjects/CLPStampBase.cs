using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Classroom_Learning_Partner.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    abstract public class  CLPStampBase : CLPPageObjectBase
    {
        protected CLPStampBase() : base()
        {
            MetaData.SetValue("IsAnchored", "True");
            MetaData.SetValue("Parts", "0");
            base.Position = new Point(10, 10);
            Width = 150;
        }

        #region Properties

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

        #endregion //Properties
    }
}