using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.Model.CLPPageObjects;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    public class CLPSnapTileViewModel : CLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the CLPSnapTileViewModel class.
        /// </summary>
        public CLPSnapTileViewModel(CLPSnapTile tile, CLPPageViewModel pageViewModel)
            : base(pageViewModel)
        {
            PageObject = tile;
            
        }

        public CLPSnapTileViewModel NextTile { get; set; }
        public CLPSnapTileViewModel PrevTile { get; set; }


    }
}
