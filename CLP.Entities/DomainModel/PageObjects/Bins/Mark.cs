﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum MarkShapes
    {
        Square,
        Circle,
        Triangle,
        Tick,
        Custom
    }

    public class Mark : APageObjectBase, ICountable
    {
        #region Constructors

        /// <summary>Initializes <see cref="Mark" /> from scratch.</summary>
        public Mark() { }

        /// <summary>Initializes <see cref="Mark" /> from</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="Mark" /> belongs to.</param>
        public Mark(CLPPage parentPage, MarkShapes markShape, string color)
            : base(parentPage)
        {
            MarkShape = markShape;
            MarkColor = color;
            Height = 20;
            Width = 20;
        }

        /// <summary>Initializes <see cref="Mark" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public Mark(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>The shape of the Mark.</summary>
        public MarkShapes MarkShape
        {
            get { return GetValue<MarkShapes>(MarkShapeProperty); }
            set { SetValue(MarkShapeProperty, value); }
        }

        public static readonly PropertyData MarkShapeProperty = RegisterProperty("MarkShape", typeof (MarkShapes), MarkShapes.Square);

        /// <summary>The color of the Mark.</summary>
        public string MarkColor
        {
            get { return GetValue<string>(MarkColorProperty); }
            set { SetValue(MarkColorProperty, value); }
        }

        public static readonly PropertyData MarkColorProperty = RegisterProperty("MarkColor", typeof (string), "Black");

        #endregion //Properties

        #region APageObjectBase Overrides

        public override string FormattedName
        {
            get { return string.Format("{0} {1} Mark", MarkColor, MarkShape); }
        }

        public override int ZIndex
        {
            get { return 80; }
        }

        /// <summary>Determines whether the <see cref="IPageObject" /> has properties can be changed by a <see cref="Person" /> anyone at any time.</summary>
        public override bool IsBackgroundInteractable
        {
            get { return false; }
        }

        public override void OnMoved(double oldX, double oldY, bool fromHistory = false)
        {
            try
            {
                foreach (
                    var acceptorPageObject in ParentPage.PageObjects.OfType<IPageObjectAccepter>().Where(pageObject => pageObject.CanAcceptPageObjects && pageObject.ID != ID))
                {
                    var removedPageObjects = new List<IPageObject>();
                    var addedPageObjects = new ObservableCollection<IPageObject>();

                    if (acceptorPageObject.AcceptedPageObjectIDs.Contains(ID) &&
                        !acceptorPageObject.PageObjectIsOver(this, .50))
                    {
                        removedPageObjects.Add(this);
                    }

                    if (!acceptorPageObject.AcceptedPageObjectIDs.Contains(ID) &&
                        acceptorPageObject.PageObjectIsOver(this, .50))
                    {
                        addedPageObjects.Add(this);
                    }

                    acceptorPageObject.ChangeAcceptedPageObjects(addedPageObjects, removedPageObjects);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Mark.OnMoved() Exception: " + ex.Message);
            }

            base.OnMoved(oldX, oldY, fromHistory);
        }

        public override IPageObject Duplicate()
        {
            var newMark = Clone() as Mark;
            if (newMark == null)
            {
                return null;
            }
            newMark.CreationDate = DateTime.Now;
            newMark.ID = Guid.NewGuid().ToCompactID();
            newMark.VersionIndex = 0;
            newMark.LastVersionIndex = null;
            newMark.ParentPage = ParentPage;

            return newMark;
        }

        #endregion //APageObjectBase Overrides

        #region ICountable Implementation

        /// <summary>Number of parts the <see cref="Stamp" /> represents.</summary>
        public int Parts
        {
            get { return GetValue<int>(PartsProperty); }
            set { SetValue(PartsProperty, value); }
        }

        public static readonly PropertyData PartsProperty = RegisterProperty("Parts", typeof (int), 1);

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

        #endregion //ICountable Implementation
    }
}