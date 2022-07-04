using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Users.Models;
using Microsoft.AspNetCore.Authorization;
namespace UserController
{
    [Route("Api/Books")]
    [ApiController]
    [Authorize(Roles ="User")]
    public class BookDashBoardController : ControllerBase
    {
        private readonly UserContext _context;
        public BookDashBoardController(UserContext context)
        {
            _context = context;
        }
        [HttpGet("GetBookss")]
        // [Authorize(Roles ="Admin,User")]
        public async Task<ActionResult<IEnumerable<Book>>> GetBookss()
        {
            if (_context.books == null)
            {
                return NotFound();
            }
            return await _context.books.ToListAsync();
        }
        

         private bool BookExists(int id)
        {
            return (_context.books?.Any(e => e.Id == id)).GetValueOrDefault();
        }





    }
}