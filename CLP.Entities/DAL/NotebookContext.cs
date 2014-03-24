using System.Data.Entity;

namespace CLP.Entities
{
    public class NotebookContext : DbContext
    {
        //public NotebookContext()
        //{
        //    //    Database.SetInitializer<NotebookContext>(null);
        //}

        public DbSet<Notebook> Notebooks { get; set; }

        #region Overrides of DbContext

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notebook>().Ignore(t => t.IsDirty);
            base.OnModelCreating(modelBuilder);
        }

        #endregion
    }
}