using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Catel.Data;
using System.IO;
//using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Ink;

namespace CLP.Models
{
    /// <summary>
    /// CLPNotebook Data object class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// </summary>
    [Serializable]
    public class CLPNotebook : SavableDataObjectBase<CLPNotebook>
    {
        public readonly Memento memento;
        private bool memEnabled;
        
        //private readonly Memento initialMemento;
        #region Constructor

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPNotebook()
        {
            memento = new Memento(this);
            memento.disableMem();
            CreationDate = DateTime.Now;
            UniqueID = Guid.NewGuid().ToString();
            Pages = new ObservableCollection<CLPPage>();
            Submissions = new Dictionary<string, ObservableCollection<CLPPage>>();
            AddPage(new CLPPage());
            memento.enableMem();
            //initialMemento = this.getMemento();
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPNotebook(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Properties

        public String UserName
        {
            get { return GetValue<string>(UserNameProperty); }
            set { SetValue(UserNameProperty, value); }
        }

       




        public static readonly PropertyData UserNameProperty = RegisterProperty("UserName", typeof(String), "NoName");

        /// <summary>
        /// Gets the list of CLPPages in the notebook.
        /// </summary>
        public ObservableCollection<CLPPage> Pages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(PagesProperty); }
            private set { SetValue(PagesProperty, value); }
        }

        /// <summary>
        /// Register the Pages property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof(ObservableCollection<CLPPage>), () => new ObservableCollection<CLPPage>());

        /// <summary>
        /// Gets a dictionary that maps the UniqueID of a page to a list of the submissions for that page.
        /// </summary>
        public Dictionary<string, ObservableCollection<CLPPage>> Submissions
        {
            get { return GetValue<Dictionary<string, ObservableCollection<CLPPage>>>(SubmissionsProperty); }
            private set { SetValue(SubmissionsProperty, value); }
        }

        /// <summary>
        /// Register the Submissions property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SubmissionsProperty = RegisterProperty("Submissions", typeof(Dictionary<string, ObservableCollection<CLPPage>>), () => new Dictionary<string, ObservableCollection<CLPPage>>());

        /// <summary>
        /// Name of notebook.
        /// </summary>
        public string NotebookName
        {
            get { return GetValue<string>(NotebookNameProperty); }
            set { SetValue(NotebookNameProperty, value); }
        }

        /// <summary>
        /// Register the NotebookName property so it is known in the class.
        /// </summary>
        public static readonly PropertyData NotebookNameProperty = RegisterProperty("NotebookName", typeof(string), null);

        /// <summary>
        /// UniqueID assigned to the notebook.
        /// </summary>
        public string UniqueID
        {
            get { return GetValue<string>(UniqueIDProperty); }
            private set { SetValue(UniqueIDProperty, value); }
        }

        /// <summary>
        /// Register the UniqueID property so it is known in the class.
        /// </summary>
        public static readonly PropertyData UniqueIDProperty = RegisterProperty("UniqueID", typeof(string), Guid.NewGuid().ToString());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        /// <summary>
        /// Register the CreationDate property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime), null);

        #endregion

        #region Methods

        public void GeneratePageIndexes()
        {
            int pageIndex = 1;
            foreach(var page in Pages)
            {
                page.PageIndex = pageIndex;
                pageIndex++;
            }
        }

        public void AddPage(CLPPage page)
        {
            page.ParentNotebookID = UniqueID;
            Pages.Add(page);
            GenerateSubmissionViews(page.UniqueID);
            GeneratePageIndexes();
            List<object> l = new List<object>();
            l.Add(memento.Page_Added);
            l.Add(page);
            this.memento.push(l);  
        }

        public void InsertPageAt(int index, CLPPage page)
        {
            Pages.Insert(index, page);
            GenerateSubmissionViews(page.UniqueID);
            GeneratePageIndexes();
            List<object> l = new List<object>();
            l.Add(memento.Page_Inserted); 
            l.Add(page);
            this.memento.push(l);  
        }

        private void GenerateSubmissionViews(string pageUniqueID)
        {
            if(!Submissions.ContainsKey(pageUniqueID))
            {
                Submissions.Add(pageUniqueID, new ObservableCollection<CLPPage>());
            }
        }

