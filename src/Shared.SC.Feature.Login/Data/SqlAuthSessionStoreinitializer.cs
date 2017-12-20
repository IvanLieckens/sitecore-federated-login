using System.Data.Entity;

namespace Shared.SC.Feature.Login.Data
{
    public class SqlAuthSessionStoreInitializer : DropCreateDatabaseIfModelChanges<SqlAuthSessionStoreContext>
    {
    }
}