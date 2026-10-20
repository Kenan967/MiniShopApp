using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.API.Data;
using OrderService.API.Models;
using OrderService.API.Services;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderDbContext _context;
    private readonly IProductServiceClient _productService;
    private readonly INotificationServiceClient _notificationService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        OrderDbContext context,
        IProductServiceClient productService,
        INotificationServiceClient notificationService,
        ILogger<OrdersController> logger)
    {
        _context = context;
        _productService = productService;
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order is null)
            return NotFound("Order not found.");

        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        // [ApiController] automatically returns 400 for invalid model data.
        if (request.Quantity <= 0)
            return BadRequest("Quantity must be greater than zero.");

        var email = request.Email.Trim();

        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email is required.");

        // 1. Get product information from ProductService.
        var product = await _productService.GetProductAsync(request.ProductId);

        if (product is null)
            return BadRequest("Product not found.");

        // 2. Check available stock.
        if (product.Stock < request.Quantity)
            return BadRequest("Not enough stock.");

        // 3. Create and save the order with the customer's email.
        var order = new Order
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Quantity = request.Quantity,
            UnitPrice = product.Price,
            TotalPrice = product.Price * request.Quantity,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // 4. Reduce stock. If this fails, remove the order and return an error.
        bool stockReduced;

        try
        {
            stockReduced = await _productService.ReduceStockAsync(
                product.Id,
                request.Quantity);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Could not contact ProductService for order {OrderId}.", order.Id);
            stockReduced = false;
        }

        if (!stockReduced)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return BadRequest("Order could not be completed because the stock update failed.");
        }

        // 5. Order is saved and stock is reduced. Immediately ask NotificationService
        // to send an order confirmation email to the email entered by the customer.
        var notificationSent = true;

        try
        {
            await _notificationService.SendAsync(new NotificationRequest
            {
                OrderId = order.Id,
                Email = order.Email,
                ProductName = order.ProductName,
                Quantity = order.Quantity,
                TotalPrice = order.TotalPrice
            });
        }
        catch (Exception ex)
        {
            // Do not delete a valid order because an email server is temporarily down.
            // Log the failure so it can be diagnosed.
            notificationSent = false;
            _logger.LogError(ex,
                "Order {OrderId} was created, but the confirmation email could not be sent to {Email}.",
                order.Id, order.Email);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = order.Id },
            new
            {
                order.Id,
                order.ProductId,
                order.ProductName,
                order.Quantity,
                order.UnitPrice,
                order.TotalPrice,
                order.Email,
                order.CreatedAt,
                notificationSent,
                message = notificationSent
                    ? "Order created successfully. Confirmation email sent."
                    : "Order created successfully, but confirmation email could not be sent. Please check NotificationService configuration."
            });
    }
}