        public void RemovePageAt(int index)
        {
            CLPPage sPage = null;
            try{
                sPage = Pages[index]; 
            }catch(Exception e){
                Console.WriteLine(e.StackTrace);
            }

            if(Pages.Count > index && index >= 0)
            {
                Submissions.Remove(Pages[index].UniqueID);
                Pages.RemoveAt(index);
            }
            if(Pages.Count == 0)
            {
                AddPage(new CLPPage());
            }
            GeneratePageIndexes();
            List<object> l = new List<object>();
            l.Add(memento.Page_Removed);
            l.Add(sPage);
            this.memento.push(l); 
        }

        public CLPPage GetPageAt(int pageIndex, int submissionIndex)
        {
            if(submissionIndex < -1) return null;
            if(submissionIndex == -1)
            {
                try
                { return Pages[pageIndex]; }
                catch(Exception e)
                {
                    return null;
                }
            }

            try
            {
                return Submissions[Pages[pageIndex].UniqueID][submissionIndex];
            }
            catch(Exception e)
            {
                return null;
            }
        }

        public CLPPage GetNotebookPageByID(string pageUniqueID)
        {
            foreach(var page in Pages)
            {
                if(page.UniqueID == pageUniqueID)
                {
                    return page;
                }
            }

            return null;
        }

        public int GetNotebookPageIndex(CLPPage page)
        {
            if(page.IsSubmission)
            {
                return -1;
            }
            else
            {
                return Pages.IndexOf(page);
            }
        }

        public CLPPage GetSubmissionByID(string pageID)
        {
            CLPPage returnPage = null;
            foreach(var pageKey in Submissions.Keys)
            {
                foreach(var page in Submissions[pageKey])
                {
                    if(page.SubmissionID == pageID)
                    {
                        returnPage = page;
                        break;
                    }
                }
                if(returnPage != null)
                {
                    break;
                }
            }

            return returnPage;
        }

        public int GetSubmissionIndex(CLPPage page)
        {
            if(page.IsSubmission)
            {
                int submissionIndex = -1;
                foreach(string uniqueID in Submissions.Keys)
                {
                    foreach(CLPPage submission in Submissions[uniqueID])
                    {
                        if(submission.SubmissionID == page.SubmissionID)
                        {
                            submissionIndex = Submissions[uniqueID].IndexOf(submission);
                            break;
                        }
                    }
                }

                return submissionIndex;
            }
            else
            {
                return -1;
            }
        }

        public void AddStudentSubmission(string pageID, CLPPage submission)
        {
            CLPPage notebookPage = GetNotebookPageByID(pageID);
            if(Submissions.ContainsKey(pageID))
            {
                int count = 0;
                foreach(var page in Submissions[pageID])
                {
                    if(submission.SubmitterName == page.SubmitterName)
                    {
                        count++;
                        break;
                    }
                }
                if(count == 0)
                {
                    notebookPage.NumberOfSubmissions++;
                }
                Submissions[pageID].Add(submission);
            }
            else
            {
                ObservableCollection<CLPPage> pages = new ObservableCollection<CLPPage>();
                pages.Add(submission);
                Submissions.Add(pageID, pages);
                notebookPage.NumberOfSubmissions++;
            }
        }

        #endregion

        #region History

        /*public Boolean replay(Memento memInit, Memento memCurrent){
            Memento memFinal = memCurrent.getClone();
            revertToMem(memInit, memCurrent);
            forwardToMem(memFinal, memInit);
            return true;
        }
        
        public Boolean replayNotebook(){
            try{
                Memento memInit = this.getInitMem();
                Memento memCurrent = this.getMemento();
                return replay(memInit, memCurrent);
            }
            catch(Exception e){
                Console.WriteLine(e.StackTrace);
                return false;
            }
        }*/

        public void undo(){
            printMemStacks("undo", "before"); 
            Stack<object> Stack1 = memento.getStack1();
            if(Stack1.Count == 0){
                return;
            }
            Stack<object> Stack2 = memento.getStack2();
            List<object> l = (List<object>)Stack1.Pop();
            try{
                memento.disableMem();
                revertToMemList(l);
                memento.enableMem();
           }catch(Exception e){
                        Console.WriteLine(e.StackTrace);
                        return;
           }
            Stack2.Push(l);
            printMemStacks("undo", "after"); 
        }

