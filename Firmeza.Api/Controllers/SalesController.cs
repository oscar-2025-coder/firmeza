using AutoMapper;
using Firmeza.API.DTOs.Sales;
using Firmeza.Infrastructure.Data;
using Firmeza.Infrastructure.Entities;   // ✅ ENTIDADES CORRECTAS
using Firmeza.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Firmeza.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SalesController : ControllerBase
    {
        private readonly FirmezaDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IPdfService _pdfService;

        public SalesController(FirmezaDbContext context, IMapper mapper, IEmailService emailService, IPdfService pdfService)
        {
            _context = context;
            _mapper = mapper;
            _emailService = emailService;
            _pdfService = pdfService;
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
            // Obtener customerId del claim JWT
            var claim = User.FindFirst("customerId");
            if (claim == null)
                return Unauthorized("Missing customerId claim.");
            if (!Guid.TryParse(claim.Value, out var customerIdFromClaim))
                return Unauthorized("Invalid customerId claim.");

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customerIdFromClaim);
            if (customer == null)
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
                CustomerId = customerIdFromClaim,
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

            // ---------------------------------------------------------
            // SEND EMAIL CONFIRMATION WITH PDF TO CUSTOMER
            // ---------------------------------------------------------
            var saleDto = _mapper.Map<SaleDto>(sale);

            // Generate PDF receipt
            var pdfBytes = _pdfService.GenerateSaleReceipt(sale);

            var body = new StringBuilder();
            body.Append($"<h2>¡Gracias por su compra, {customer.FullName}!</h2>");
            body.Append($"<p>Su comprobante de compra está adjunto en formato PDF.</p>");
            body.Append($"<p><strong>ID de Venta:</strong> {sale.Id}</p>");
            body.Append($"<p><strong>Fecha:</strong> {sale.Date.LocalDateTime:dd/MM/yyyy HH:mm}</p>");
            body.Append($"<h3>Resumen:</h3>");
            body.Append($"<p><strong>Total:</strong> ${sale.Total:N0} COP</p>");
            body.Append($"<p style='color: #666; font-size: 12px;'>Por favor, revise el comprobante adjunto para más detalles.</p>");

            await _emailService.SendEmailWithAttachmentAsync(
                customer.Email,
                "Comprobante de Compra - Firmeza",
                body.ToString(),
                pdfBytes,
                "comprobante.pdf"
            );

            return CreatedAtAction(nameof(GetById), new { id = sale.Id }, saleDto);
        }

        // ---------------------------------------------------------
        // PUT: api/sales/{id}
        // ---------------------------------------------------------
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, SaleUpdateDto dto)
        {
            var sale = await _context.Sales.FindAsync(id);

            if (sale == null)
                return NotFound($"Sale with ID {id} was not found.");

            sale.Notes = dto.Notes;

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
}
