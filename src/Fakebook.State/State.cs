using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Fakebook.State.Contracts;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
namespace Fakebook.State
{
    sealed class State : StatefulService, IState
    {
        public State(StatefulServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener(this.CreateServiceRemotingListener)
            };
        }

        public async Task<Dictionary<string, int>> ListAllItems()
        {
            var items = await StateManager.GetOrAddAsync<IReliableDictionary<string, int>>("items");

            var result = new Dictionary<string, int>();
            using (var tx = StateManager.CreateTransaction())
            {
                var enumerable = await items.CreateEnumerableAsync(tx, EnumerationMode.Unordered);
                using (var enumerator = enumerable.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(CancellationToken.None))
                    {
                        var kvp = enumerator.Current;
                        result[kvp.Key] = kvp.Value;
                    }
                }
            }

            return result;
        }

        public async Task Like(string item)
        {
            var items = await StateManager.GetOrAddAsync<IReliableDictionary<string, int>>("items");
            
            using (var tx = StateManager.CreateTransaction())
            {
                await items.AddOrUpdateAsync(tx, item, (_) => 1, (_, count) => count + 1);
                await tx.CommitAsync();
            }
        }
    }
}