        public void printMemStacks(String methodName, String pos) {
            Console.WriteLine(pos + " " + methodName + " stack1");
            Stack<object> stack1 = memento.getStack1();
            foreach(Object o in stack1)
            {
                Console.WriteLine(((List<object>)o)[0]);
            }
            Console.WriteLine(pos + " " + methodName + " stack2");
            Stack<object> stack2 = memento.getStack2();
            foreach(Object o in stack2)
            {
                Console.WriteLine(((List<object>)o)[0]);
            }
        }


        public void redo(){
             printMemStacks("redo", "before"); 
             Stack<object> stack2 = memento.getStack2();
             if(stack2.Count==0){
              return;
             }
            List<object> l = (List<object>)stack2.Pop();
            try{
                memento.disableMem();
                forwardToMemList(l);
                memento.enableMem();
            }catch(Exception e){
                Console.WriteLine(e.StackTrace);
                return;
            }
            this.memento.getStack1().Push(l);
            printMemStacks("redo", "after"); 
        }

        private Boolean revertToMemList(List<object> l){
            string inst = (string)l[0];
            if(inst.Equals(memento.Page_Added)){
                CLPPage page = (CLPPage)l[1];
                RemovePageAt(page.PageIndex-1);
                return true;
            }else if(inst.Equals(memento.Page_Inserted)){
                CLPPage page = (CLPPage)l[1];
                RemovePageAt(page.PageIndex-1);
                return true;
            }else if(inst.Equals(memento.Page_Removed)){
                CLPPage page = (CLPPage)l[1];
                InsertPageAt(page.PageIndex-1,page);
                return true;
            }else if(inst.Equals(memento.Stroke_Added)){
                CLPPage page = (CLPPage)l[1];
                Stroke s = (Stroke)l[2];
                page.InkStrokes.Remove(s);
                return true;
            }else if(inst.Equals(memento.Stroke_Removed)){
                CLPPage page = (CLPPage)l[1];
                Stroke s = (Stroke)l[2];
                page.InkStrokes.Add(s);
                return true;
            }else{
                return false;
            }
        }

        private Boolean forwardToMemList(List<object> l){
            string inst = (string)l[0];
            if(inst.Equals(memento.Page_Added)){
                CLPPage page = (CLPPage)l[1];
                AddPage(page);
                return true;
            }else if(inst.Equals(memento.Page_Inserted)){
                CLPPage page = (CLPPage)l[1];
                InsertPageAt(page.PageIndex-1, page);
                return true;
            }else if(inst.Equals(memento.Page_Removed)){
                CLPPage page = (CLPPage)l[1];
                RemovePageAt(page.PageIndex-1);
                return true;
            }else if(inst.Equals(memento.Stroke_Added)){
                CLPPage page = (CLPPage)l[1];
                Stroke s = (Stroke)l[2];
                page.InkStrokes.Add(s);
                return true;
            }else if(inst.Equals(memento.Stroke_Removed)){
                CLPPage page = (CLPPage)l[1];
                Stroke s = (Stroke)l[2];
                page.InkStrokes.Remove(s);
                return true;
            }else{
                return false;
            } 
        }

