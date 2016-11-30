using System.Data.Entity;

namespace CLP.Entities.Ann
{
    public class ClassPeriodContext : DbContext
    {
        public DbSet<ClassSubject> ClassSubjects { get; set; }

        public DbSet<ClassPeriod> ClassPeriods { get; set; }
        public DbSet<Notebook> Notebooks { get; set; }
        public DbSet<CLPPage> Pages { get; set; }

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
        /// Prevents the 3 ModelBase Catel Properties from appearing in the database. Must have an entry for every new AEntityBase created.
        /// </summary>
        /// <param name="modelBuilder"><see cref="DbModelBuilder" /> from the <see cref="ClassPeriodContext" />.</param>
        private void CatelModelBaseSanitizer(DbModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<AEntityBase>();
            modelBuilder.Ignore<APageObjectBase>();
            modelBuilder.Ignore<ATagBase>();

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

            //PageObjects
            modelBuilder.Entity<Shape>().Ignore(t => t.IsDirty);
            modelBuilder.Entity<Shape>().Ignore(t => t.IsReadOnly);
            modelBuilder.Entity<Shape>().Ignore(t => t.Mode);

            modelBuilder.Entity<CLPTextBox>().Ignore(t => t.IsDirty);
            modelBuilder.Entity<CLPTextBox>().Ignore(t => t.IsReadOnly);
            modelBuilder.Entity<CLPTextBox>().Ignore(t => t.Mode);

            //Tags
            modelBuilder.Entity<CorrectnessTag>().Ignore(t => t.IsDirty);
            modelBuilder.Entity<CorrectnessTag>().Ignore(t => t.IsReadOnly);
            modelBuilder.Entity<CorrectnessTag>().Ignore(t => t.Mode);

            modelBuilder.Entity<PageTopicTag>().Ignore(t => t.IsDirty);
            modelBuilder.Entity<PageTopicTag>().Ignore(t => t.IsReadOnly);
            modelBuilder.Entity<PageTopicTag>().Ignore(t => t.Mode);

            modelBuilder.Entity<StarredTag>().Ignore(t => t.IsDirty);
            modelBuilder.Entity<StarredTag>().Ignore(t => t.IsReadOnly);
            modelBuilder.Entity<StarredTag>().Ignore(t => t.Mode);
        }

        #endregion //Methods
    }
}