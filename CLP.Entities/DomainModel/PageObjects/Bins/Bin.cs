﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Catel.Collections;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities
{
    public class Bin : APageObjectBase, ICountable, IPageObjectAccepter
    {
        #region Constructors

        /// <summary>Initializes <see cref="Bin" /> from scratch.</summary>
        public Bin() { }

        /// <summary>Initializes <see cref="Bin" /> from <see cref="ShapeType" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="Bin" /> belongs to.</param>
        public Bin(CLPPage parentPage)
            : base(parentPage)
        {
            Height = 165 + PartsHeight;
            Width = 165;
        }

        /// <summary>Initializes <see cref="Bin" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public Bin(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public double PartsHeight
        {
            get { return 20; }
        }

        #endregion //Properties

        #region APageObjectBase Overrides

        public override string FormattedName
        {
            get { return string.Format("Bin of {0}", Parts); }
        }

        public override int ZIndex
        {
            get { return 70; }
        }

        public override bool IsBackgroundInteractable
        {
            get { return true; }
        }

        public override void OnAdded(bool fromHistory = false)
        {
            if (!CanAcceptPageObjects ||
                !AcceptedPageObjects.Any() ||
                !fromHistory)
            {
                return;
            }

            var pageObjectsToRestore = new List<IPageObject>();

            foreach (var pageObject in AcceptedPageObjects.Where(p => ParentPage.History.TrashedPageObjects.Contains(p)))
            {
                pageObjectsToRestore.Add(pageObject);
            }

            ParentPage.PageObjects.AddRange(pageObjectsToRestore);
            foreach (var pageObject in pageObjectsToRestore)
            {
                ParentPage.History.TrashedPageObjects.Remove(pageObject);
            }

            base.OnAdded(fromHistory);
        }

        public override void OnDeleted(bool fromHistory = false)
        {
            if (!CanAcceptPageObjects ||
                !AcceptedPageObjects.Any())
            {
                return;
            }

            var pageObjectsToTrash = new List<IPageObject>();

            foreach (var pageObject in AcceptedPageObjects.Where(p => ParentPage.PageObjects.Contains(p)))
            {
                pageObjectsToTrash.Add(pageObject);
            }

            foreach (var pageObject in pageObjectsToTrash)
            {
                ParentPage.PageObjects.Remove(pageObject);
            }
            
            ParentPage.History.TrashedPageObjects.AddRange(pageObjectsToTrash);

            base.OnDeleted(fromHistory);
        }

        public override void OnMoving(double oldX, double oldY, bool fromHistory = false)
        {
            var deltaX = XPosition - oldX;
            var deltaY = YPosition - oldY;

            if (!CanAcceptPageObjects)
            {
                return;
            }

            foreach (var pageObject in AcceptedPageObjects)
            {
                pageObject.XPosition += deltaX;
                pageObject.YPosition += deltaY;
            }
        }

        public override void OnMoved(double oldX, double oldY, bool fromHistory = false)
        {
            OnMoving(oldX, oldY, fromHistory);
            base.OnMoved(oldX, oldY, fromHistory);
        }

        public override IPageObject Duplicate()
        {
            var newBin = Clone() as Bin;
            if (newBin == null)
            {
                return null;
            }
            newBin.CreationDate = DateTime.Now;
            newBin.ID = Guid.NewGuid().ToCompactID();
            newBin.VersionIndex = 0;
            newBin.LastVersionIndex = null;
            newBin.ParentPage = ParentPage;

            return newBin;
        }

        #endregion //APageObjectBase Overrides

        #region ICountable Implementation

        /// <summary>Number of parts the <see cref="Bin" /> represents.</summary>
        public int Parts
        {
            get { return GetValue<int>(PartsProperty); }
            set { SetValue(PartsProperty, value); }
        }

        public static readonly PropertyData PartsProperty = RegisterProperty("Parts", typeof (int), 0);

        /// <summary>Is an <see cref="ICountable" /> that doesn't accept inner parts.</summary>
        public bool IsInnerPart
        {
            get { return GetValue<bool>(IsInnerPartProperty); }
            set { SetValue(IsInnerPartProperty, value); }
        }

        public static readonly PropertyData IsInnerPartProperty = RegisterProperty("IsInnerPart", typeof (bool), false);

        /// <summary>Parts is Auto-Generated and non-modifiable (except under special circumstances).</summary>
        public bool IsPartsAutoGenerated
        {
            get { return GetValue<bool>(IsPartsAutoGeneratedProperty); }
            set { SetValue(IsPartsAutoGeneratedProperty, value); }
        }

        public static readonly PropertyData IsPartsAutoGeneratedProperty = RegisterProperty("IsPartsAutoGenerated", typeof (bool), true);

        public void RefreshParts()
        {
            Parts = 0;
            foreach (var pageObject in AcceptedPageObjects.OfType<ICountable>())
            {
                Parts += pageObject.Parts;
            }
        }

        #endregion //ICountable Implementation

        #region IPageObjectAccepter Implementation

        /// <summary>Determines whether the <see cref="Stamp" /> can currently accept <see cref="IPageObject" />s.</summary>
        public bool CanAcceptPageObjects
        {
            get { return GetValue<bool>(CanAcceptPageObjectsProperty); }
            set { SetValue(CanAcceptPageObjectsProperty, value); }
        }

        public static readonly PropertyData CanAcceptPageObjectsProperty = RegisterProperty("CanAcceptPageObjects", typeof (bool), true);

        /// <summary>The currently accepted <see cref="IPageObject" />s.</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public List<IPageObject> AcceptedPageObjects
        {
            get { return GetValue<List<IPageObject>>(AcceptedPageObjectsProperty); }
            set { SetValue(AcceptedPageObjectsProperty, value); }
        }

        public static readonly PropertyData AcceptedPageObjectsProperty = RegisterProperty("AcceptedPageObjects", typeof (List<IPageObject>), () => new List<IPageObject>());

        /// <summary>The IDs of the <see cref="IPageObject" />s that have been accepted.</summary>
        public List<string> AcceptedPageObjectIDs
        {
            get { return GetValue<List<string>>(AcceptedPageObjectIDsProperty); }
            set { SetValue(AcceptedPageObjectIDsProperty, value); }
        }

        public static readonly PropertyData AcceptedPageObjectIDsProperty = RegisterProperty("AcceptedPageObjectIDs", typeof (List<string>), () => new List<string>());

        public void AcceptPageObjects(IEnumerable<IPageObject> addedPageObjects, IEnumerable<IPageObject> removedPageObjects)
        {
            if (!CanAcceptPageObjects)
            {
                return;
            }

            foreach (var pageObject in removedPageObjects.Where(pageObject => AcceptedPageObjectIDs.Contains(pageObject.ID)))
            {
                AcceptedPageObjects.Remove(pageObject);
                AcceptedPageObjectIDs.Remove(pageObject.ID);
            }

            foreach (var pageObject in addedPageObjects.OfType<Mark>())
            {
                AcceptedPageObjects.Add(pageObject);
                AcceptedPageObjectIDs.Add(pageObject.ID);
            }

            RefreshParts();
        }

        public void RefreshAcceptedPageObjects()
        {
            AcceptedPageObjects.Clear();
            AcceptedPageObjectIDs.Clear();
            if (!CanAcceptPageObjects)
            {
                return;
            }

            var pageObjectsOverStamp = from pageObject in ParentPage.PageObjects
                                       where PageObjectIsOver(pageObject, .90) //PageObject must be at least 90% contained by Bin.
                                       select pageObject;

            AcceptPageObjects(pageObjectsOverStamp, new List<IPageObject>());
        }

        #endregion //IPageObjectAccepter Implementation
    }
}