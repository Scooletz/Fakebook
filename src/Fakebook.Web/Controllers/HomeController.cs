using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Fakebook.State.Contracts;
using Microsoft.AspNetCore.Mvc;
using Fakebook.Web.Models;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace Fakebook.Web.Controllers
{
    public class HomeController : Controller
    {
        static readonly Uri StateUri = new Uri("fabric:/Fakebook/Fakebook.State");

        public async Task<IActionResult> Index()
        {
            var proxies = Enumerable.Range(0, 10).Select(BuildProxy).ToArray();
            var results = await Task.WhenAll(proxies.Select(p => p.ListAllItems()));

            var allItems = results.SelectMany(items=>items);
            return View(allItems);
        }

        public async Task<IActionResult> Like()
        {
            ViewData["Message"] = "You just liked one random item";

            var rand = new Random();
            var index = rand.Next(0, 9);

            var proxy = BuildProxy(index);
            await proxy.Like($"{index}");

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? ControllerContext.HttpContext.TraceIdentifier });
        }

        static IState BuildProxy(int partitionKey)
        {
            var partition = new ServicePartitionKey(partitionKey);
            var service = ServiceProxy.Create<IState>(StateUri, partition);
            return service;
        }
    }
}
