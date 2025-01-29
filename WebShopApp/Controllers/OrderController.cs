using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

using WebShopApp.Core.Contracts;
using WebShopApp.Infrastructure.Data.Domain;
using WebShopApp.Models.Order;

namespace WebShopApp.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        public OrderController(IProductService productService, IOrderService orderService)
        {
            _productService = productService;
            _orderService = orderService;
        }

        // GET: OrderController/Create
        public ActionResult Create(int id)
        {
            Product product = _productService.GetProductById(id);

            if (product == null)
            {
                return NotFound();
            }

            OrderCreateVM order = new OrderCreateVM()
            {
                ProductId = product.Id,
                ProductName = product.ProductName,
                QuantityInStock = product.Quantity,
                Price = product.Price,
                Discount = product.Discount,
                Picture = product.Picture
            };

            return View(order);
        }

        // POST: OrderController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(OrderCreateVM bindingModel) 
        {
            string currentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var product = this._productService.GetProductById(bindingModel.ProductId);

            if (currentUserId == null || product == null || product.Quantity < bindingModel.Quantity || product.Quantity == 0)
            {
                return RedirectToAction("Denied", "Order");
            }

            if (ModelState.IsValid)
            {
                _orderService.Create(bindingModel.ProductId, currentUserId, bindingModel.Quantity);
            }

            return this.RedirectToAction("Index", "Product");
        }

        // GET: OrderController
        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            // string userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            // var user = context.Users.SingleOrDefault(x => x.Id == userId);

            List<OrderIndexVM> orders = _orderService.GetOrders().Select(x => new OrderIndexVM
            {
                Id = x.Id,
                OrderDate = x.OrderDate.ToString("dd-MMM-yyyy hh:mm", CultureInfo.InvariantCulture),
                UserId = x.UserId,
                User = x.User.UserName,
                ProductId = x.ProductId,
                Product = x.Product.ProductName,
                Picture = x.Product.Picture,
                Quantity = x.Quantity,
                Price = x.Price,
                Discount = x.Discount,
                TotalPrice = x.TotalPrice,
            }).ToList();

            return View(orders);
        }

        // GET: OrderController/Edit/5
        public ActionResult Edit(int id)
        {
            var order = _orderService.GetOrderById(id);

            if (order == null)
            {
                return NotFound();
            }

            OrderEditVM updatedOrder = new OrderEditVM
            {
                Id = order.Id,
                ProductId = order.ProductId,
                ProductName = order.Product.ProductName,
                QuantityInStock = order.Product.Quantity,
                Quantity = order.Quantity,
                Price = order.Price,
                Discount = order.Discount,
                Picture = order.Product.Picture,
                TotalPrice = order.TotalPrice,
            };

            return View(updatedOrder);
        }

        // POST: OrderController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, OrderEditVM order)
        {
            if (ModelState.IsValid)
            {
                string currentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

                var updated = _orderService.Update(id, order.ProductId, currentUserId, order.Quantity);

                if (updated)
                {
                    return this.RedirectToAction("MyOrders");
                }
            }

            return View(order);
        }


        // GET: OrderController/Delete/5
        public ActionResult Delete(int id)
        {
            var order = _orderService.GetOrderById(id);

            if (order == null)
            {
                return NotFound();
            }

            OrderDeleteVM deletedOrder = new OrderDeleteVM
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                ProductId = order.ProductId,
                ProductName = order.Product.ProductName,
                Quantity = order.Quantity,
                Price = order.Price,
                Discount = order.Discount,
                Picture = order.Product.Picture,
                TotalPrice = order.TotalPrice,
            };

            return View(deletedOrder);
        }

        // POST: OrderController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            var deleted = _orderService.RemoveById(id);

            if (deleted)
            {
                return this.RedirectToAction("Success");
            }
            else
            {
                return View();
            }
        }

        // GET: OrderController/MyOrders
        public ActionResult MyOrders()
        {
            string currentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            // var user = context.Users.SingleOrDefault(x => x.Id == user.Id");

            List<OrderIndexVM> orders = _orderService.GetOrdersByUser(currentUserId).Select(x => new OrderIndexVM
            {
                Id = x.Id,
                OrderDate = x.OrderDate.ToString("dd-MMM-yyyy hh:mm", CultureInfo.InvariantCulture),
                UserId = x.UserId,
                User = x.User.UserName,
                ProductId = x.ProductId,
                Product = x.Product.ProductName,
                Picture = x.Product.Picture,
                Quantity = x.Quantity,
                Price = x.Price,
                Discount = x.Discount,
                TotalPrice = x.TotalPrice,
            }).ToList();

            return View(orders);
        }

        // GET: OrderController/Denied
        public ActionResult Denied()
        {
            return View();
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}