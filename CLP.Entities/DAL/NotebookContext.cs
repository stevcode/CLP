using System.Data.Entity;

namespace CLP.Entities
{
    public class NotebookContext : DbContext
    {
        public DbSet<Notebook> Notebooks { get; set; }
        public DbSet<CLPPage> CLPPages { get; set; }

        #region Overrides of DbContext

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Exclude Catel's ModelBase Properties from Database
            CatelModelBaseSanitizer(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Prevents the 3 ModelBase Catel Properties from appearing in the database. Must have an entry for every new EntityBase created.
        /// </summary>
        /// <param name="modelBuilder"><see cref="DbModelBuilder" /> from the <see cref="NotebookContext" />.</param>
        private void CatelModelBaseSanitizer(DbModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<EntityBase>();

            modelBuilder.Entity<Notebook>().Ignore(t => t.IsDirty);
            modelBuilder.Entity<Notebook>().Ignore(t => t.IsReadOnly);
            modelBuilder.Entity<Notebook>().Ignore(t => t.Mode);

            modelBuilder.Entity<CLPPage>().Ignore(t => t.IsDirty);
            modelBuilder.Entity<CLPPage>().Ignore(t => t.IsReadOnly);
            modelBuilder.Entity<CLPPage>().Ignore(t => t.Mode);
        }

        #endregion
    }
}