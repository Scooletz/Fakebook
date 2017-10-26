using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Fakebook.State.Contracts
{
    public interface IState : IService
    {
        Task<Dictionary<string, int>> ListAllItems();
        Task Like(string item);
    }
}
