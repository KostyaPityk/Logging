using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using LoggingSample_BLL.Helpers;
using LoggingSample_BLL.Models;
using LoggingSample_DAL.Context;

namespace LoggingSample_BLL.Services
{
    public class OrderService : IDisposable
    {
        private readonly AppDbContext _context = new AppDbContext();

        public Task<IEnumerable<OrderModel>> Get(int customerId)
        {
            if (customerId == 56)
            {
                throw new OrderServiceException("Wrong id has been requested",
                    OrderServiceException.ErrorType.WrongCustomerId);
            }

            return _context.Orders.Where(item => item.CustomerId == customerId).ToListAsync().ContinueWith(task =>
            {
                var orders = task.Result;

                return orders.Select(item => item.Map());
            });
        }
        
        public Task<OrderModel> Get(int customerId, int orderId)
        {
            if (customerId == 56)
            {
                throw new OrderServiceException("Wrong customer id has been requested",
                    OrderServiceException.ErrorType.WrongCustomerId);
            }

            if (orderId == 21)
            {
                throw new OrderServiceException("Wrong order id has been requeted",
                    OrderServiceException.ErrorType.WrongOrderId);
            }

            return _context.Orders.SingleOrDefaultAsync(item => item.Id == orderId && item.CustomerId == customerId).ContinueWith(task =>
            {
                var orders = task.Result;

                return orders?.Map();
            });
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }

    public class OrderServiceException : Exception
    {
        public enum ErrorType
        {
            WrongCustomerId,
            WrongOrderId
        }

        public ErrorType Type { get; set; }

        public OrderServiceException(string message, ErrorType errorType) : base(message)
        {
            Type = errorType;
        }
    }
}
