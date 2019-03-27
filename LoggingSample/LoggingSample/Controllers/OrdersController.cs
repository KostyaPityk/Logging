using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Routing;
using LoggingSample_BLL.Helpers;
using LoggingSample_BLL.Models;
using LoggingSample_BLL.Services;
using LoggingSample_DAL.Context;
using NLog;

namespace LoggingSample.Controllers
{
    [RoutePrefix("api")]
    public class OrdersController : ApiController
    {
        private readonly AppDbContext _context = new AppDbContext();
        private readonly OrderService _orderService = new OrderService();
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        [Route("customers/{customerId}/orders", Name = "Orders")]
        public async Task<IHttpActionResult> Get(int customerId)
        {
            Logger.Info($"Start getting orders with customer with id {customerId}.");

            try
            {
                var orders = await _orderService.Get(customerId);

                Logger.Info($"Retrieving  order witth customer with id {customerId} to response.");
                return Ok(orders.Select(InitOrder));
            }
            catch(OrderServiceException ex)
            {
                if (ex.Type == OrderServiceException.ErrorType.WrongCustomerId)
                {
                    Logger.Warn($"Wrong customerId has been request in an effort getting order with customer id: {customerId}", ex);
                    return BadRequest($"Wrong customerId has been request in an effort getting order with customer id: {customerId}");
                }
                throw;
            }
            catch(Exception ex)
            {
                Logger.Error(ex, $"Some error occured while getting orders with customerId {customerId}");
                throw;
            }
        }

        [Route("customers/{customerId}/orders/{orderId}", Name = "Order")]
        public async Task<IHttpActionResult> Get(int customerId, int orderId)
        {
            Logger.Info($"Start getting order with id {orderId} and cusotomer id {customerId}.");

            try
            {
                var customer = await _orderService.Get(customerId, orderId);

                if (customer == null)
                {
                    Logger.Info($"No order with id {orderId} and order customer id {customerId}.");
                    return NotFound();
                }

                Logger.Info($"Retrieving  order after customer with id {customerId} and order with id {orderId}  to response.");
                return Ok(InitOrder(customer));
            }
            catch(OrderServiceException ex)
            {
                if (ex.Type == OrderServiceException.ErrorType.WrongCustomerId)
                {
                    Logger.Warn($"Wrong customerId has been request in an effort getting order with customer id: {customerId}", ex);
                    return BadRequest($"Wrong customerId has been request in an effort getting order with customer id: {customerId}");
                }
                else if(ex.Type == OrderServiceException.ErrorType.WrongOrderId)
                {
                    Logger.Warn($"Wrong order id has been request in an effort getting order with order id: {orderId}", ex);
                    return BadRequest($"Wrong order id has been request an effort getting  order with order id: {orderId}");
                }
                throw;
            }
            catch(Exception ex)
            {
                Logger.Error(ex, $"Some error occured while getting orders after customerId {customerId} and order with id {customerId}");
                throw;
            }
        }

        private object InitOrder(OrderModel model)
        {
            return new
            {
                _self = new UrlHelper(Request).Link("Order", new {customerId = model.CustomerId, orderId = model.Id}),
                customer = new UrlHelper(Request).Link("Customer", new {customerId = model.CustomerId}),
                data = model
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}