      /*  private Boolean revertToMem(Memento oldMem){
            Memento currentMem = this.getMemento();
            return revertToMem(oldMem, currentMem);
        }

        public Boolean revertToMem(Memento oldMem, Memento currentMem) { 
            String oldMemNotebookId = oldMem.getNotebookID();
            String currentMemNotebookId = currentMem.getNotebookID();
            if(oldMemNotebookId.Equals(currentMemNotebookId)){
                Stack<object> sOldMem = oldMem.getStack();
                Stack<object> sCurrentMem = currentMem.getStack();
                if(sOldMem.Count<sCurrentMem.Count){
                    try{
                        while(sOldMem.Count < sCurrentMem.Count){
                            List<object> l = (List<object>)sCurrentMem.Pop();
                            Console.WriteLine("This is the action being UNDONE: " + l[0]);
                            revertToMemList(l);
                        }
                        return true;
                    }catch(Exception e){
                        Console.WriteLine(e.StackTrace);
                        return false;
                    }
                }
            } 
            return false;
        }

        public Boolean forwardToMem(Memento futureMem, Memento currentMem) {
            Stack<object> sFutureMem = futureMem.getStack();
            int futureMemCount = sFutureMem.Count;
            Stack<object> sCurrentMem = currentMem.getStack();
            int currentMemCount = sCurrentMem.Count;
            if(futureMemCount <= currentMemCount) {
                return false;
            }
            ArrayList aFutureMem = new ArrayList();
            while(sFutureMem.Count > currentMemCount){
                aFutureMem.Insert(0, sFutureMem.Pop());
            }
            try{
                for(int i = 0; i < aFutureMem.Count; i++){
                    List<object> l = (List<object>)aFutureMem[i];
                    Console.WriteLine("This is the action being redone: " +l[0]);
                    forwardToMemList(l);
                }
                return true;
            }catch(Exception e){
                Console.WriteLine(e.StackTrace);
                return false;
            }
        }

        public Memento getInitMem() {
            Memento m = this.initialMemento.getClone();
            return m;
        }

        public Memento getMemento() {
            Memento m = this.memento.getClone();
            return m;

        }*/
       
       

        [Serializable]
        public class Memento{
            
            private readonly CLPNotebook notebook;
            private readonly string memId;

            private bool memEnabledF;
            private string closed = null;
            private Stack<object> stack1 = new Stack<object>();
            private Stack<object> stack2 = new Stack<object>();
            
            
            public readonly string Page_Added = "Page_Added";
            public readonly string Page_Inserted = "Page_Inserted";
            public readonly string Page_Removed = "Page_Removed";
            public readonly string Stroke_Added = "Stroke_Added";
            public readonly string Stroke_Removed = "Stroke_Removed";
            
            public Memento(CLPNotebook clpnb){
                notebook = clpnb;
                memId = notebook.UniqueID;
                memEnabledF = true;

            }
           
            public void enableMem() {
                memEnabledF= true;
                
            }
            public void disableMem() {
                memEnabledF = false;
                
            }

            public bool isMemEnabled() {
                return memEnabledF;
            }
            public void close() {
                closed = "closed";
            }
            
            public Boolean push(Object o){
                if(closed==null){
                    if(isMemEnabled()) {
                        Console.WriteLine("memEnabled: " + isMemEnabled());
                        stack1.Push(o);
                        stack2.Clear();
                        return true;
                    }else{
                        return false;
                    }
                }else{
                    return false;
                }
            }

            /*
               public Object pop(){
                   if(closed == null){
                       if(notebook.isMemEnabled() && isMemEnabledInternal())
                       {
                           if(stack1.Count!=0){
                               Object o = stack1.Pop();
                               stack2.Push(o);
                               return o;
                           }else{
                               return null;
                               }
                       }else{
                           return null;
                       }
                   }else{
                       return null;
                   }
               }
            
               public static T Clone<T>(T source)
               {
                  if(!typeof(T).IsSerializable)
                 {
                   Console.WriteLine("object is not serializable");
                       throw new ArgumentException("The type must be serializable.", "source");
                 }
       
                 // Don't serialize a null object, simply return the default for that object
                  if(Object.ReferenceEquals(source, null))
                   {
                       Console.WriteLine("Don't serialize a null object, simply return the default for that object");
                      return default(T);
                  }

                   IFormatter formatter = new BinaryFormatter();
                   Stream stream = new MemoryStream();
                   using(stream)
                   {
                       formatter.Serialize(stream, source);
                       stream.Seek(0, SeekOrigin.Begin);
                       return (T)formatter.Deserialize(stream);
                   }
               }

               public Memento getClone(){
                   Memento m = Clone(this);
                   m.close();
                   return m;
               }
            
               public Stack<object> getStack() {
                 return Clone(this.stack1);
              }*/
            
            public string getNotebookID() {
                return memId;
            }

            public Stack<object> getStack1() {
                return stack1;
            }

            public Stack<object> getStack2(){
            return stack2;
            }
              
        }
        #endregion 
    }
}
