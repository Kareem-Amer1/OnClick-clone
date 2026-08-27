using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Talabat.APlS.DTOs;
using Talabat.APlS.Errors;
using Talabat.Core.Entites;
using Talabat.Core.Entites.Order_Aggregate;
using Talabat.Core.Repositories;
using Talabat.Repository.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Talabat.APlS.Controllers
{
    [Authorize]
    public class BrandOrdersController : APIBaseController
    {
        private readonly IGenericRepository<Order> _ordersRepo;
        private readonly IGenericRepository<OrderItem> _orderItemsRepo;
        private readonly StoreContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<BrandOrdersController> _logger;
        private readonly IConfiguration _configuration;

        public BrandOrdersController(
            IGenericRepository<Order> ordersRepo,
            IGenericRepository<OrderItem> orderItemsRepo,
            StoreContext context,
            IMapper mapper,
            ILogger<BrandOrdersController> logger,
            IConfiguration configuration)
        {
            _ordersRepo = ordersRepo;
            _orderItemsRepo = orderItemsRepo;
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OrderItemDto>>> GetBrandPendingOrders()
        {
            try
            {
                _logger.LogInformation("Getting brand pending orders");
                
                // Get brand id from claims
                var brandIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (brandIdClaim == null)
                    return Unauthorized(new ApiResponse(401, "Unauthorized"));

                int brandId = int.Parse(brandIdClaim.Value);
                _logger.LogInformation($"Brand ID: {brandId}");

                // Get all products for this brand
                var brandProducts = await _context.Products
                    .Where(p => p.ProductBrandId == brandId)
                    .Select(p => p.Id)
                    .ToListAsync();
                
                if (brandProducts.Count == 0)
                {
                    _logger.LogWarning($"No products found for brand {brandId}");
                    return Ok(new List<object>());
                }

                _logger.LogInformation($"Found {brandProducts.Count} products for brand {brandId}");

                // Simplified approach: Get all pending order items
                var allOrderItems = await _context.OrderItems
                    .Include(oi => oi.Product)
                    .Where(oi => oi.View == "Pending")
                    .ToListAsync();

                // Filter by brand products
                var orderItems = allOrderItems
                    .Where(oi => brandProducts.Contains(oi.Product.ProductId))
                    .ToList();

                _logger.LogInformation($"Found {orderItems.Count} pending items for brand {brandId}");

                // Get all orders (this is not efficient but will work for now)
                var orders = await _context.Orders
                    .Include(o => o.ShippingAddress)
                    .Include(o => o.Items)
                    .ToListAsync();

                var result = new List<object>();

                foreach (var item in orderItems)
                {
                    // Find the order that contains this item
                    var order = orders.FirstOrDefault(o => o.Items.Any(i => i.Id == item.Id));

                    if (order != null)
                    {
                        // Create a more comprehensive order item object
                        result.Add(new
                        {
                            id = item.Id,
                            orderId = order.Id,
                            orderDate = order.OrderDate,
                            productId = item.Product.ProductId,
                            productName = item.Product.ProductName,
                            price = item.Price,
                            quantity = item.Quantity,
                            pictureUrl = !string.IsNullOrEmpty(item.Product.PictureUrl) 
                                ? $"{_configuration["ApiBaseUrl"]}{item.Product.PictureUrl}"
                                : string.Empty,
                            buyerName = order.BuyerEmail?.Split('@')[0] ?? "Customer",
                            deliveryAddress = order.ShippingAddress != null 
                                ? $"{order.ShippingAddress.Street}, {order.ShippingAddress.City}, {order.ShippingAddress.Country}"
                                : "No address provided",
                            phoneNumber = order.ShippingAddress?.PhoneNumber ?? "No phone provided",
                            view = item.View
                        });
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending orders");
                return StatusCode(500, new ApiResponse(500, $"An error occurred: {ex.Message}"));
            }
        }

        [HttpPut("update-status/{id}")]
        public async Task<ActionResult> UpdateOrderItemStatus(int id, [FromBody] UpdateOrderItemDto updateDto)
        {
            try
            {
                // Get brand id from claims
                var brandIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (brandIdClaim == null)
                    return Unauthorized(new ApiResponse(401, "Unauthorized"));

                int brandId = int.Parse(brandIdClaim.Value);

                // Get the order item with product
                var orderItem = await _context.OrderItems
                    .Include(oi => oi.Product)
                    .FirstOrDefaultAsync(oi => oi.Id == id);

                if (orderItem == null)
                    return NotFound(new ApiResponse(404, "Order item not found"));

                // Get product details to verify brand ownership
                var product = await _context.Products.FindAsync(orderItem.Product.ProductId);
                
                if (product == null)
                    return NotFound(new ApiResponse(404, "Product not found"));
                
                // Verify this product belongs to the brand
                if (product.ProductBrandId != brandId)
                    return Forbid();

                // Update the status
                orderItem.View = updateDto.Status;
                _context.OrderItems.Update(orderItem);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Order item status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating order item status: {id}");
                return StatusCode(500, new ApiResponse(500, $"An error occurred: {ex.Message}"));
            }
        }
    }

    public class UpdateOrderItemDto
    {
        public string Status { get; set; }
    }
} 