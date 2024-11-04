using System.Threading.Tasks;

namespace CommonServices.Infrastructure.Db
{
    public interface IDatabaseManager
    {
        Task ExecuteStoredProcedureAsync<TModel>(string storedProcedureName, TModel model) where TModel : class;
    }
}
