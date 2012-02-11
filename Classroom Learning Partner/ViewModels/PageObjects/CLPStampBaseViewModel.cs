using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Windows;
using System.Windows.Media;
using Classroom_Learning_Partner.Model;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    abstract public class CLPStampBaseViewModel : CLPPageObjectBaseViewModel
    {

        #region Constructors

        public CLPStampBaseViewModel(CLPStampBase stamp, CLPPageViewModel pageViewModel)
            : base(pageViewModel)
        {
            PageObject = stamp;
            IsAnchored = stamp.IsAnchored;
            Parts = stamp.Parts;
        }

        #endregion //Constructors

        #region Bindings

        /// <summary>
        /// The <see cref="IsAnchor" /> property's name.
        /// </summary>
        public const string IsAnchorPropertyName = "IsAnchored";

        private bool _isAnchored = true;

        /// <summary>
        /// Sets and gets the IsAnchor property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsAnchored
        {
            get
            {
                return _isAnchored;
            }

            set
            {
                if (_isAnchored == value)
                {
                    return;
                }

                _isAnchored = value;
                RaisePropertyChanged(IsAnchorPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Parts" /> property's name.
        /// </summary>
        public const string PartsPropertyName = "Parts";

        private int _parts = 0;

        /// <summary>
        /// Sets and gets the Parts property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int Parts
        {
            get
            {
                return _parts;
            }

            set
            {
                if (_parts == value)
                {
                    return;
                }

                _parts = value;
                //bad, quick hack, update to set database values
                (PageObject as CLPStampBase).Parts = value;
                RaisePropertyChanged(PartsPropertyName);
            }
        }

        #endregion //Bindings
    }
}