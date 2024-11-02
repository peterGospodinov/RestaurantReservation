using System.Collections.Generic;

namespace CommonServices.Infrastructure.Db
{
    public interface IDatabaseManager
    {
        void ExecuteStoredProcedure<TModel>(string storedProcedureName, TModel model) where TModel : class;
    }
}
