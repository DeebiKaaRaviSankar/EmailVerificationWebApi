using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Users.Models;
using Microsoft.AspNetCore.Authorization;
namespace AdminControllers
{
    [Route("Api/Books")]
    [ApiController]
    [Authorize(Roles ="Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserContext _context;
        public AdminController(UserContext context)
        {
            _context = context;
        }
        [HttpGet("GetBooks")]
        // [Authorize(Roles ="Admin,User")]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            if (_context.books == null)
            {
                return NotFound();
            }
            return await _context.books.ToListAsync();
        }
        [HttpGet("GetBookById/{id}")]
        // [Authorize(Roles ="Admin,User")]
        public async Task<ActionResult<Book>> GetBookById(int id){
            if (_context.books == null)
            {
                return NotFound();
            }
            var findEmployee=await _context.books.FindAsync(id);
            if(findEmployee==null){
                return NotFound();
            }
            return findEmployee;
        }
        [HttpPost]
        public async Task<ActionResult<Book>> PostBook(Book book){
            if(_context.books==null){
                return Problem("Entity set 'userDbcontext.books'  is null.");
            }
            _context.books.Add(book);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetBooks", new { id = book.Id }, book);
        }
        [HttpPut("UpdateBooks/{id}")]
        // [Authorize(Roles ="Admin")]
        public async Task<IActionResult> PutBook(int id, Book book)
        {
            if (id != book.Id)
            {
                return BadRequest();
            }

            _context.Entry(book).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
         [HttpDelete("DeleteBook/{id}")]
        //  [Authorize(Roles ="Admin")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            if (_context.books == null)
            {
                return NotFound();
            }
            var employee = await _context.books.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            _context.books.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }


         private bool BookExists(int id)
        {
            return (_context.books?.Any(e => e.Id == id)).GetValueOrDefault();
        }





    }
}