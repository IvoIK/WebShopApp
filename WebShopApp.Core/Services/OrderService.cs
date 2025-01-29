using Microsoft.IdentityModel.Protocols.OpenIdConnect;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebShopApp.Core.Contracts;
using WebShopApp.Infrastructure.Data;
using WebShopApp.Infrastructure.Data.Domain;

namespace WebShopApp.Core.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;

        public OrderService(ApplicationDbContext context, IProductService productService)
        {
            _context = context;
            _productService = productService;
        }

        public bool Create(int productId, string userId, int quantity)
        {
            var product = this._context.Products.SingleOrDefault(x => x.Id == productId);

            if (product == null) 
            {
                return false;
            }

            Order item = new Order
            {
                OrderDate = DateTime.Now,
                ProductId = productId,
                UserId = userId,
                Quantity = quantity,
                Price = product.Price,
                Discount = product.Discount
            };

            product.Quantity -= quantity;

            this._context.Products.Update(product);
            this._context.Orders.Add(item);

            return _context.SaveChanges() != 0;
        }

        public Order GetOrderById(int orderId)
        {
            return _context.Orders.Find(orderId);
        }

        public List<Order> GetOrders()
        {
            return _context.Orders.OrderByDescending(x => x.OrderDate).ToList();
        }

        public List<Order> GetOrdersByUser(string userId)
        {
            return _context.Orders.Where(x => x.UserId == userId).OrderByDescending(x => x.OrderDate).ToList();
        }

        public bool RemoveById(int orderId)
        {
            var order = GetOrderById(orderId);

            if (order == null)
            { 
                return false; 
            }

            var product = this._context.Products.SingleOrDefault(x => x.Id == order.ProductId);

            if (product != null)
            {
                product.Quantity += order.Quantity;
                _context.Products.Update(product);
            }

            _context.Orders.Remove(order);
            return _context.SaveChanges() != 0;
        }

        public bool Update(int orderId, int productId, string userId, int quantity)
        {
            var order = GetOrderById(orderId);

            if (order == null)
            {
                return false;
            }

            var updatedProduct = this._context.Products.SingleOrDefault(x => x.Id == productId);

            if (updatedProduct == null)
            {
                return false;
            }

            var oldProduct = this._context.Products.SingleOrDefault(x => x.Id == order.ProductId);

            if (oldProduct != null && oldProduct.Id != updatedProduct.Id)
            {
                oldProduct.Quantity += order.Quantity;
                _context.Products.Update(oldProduct);
            }

            if (updatedProduct.Quantity + order.Quantity < quantity)
            {
                return false; 
            }

            updatedProduct.Quantity = updatedProduct.Quantity + order.Quantity - quantity;

            order.OrderDate = DateTime.Now;
            order.ProductId = productId;
            order.UserId = userId;
            order.Quantity = quantity;
            order.Price = updatedProduct.Price;
            order.Discount = updatedProduct.Discount;

            _context.Products.Update(updatedProduct);
            _context.Orders.Update(order);
            return _context.SaveChanges() != 0;
        }
    }
}