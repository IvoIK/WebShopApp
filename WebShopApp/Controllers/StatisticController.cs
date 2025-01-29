using Microsoft.AspNetCore.Mvc;

using WebShopApp.Core.Contracts;
using WebShopApp.Models.Statistic;

namespace WebShopApp.Controllers
{
    public class StatisticController : Controller
    {
        private readonly IStatisticService _statisticsService;

        public StatisticController(IStatisticService statisticsService)
        {
            this._statisticsService = statisticsService;
        }

        public IActionResult Index()
        {
            StatisticVM statistics = new StatisticVM();

            statistics.CountClients = _statisticsService.CountClients();
            statistics.CountProducts = _statisticsService.CountProducts();
            statistics.CountOrders = _statisticsService.CountOrders();
            statistics.SumOrders = _statisticsService.SumOrders();

            return View(statistics);
        }
    }
}