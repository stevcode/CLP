using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Ink;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Model.CLPPageObjects;

namespace Classroom_Learning_Partner.Model
{
    [Serializable]
    public class CLPHistory
    {
        public CLPHistory()
        {
            
        }

        private MetaDataContainer _metaData = new MetaDataContainer();
        public MetaDataContainer MetaData
        {
            get
            {
                return _metaData;
            }
        }
        //ignore this dictionary for serialization
        private Dictionary<string, object> _objectReferences = new Dictionary<string, object>();
        public Dictionary<string, object> ObjectReferences
        {
            get
            {
                return _objectReferences;
            }
            set 
            {
                _objectReferences = value;
            }
        }
        //Run this method after serialization please! :-)
        public void TuplesToDict()
        {
            foreach (var tup in ObjTuples.Keys)
            {
                if (tup.Item1) //this is a CLPObjectBase
                {
                    ObjectReferences.Add(tup.Item2, ObjTuples[tup].Item1);
                }
                else
                {
                    ObjectReferences.Add(tup.Item2, ObjTuples[tup].Item2);
                }
            }
        }
        //Run this method before serialization thank you! :-)
   /*     public void DictToTuples()
        {
            foreach (var key in ObjectReferences.Keys)
            {
                if (ObjectReferences[key] is CLPPageObjectBase)
                {
                    CLPPageObjectBase obj = ObjectReferences[key] as CLPPageObjectBase;
                    if (obj is CLPAnimation)
                    {
                        CLPAnimation pageObj = obj as CLPAnimation;
                        ObjTuples.Add(Tuple.Create<bool, string>(true, key), Tuple.Create<CLPPageObjectBase, string>(pageObj, "null"));
                    }
                    else if (obj is CLPBlankStamp)
                    {
                        CLPBlankStamp pageObj = obj as CLPBlankStamp;
                        ObjTuples.Add(Tuple.Create<bool, string>(true, key), Tuple.Create<CLPPageObjectBase, string>(pageObj, "null"));
                    }
                    else if (obj is CLPCircle)
                    {
                        CLPCircle pageObj = obj as CLPCircle;
                        ObjTuples.Add(Tuple.Create<bool, string>(true, key), Tuple.Create<CLPPageObjectBase, string>(pageObj, "null"));
                    }
                    else if (obj is CLPImage)
                    {
                        CLPImage pageObj = obj as CLPImage;
                        ObjTuples.Add(Tuple.Create<bool, string>(true, key), Tuple.Create<CLPPageObjectBase, string>(pageObj, "null"));
                    }
                    else if (obj is CLPImageStamp)
                    {
                        CLPImageStamp pageObj = obj as CLPImageStamp;
                        ObjTuples.Add(Tuple.Create<bool, string>(true, key), Tuple.Create<CLPPageObjectBase, string>(pageObj, "null"));
                    }
                    else if (obj is CLPSnapTile)
                    {
                        CLPSnapTile pageObj = obj as CLPSnapTile;
                        ObjTuples.Add(Tuple.Create<bool, string>(true, key), Tuple.Create<CLPPageObjectBase, string>(pageObj, "null"));
                    }
                    else if (obj is CLPSquare)
                    {
                        CLPSquare pageObj = obj as CLPSquare;
                        ObjTuples.Add(Tuple.Create<bool, string>(true, key), Tuple.Create<CLPPageObjectBase, string>(pageObj, "null"));
                    }
                    else if (obj is CLPStampBase)
                    {
                        CLPStampBase pageObj = obj as CLPStampBase;
                        ObjTuples.Add(Tuple.Create<bool, string>(true, key), Tuple.Create<CLPPageObjectBase, string>(pageObj, "null"));
                    }
                    else if (obj is CLPTextBox)
                    {
                        CLPTextBox pageObj = obj as CLPTextBox;
                        ObjTuples.Add(Tuple.Create<bool, string>(true, key), Tuple.Create<CLPPageObjectBase, string>(pageObj, "null"));
                    }

                     }
                else
                {
                    ObjTuples.Add(Tuple.Create<bool, string>(false, key), Tuple.Create<CLPPageObjectBase, string>(null, ObjectReferences[key] as string));
              
                }
            }
        }
        */
        public void DictToTuples()
        {
            ObjTuples = new Dictionary<Tuple<bool, string>, Tuple<CLPPageObjectBase, string>>();
            foreach (var key in ObjectReferences.Keys)
                 {
               if (ObjectReferences[key] is CLPPageObjectBase)
               {
                   Console.WriteLine(ObjTuples.Count().ToString());
          ObjTuples.Add(Tuple.Create<bool, string>(true, key), Tuple.Create<CLPPageObjectBase, string>(ObjectReferences[key] as CLPPageObjectBase, "null"));
               }
               else
               {
                           ObjTuples.Add(Tuple.Create<bool, string>(false, key), Tuple.Create<CLPPageObjectBase, string>(null, ObjectReferences[key] as string));

               }
           }
       }
        //for serialiazation
        private Dictionary<Tuple<bool, String>,Tuple<CLPPageObjectBase, String>> _objTuples = new Dictionary<Tuple<bool,string>,Tuple<CLPPageObjectBase,string>>();
        public Dictionary<Tuple<bool, String>,Tuple<CLPPageObjectBase, String>> ObjTuples
        {
            get
            {
                return _objTuples;
            }
            set
            {
                _objTuples = value;
            }
        }
        private ObservableCollection<CLPHistoryItem> _historyItems = new ObservableCollection<CLPHistoryItem>();
        public ObservableCollection<CLPHistoryItem> HistoryItems
        {
            get
            {
                return _historyItems;
            }
            
        }

