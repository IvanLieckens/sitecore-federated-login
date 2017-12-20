using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

using Shared.SC.Feature.Login.Models;

namespace Shared.SC.Feature.Login.Data
{
    public class SqlAuthSessionStoreContext : DbContext
    {
        public SqlAuthSessionStoreContext()
            : this("AuthSessionStoreContext")
        {
        }

        public SqlAuthSessionStoreContext(string init)
            : base(init)
        {
            Database.SetInitializer(new SqlAuthSessionStoreInitializer());
        }

        public DbSet<AuthSessionEntry> Entries { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}