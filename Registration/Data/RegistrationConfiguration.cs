using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace Registration.Data
{
    public class RegistrationConfiguration : DbConfiguration
    {
        public RegistrationConfiguration()
        {
            SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy());
        }
    }
}