        //List to enable undo/redo functionality
        private ObservableCollection<CLPHistoryItem> _undoneHistoryItems = new ObservableCollection<CLPHistoryItem>();
        public ObservableCollection<CLPHistoryItem> UndoneHistoryItems
        {
            get
            {
                return _undoneHistoryItems;
            }
            
        }

        #region Public Methods

        public void AddHistoryItem(object obj, CLPHistoryItem historyItem)
        {
            string uniqueID = null;
            if (obj is CLPPageObjectBase)
            {
                uniqueID = (obj as CLPPageObjectBase).UniqueID;
            }
            else if (obj is Stroke)
            {
                uniqueID = (obj as Stroke).GetPropertyData(CLPPage.StrokeIDKey) as string;
            }
            else if (obj is String)
            {
                uniqueID = (CLPPageViewModel.StringToStroke(obj as string) as Stroke).GetPropertyData(CLPPage.StrokeIDKey) as string;
            }
            if (uniqueID != null && !ObjectReferences.ContainsKey(uniqueID))
            {
                AddObjectToReferences(uniqueID, obj);
            }

            historyItem.ObjectID = uniqueID;
            _historyItems.Add(historyItem);

            System.Console.WriteLine("AddHistoryItem: HistoryItems.Count: " + HistoryItems.Count());
            System.Console.WriteLine("ObjectRefIds: " + ObjectReferences.Count());
        }
        public void AddHistoryItem(CLPHistoryItem item)
        {
            _historyItems.Add(item);
        }
        public void AddUndoneHistoryItem(object obj, CLPHistoryItem historyItem)
        {
            string uniqueID = null;
            if (obj is CLPPageObjectBase)
            {
                uniqueID = (obj as CLPPageObjectBase).UniqueID;
            }
            else if (obj is Stroke)
            {
                uniqueID = (obj as Stroke).GetPropertyData(CLPPage.StrokeIDKey) as string;
            }
            else if (obj is String)
            {
                uniqueID = (CLPPageViewModel.StringToStroke(obj as string) as Stroke).GetPropertyData(CLPPage.StrokeIDKey) as string;
            }
            if (uniqueID != null && !ObjectReferences.ContainsKey(uniqueID))
            {
                AddObjectToReferences(uniqueID, obj);
            }

            historyItem.ObjectID = uniqueID;
            _undoneHistoryItems.Add(historyItem);
        }
        public void AddUndoneHistoryItem(CLPHistoryItem item)
        {
            _undoneHistoryItems.Add(item);
        }
        private void AddObjectToReferences(string key, object obj)
        {
            if (obj is Stroke)
            {
                ObjectReferences.Add(key, CLPPageViewModel.StrokeToString(obj as Stroke));
            }
            
            else if (obj is CLPPageObjectBase)
            {
                ObjectReferences.Add(key, obj);
            }
            else
            {
                Logger.Instance.WriteToLog("Unknown Object attempted to write to History");
            }
        }

        #endregion //Public Methods

      
    }
}
