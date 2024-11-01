using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommonServices.Infrastructure.Logging
{
    public interface IFileLogger
    {
        Task LogAsync(IEnumerable<string> messages);
    }
}
