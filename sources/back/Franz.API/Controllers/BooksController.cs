using Franz.Common.Mediator.Dispatchers;
using Franz.Contracts.Commands.Books;
using Franz.Contracts.DTOs;
using Franz.Contracts.Queries.Books;

namespace Franz.API.Controllers;
    
[ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
{
  private readonly IDispatcher _dispatcher;

  public BooksController(IDispatcher dispatcher)
  {
    _dispatcher = dispatcher;
  }

  [HttpGet("{id:int}")]
  public async Task<ActionResult<BookDto>> GetById(int id, CancellationToken ct)
  {
    var result = await _dispatcher.SendAsync(new GetBookByIdQuery(id), ct);
    return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error.Message);
  }

  [HttpGet("by-title/{title}")]
  public async Task<ActionResult<IReadOnlyList<BookDto>>> GetByTitle(string title, CancellationToken ct)
  {
    var result = await _dispatcher.SendAsync(new GetBookByTitleQuery(title), ct);
    return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error.Message);
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<BookDto>>> GetAll(CancellationToken ct)
  {
    var result = await _dispatcher.SendAsync(new ListBooksQuery(), ct);
    return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error.Message);
  }

  [HttpGet("by-author/{author}")]
  public async Task<ActionResult<IReadOnlyList<BookDto>>> GetByAuthor(string author, CancellationToken ct)
  {
    var result = await _dispatcher.SendAsync(new GetBooksByAuthorQuery(author), ct);
    return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error.Message);
  }

  [HttpPost]
  public async Task<ActionResult<Guid>> Create([FromBody] AddBookCommand command, CancellationToken ct)
  {
    var result = await _dispatcher.SendAsync(command, ct);
    if (result == 0)
      return BadRequest();

    return CreatedAtAction(nameof(GetById), new { id = result }, result);
  }

  [HttpPut("{id:int}")]
  public async Task<ActionResult> Update(int id, [FromBody] UpdateBookCommand command, CancellationToken ct)
  {
    if (id != command.Id)
      return BadRequest("Route ID and command ID must match.");

    var result = await _dispatcher.SendAsync(command, ct);
    return result.IsSuccess ? Ok() : NotFound(result.Error.Message);
  }

  [HttpDelete("{id:guid}")]
  public async Task<ActionResult> Delete(int id, CancellationToken ct)
  {
    var result = await _dispatcher.SendAsync(new DeleteBookCommand(id), ct);
    return result.IsSuccess ? NoContent() : NotFound(result.Error.Message);
  }
}