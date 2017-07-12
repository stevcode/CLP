using System;
using Catel;

namespace CLP.Entities
{
    public static class NotebookExtensions
    {
        #region Cache

        public static Notebook CopyNotebookForNewOwner(this Notebook notebook, Person newOwner)
        {
            Argument.IsNotNull("notebook", notebook);
            Argument.IsNotNull("newOwner", newOwner);

            var newNotebook = notebook.DeepCopy();
            newNotebook.Owner = newOwner;
            newNotebook.GenerationDate = DateTime.Now;
            newNotebook.LastSavedDate = DateTime.Now;

            return newNotebook;
        }

        #endregion // Cache
    }
}
