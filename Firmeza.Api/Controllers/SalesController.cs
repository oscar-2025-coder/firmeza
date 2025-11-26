using AutoMapper;
using Firmeza.Admin.Models;
using Firmeza.API.DTOs.Sales;
using Firmeza.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly FirmezaDbContext _context;
    private readonly IMapper _mapper;

    public SalesController(FirmezaDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // ---------------------------------------------------------
    // GET: api/sales
    // ---------------------------------------------------------
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SaleDto>>> GetAll()
    {
        var sales = await _context.Sales
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .Include(s => s.Customer)
            .AsNoTracking()
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<SaleDto>>(sales));
    }

    // ---------------------------------------------------------
    // GET: api/sales/{id}
    // ---------------------------------------------------------
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SaleDto>> GetById(Guid id)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .Include(s => s.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale == null)
            return NotFound($"Sale with ID {id} was not found.");

        return Ok(_mapper.Map<SaleDto>(sale));
    }

    // ---------------------------------------------------------
    // POST: api/sales
    // ---------------------------------------------------------
    [HttpPost]
    public async Task<ActionResult<SaleDto>> Create(SaleCreateDto dto)
    {
        // Validate Customer
        if (!await _context.Customers.AnyAsync(c => c.Id == dto.CustomerId))
            return BadRequest("Customer does not exist.");

        if (dto.Items == null || dto.Items.Count == 0)
            return BadRequest("Sale must include at least one item.");

        // Validate product IDs
        var productIds = dto.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        if (products.Count != dto.Items.Count)
            return BadRequest("Some product IDs are invalid.");

        // Create sale
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            CustomerId = dto.CustomerId,
            Date = DateTimeOffset.UtcNow,
            Notes = dto.Notes,
            Status = SaleStatus.Confirmed,
            Items = new List<SaleItem>()
        };

        decimal subtotal = 0;

        foreach (var itemDto in dto.Items)
        {
            var product = products.First(p => p.Id == itemDto.ProductId);

            var amount = itemDto.Quantity * product.UnitPrice;
            subtotal += amount;

            sale.Items.Add(new SaleItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                UnitPrice = product.UnitPrice,
                Amount = amount
            });
        }

        sale.Subtotal = subtotal;
        sale.Tax = subtotal * 0.19m;
        sale.Total = sale.Subtotal + sale.Tax;

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = sale.Id }, _mapper.Map<SaleDto>(sale));
    }

    // ---------------------------------------------------------
    // PUT: api/sales/{id}
    // Only updates Notes + Status
    // ---------------------------------------------------------
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, SaleUpdateDto dto)
    {
        var sale = await _context.Sales.FindAsync(id);

        if (sale == null)
            return NotFound($"Sale with ID {id} was not found.");

        sale.Notes = dto.Notes;

        // Validate enum
        if (!Enum.TryParse<SaleStatus>(dto.Status, true, out var newStatus))
            return BadRequest("Invalid status value.");

        sale.Status = newStatus;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ---------------------------------------------------------
    // DELETE: api/sales/{id}
    // ---------------------------------------------------------
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale == null)
            return NotFound($"Sale with ID {id} was not found.");

        _context.SaleItems.RemoveRange(sale.Items);
        _context.Sales.Remove(sale);

        await _context.SaveChangesAsync();
        return NoContent();
    }
}
