using System.Data.Entity;

namespace CLP.Entities
{
    public class ClassPeriodContext : DbContext
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

        #endregion

        #region Methods

        /// <summary>
        /// Prevents the 3 ModelBase Catel Properties from appearing in the database. Must have an entry for every new EntityBase created.
        /// </summary>
        /// <param name="modelBuilder"><see cref="DbModelBuilder" /> from the <see cref="ClassPeriodContext" />.</param>
        private void CatelModelBaseSanitizer(DbModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<EntityBase>();

            modelBuilder.Entity<ClassSubject>().Ignore(t => t.IsDirty);
            modelBuilder.Entity<ClassSubject>().Ignore(t => t.IsReadOnly);
            modelBuilder.Entity<ClassSubject>().Ignore(t => t.Mode);

            modelBuilder.Entity<ClassPeriod>().Ignore(t => t.IsDirty);
            modelBuilder.Entity<ClassPeriod>().Ignore(t => t.IsReadOnly);
            modelBuilder.Entity<ClassPeriod>().Ignore(t => t.Mode);

            modelBuilder.Entity<Person>().Ignore(t => t.IsDirty);
            modelBuilder.Entity<Person>().Ignore(t => t.IsReadOnly);
            modelBuilder.Entity<Person>().Ignore(t => t.Mode);

            modelBuilder.Entity<Notebook>().Ignore(t => t.IsDirty);
            modelBuilder.Entity<Notebook>().Ignore(t => t.IsReadOnly);
            modelBuilder.Entity<Notebook>().Ignore(t => t.Mode);

            modelBuilder.Entity<CLPPage>().Ignore(t => t.IsDirty);
            modelBuilder.Entity<CLPPage>().Ignore(t => t.IsReadOnly);
            modelBuilder.Entity<CLPPage>().Ignore(t => t.Mode);
        }

        #endregion //Methods
    }
}