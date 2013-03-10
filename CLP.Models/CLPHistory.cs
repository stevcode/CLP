using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Catel.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Ink;

namespace CLP.Models
{
    [Serializable]
    public class CLPHistory
    {

        
        private readonly string historyId;

        private bool memEnabled;
        private string closed = null;
        private Stack<object> stack1 = new Stack<object>();
        private Stack<object> stack2 = new Stack<object>();



        public readonly string Page_Added = "Page_Added";
        public readonly string Page_Inserted = "Page_Inserted";
        public readonly string Page_Removed = "Page_Removed";
        public readonly string Stroke_Added = "Stroke_Added";
        public readonly string Stroke_Removed = "Stroke_Removed";
        public readonly string Object_Added = "Add"; //do not modify
        public readonly string Object_Removed = "Remove"; //do not modify

       

        public CLPHistory()
        {
            
            //historyId = clpPage.UniqueID;
            memEnabled = true;

        }

        public void enableMem()
        {
            memEnabled = true;

        }
        public void disableMem()
        {
            memEnabled = false;

        }

        public bool isMemEnabled()
        {
            return memEnabled;
        }
        public void close()
        {
            closed = "closed";
        }

        public Boolean push(Object o)
        {
            if(closed == null)
            {
                if(isMemEnabled())
                {
                    Console.WriteLine("memEnabled: " + isMemEnabled());
                    stack1.Push(o);
                    stack2.Clear();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

      

        public string getHistoryId()
        {
            return historyId;
        }

        private Stack<object> getStack1()
        {
            return stack1;
        }

        private Stack<object> getStack2()
        {
            return stack2;
        }

        private void setStack1(Stack<object> stack)
        {
            stack1 = stack;
        }
        private void setStack2(Stack<object> stack)
        {
            stack2 = stack;
        }
        private object popStack1()
        {
            return stack1.Pop();
        }
        private object popStack2()
        {
            return stack2.Pop();
        }

        private void pushStack1(object o)
        {
            stack1.Push(o);
        }

        private void pushStack2(object o)
        {
            stack2.Push(o);
        }

        private void ClearStack1()
        {
            stack1.Clear();
        }

        private void ClearStack2()
        {
            stack2.Clear();
        }
        /////////////////////////////////
        public void undo(){
            printMemStacks("undo", "before1");
            Stack<object> stack1 = getStack1();
            int stack1Count = stack1.Count;
            if(stack1Count==0){
                return;
            }
            Stack<object> stack2 = getStack2();
            int stack2Count = stack2.Count;

            printMemStacks("undo", "before2");
            Stack<object> stack3 = new Stack<object>(new Stack<object>(stack2));
            printMemStacks("undo", "before3");
            
            List<object> l = (List<object>)popStack1();
            disableMem();
            try{
                revertToMemList(l);
            }catch(Exception e){
                Console.WriteLine(e.StackTrace);
            }
            ///////////////
            //Integrity check
            int stack1CountAfter = getStack1().Count;
            if(stack1CountAfter != (stack1Count - 1))
            {
                popStack1();
                setStack2(stack3);
                ClearStack1();
                enableMem();
            }
            else {
                pushStack2(l);
                enableMem();
            }
            ////////////////
            printMemStacks("undo", "after"); 
        }

        public void redo(){
             printMemStacks("redo", "before1"); 
             
             Stack<object> stack2 = getStack2();
             int stack2Count = stack2.Count;
             if(stack2Count == 0)
             {
                return;
             }
             Stack<object> stack1 = getStack1();
             int stack1Count = stack1.Count;

             printMemStacks("redo", "before2");
             Stack<object> stack3 = new Stack<object>(new Stack<object>(stack2));
             printMemStacks("redo", "before3");

            List<object> l = (List<object>)popStack2();
            disableMem();
            try{
                forwardToMemList(l);
            }catch(Exception e){
                Console.WriteLine(e.StackTrace);
            }
            ///////////////
            //Integrity check
            int stack1CountAfter = getStack1().Count;
            if(stack1CountAfter != stack1Count)
            {
                popStack1();
                setStack2(stack3);
                ClearStack2();
                enableMem();
            }
            else
            {
                pushStack1(l);
                enableMem();
            }
            ////////////////
            printMemStacks("redo", "after");
        }

        public void printMemStacks(String methodName, String pos)
        {
            Console.WriteLine(pos + " " + methodName + " stack1");
            Stack<object> stack1 = getStack1();
            foreach(Object o in stack1)
            {
                Console.WriteLine(((List<object>)o)[0]);
            }
            Console.WriteLine(pos + " " + methodName + " stack2");
            Stack<object> stack2 = getStack2();
            foreach(Object o in stack2)
            {
                Console.WriteLine(((List<object>)o)[0]);
            }
        }

        private Boolean revertToMemList(List<object> l){
            string inst = (string)l[0];
           if(inst.Equals(Stroke_Added)){
                CLPPage page = (CLPPage)l[1];
                Stroke s = (Stroke)l[2];
                page.InkStrokes.Remove(s);
                return true;
            }else if(inst.Equals(Stroke_Removed)){
                CLPPage page = (CLPPage)l[1];
                Stroke s = (Stroke)l[2];
                page.InkStrokes.Add(s);
                return true;
            }else if(inst.Equals(Object_Added)){
                CLPPage page = (CLPPage)l[1];
                IList addedPageObjects = (IList)l[2];
                foreach(ICLPPageObject addedPageObject in addedPageObjects)
                {
                    page.PageObjects.Remove(addedPageObject);
                }
                return true;
            }else if(inst.Equals(Object_Removed)){
                CLPPage page = (CLPPage)l[1];
                IList removedPageObjects = (IList)l[2];
                foreach(ICLPPageObject removedPageObject in removedPageObjects)
                {
                    page.PageObjects.Add(removedPageObject);
                }
                return true;
            }else{
                return false;
            }
        }

        private Boolean forwardToMemList(List<object> l){
            string inst = (string)l[0];
            if(inst.Equals(Stroke_Added)){
                CLPPage page = (CLPPage)l[1];
                Stroke s = (Stroke)l[2];
                page.InkStrokes.Add(s);
                return true;
            }else if(inst.Equals(Stroke_Removed)){
                CLPPage page = (CLPPage)l[1];
                Stroke s = (Stroke)l[2];
                page.InkStrokes.Remove(s);
                return true;
            }
            else if(inst.Equals(Object_Added))
            {
                CLPPage page = (CLPPage)l[1];
                IList addedPageObjects = (IList)l[2];
                foreach(ICLPPageObject addedPageObject in addedPageObjects)
                {
                    page.PageObjects.Add(addedPageObject);
                }
                return true;
            }
            else if(inst.Equals(Object_Removed))
            {
                CLPPage page = (CLPPage)l[1];
                IList removedPageObjects = (IList)l[2];
                foreach(ICLPPageObject removedPageObject in removedPageObjects)
                {
                    page.PageObjects.Remove(removedPageObject);
                }
                return true;
            }else{
                return false;
            } 
        }

        #region Previous History

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


        #endregion 
    }
}