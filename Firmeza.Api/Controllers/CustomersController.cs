using AutoMapper;
using Firmeza.Admin.Models;
using Firmeza.API.DTOs.Customers;
using Firmeza.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly FirmezaDbContext _context;
    private readonly IMapper _mapper;

    public CustomersController(FirmezaDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // ---------------------------------------------------------
    // GET: api/customers
    // ---------------------------------------------------------
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
    {
        var customers = await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.FullName)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<CustomerDto>>(customers));
    }

    // ---------------------------------------------------------
    // GET: api/customers/{id}
    // ---------------------------------------------------------
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
            return NotFound($"Customer with ID {id} was not found.");

        return Ok(_mapper.Map<CustomerDto>(customer));
    }

    // ---------------------------------------------------------
    // POST: api/customers
    // ---------------------------------------------------------
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CustomerCreateDto dto)
    {
        // VALIDACIÓN BÁSICA
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // VALIDACIÓN DE UNICIDAD
        if (await _context.Customers.AnyAsync(c => c.DocumentNumber == dto.DocumentNumber))
            return Conflict("DocumentNumber already exists.");

        if (await _context.Customers.AnyAsync(c => c.Email == dto.Email))
            return Conflict("Email already exists.");

        if (await _context.Customers.AnyAsync(c => c.PhoneNumber == dto.PhoneNumber))
            return Conflict("PhoneNumber already exists.");

        var customer = _mapper.Map<Customer>(dto);

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var result = _mapper.Map<CustomerDto>(customer);

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, result);
    }

    // ---------------------------------------------------------
    // PUT: api/customers/{id}
    // ---------------------------------------------------------
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CustomerUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
            return NotFound($"Customer with ID {id} was not found.");

        // VALIDACIONES DE UNICIDAD EXCLUYENDO EL MISMO REGISTRO
        if (await _context.Customers.AnyAsync(c =>
                c.DocumentNumber == dto.DocumentNumber && c.Id != id))
            return Conflict("DocumentNumber already exists.");

        if (await _context.Customers.AnyAsync(c =>
                c.Email == dto.Email && c.Id != id))
            return Conflict("Email already exists.");

        if (await _context.Customers.AnyAsync(c =>
                c.PhoneNumber == dto.PhoneNumber && c.Id != id))
            return Conflict("PhoneNumber already exists.");

        _mapper.Map(dto, customer);

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ---------------------------------------------------------
    // DELETE: api/customers/{id}
    // ---------------------------------------------------------
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
            return NotFound($"Customer with ID {id} was not found.");

        // Protegemos para no borrar clientes con ventas
        var hasSales = await _context.Sales.AnyAsync(s => s.CustomerId == id);
        if (hasSales)
            return Conflict("Customer has sales and cannot be deleted.");

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
