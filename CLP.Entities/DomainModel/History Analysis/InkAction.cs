﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace CLP.Entities
{
    public class InkAction : AHistoryActionBase
    {
        public enum InkActions
        {
            Change,
            Add,
            Erase,
            Ignore
        }

        public enum InkLocations
        {
            None,
            Over,
            Left,
            Right,
            Top,
            Bottom
        }

        #region Constructors

        /// <summary>Initializes <see cref="InkAction" /> using <see cref="CLPPage" />.</summary>
        public InkAction(CLPPage parentPage, List<IHistoryItem> historyItems)
            : base(parentPage)
        {
            HistoryItemIDs = historyItems.Select(h => h.ID).ToList();
            foreach (var historyItem in historyItems)
            {
                var pageChangedHistoryItem = (ObjectsOnPageChangedHistoryItem)historyItem;
                if (!pageChangedHistoryItem.StrokeIDsAdded.Any() && 
                    !pageChangedHistoryItem.StrokeIDsRemoved.Any()) 
                {
                    //throw error, no strokes
                }
                if(pageChangedHistoryItem.PageObjectIDsAdded.Any() ||
                    pageChangedHistoryItem.PageObjectIDsRemoved.Any())
                {
                    //throw error, items besides strokes
                }
            }
        }

        /// <summary>Initializes <see cref="InkAction" /> using <see cref="CLPPage" />.</summary>
        public InkAction(CLPPage parentPage, List<InkAction> inkActions, List<IHistoryItem> historyItems, InkActions inkActionType)
            : base(parentPage)
        {
            HistoryItemIDs = historyItems.Select(h => h.ID).ToList();
            HistoryActionIDs = inkActions.Select(i => i.ID).ToList();

            InkActionType = inkActionType;

            foreach (var historyItem in HistoryItems)
            {
                var pageHistoryItem = (ObjectsOnPageChangedHistoryItem)historyItem;
                //validate that all adds or removed
                if (inkActionType == InkActions.Add)
                {
                    if (pageHistoryItem.StrokeIDsRemoved.Any() ||
                        pageHistoryItem.PageObjectIDsAdded.Any() ||
                        pageHistoryItem.PageObjectIDsRemoved.Any())
                    {
                        //throw error
                    }
                }
                else if (inkActionType == InkActions.Erase)
                {
                    if (pageHistoryItem.StrokeIDsAdded.Any() ||
                        pageHistoryItem.PageObjectIDsAdded.Any() ||
                        pageHistoryItem.PageObjectIDsRemoved.Any())
                    {
                        //throw error
                    }
                }
            }
        }

        /// <summary>Initializes <see cref="InkAction" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public InkAction(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        //Make string CollectionID

        /// <summary>
        /// The type of Ink Action this HistoryAction represents.
        /// </summary>
        public InkActions InkActionType
        {
            get { return GetValue<InkActions>(InkActionTypeProperty); }
            set { SetValue(InkActionTypeProperty, value); }
        }

        public static readonly PropertyData InkActionTypeProperty = RegisterProperty("InkActionType", typeof(InkActions));

        /// <summary>
        /// Location the Ink Action occurs.
        /// </summary>
        public InkLocations InkLocation
        {
            get { return GetValue<InkLocations>(InkLocationProperty); }
            set { SetValue(InkLocationProperty, value); }
        }

        public static readonly PropertyData InkLocationProperty = RegisterProperty("InkLocation", typeof (InkLocations));

        public override string CodedValue
        {
            get
            {
                if (InkActionType == InkActions.Change)
                {
                    return "INK change";
                }

                if (InkActionType == InkActions.Ignore)
                {
                    return "";
                }

                var codedActionType = InkActionType.ToString().ToLower();
                //get location
                //get relative object
                //get type (add or remove)
                return string.Format("INK {1} {2}", codedActionType, "[A]");
            }
        } 

        #endregion //Properties
    }
}
