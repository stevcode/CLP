using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    [Serializable]
    public class CLPTextBox : CLPPageObjectBase
    {
        public CLPTextBox() : this("")
        { 
        }

        public CLPTextBox(string s) : base()
        {
            _text = s;
            Position = new Point(100, 100);
            Height = 200;
            Width = 400;
        }

        private string _text;
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
            }
        }
    }
}
