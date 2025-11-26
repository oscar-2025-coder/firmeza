using AutoMapper;
using Firmeza.Admin.Models;
using Firmeza.API.DTOs.Products;
using Firmeza.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly FirmezaDbContext _context;
    private readonly IMapper _mapper;

    public ProductsController(FirmezaDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        var products = await _context.Products.AsNoTracking().ToListAsync();
        return Ok(_mapper.Map<IEnumerable<ProductDto>>(products));
    }

    // GET api/products/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
            return NotFound(new { message = $"Product with ID {id} not found." });

        return Ok(_mapper.Map<ProductDto>(product));
    }

    // POST api/products
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(ProductCreateDto dto)
    {
        var entity = _mapper.Map<Product>(dto);

        _context.Products.Add(entity);
        await _context.SaveChangesAsync();

        var result = _mapper.Map<ProductDto>(entity);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
    }

    // PUT api/products/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ProductUpdateDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound(new { message = $"Product with ID {id} not found." });

        _mapper.Map(dto, product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE api/products/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound(new { message = $"Product with ID {id} not found." });

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
