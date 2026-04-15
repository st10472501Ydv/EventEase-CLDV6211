using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Events.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Event evt)
        {
            if (ModelState.IsValid)
            {
                _context.Add(evt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(evt);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var evt = await _context.Events.FirstOrDefaultAsync(m => m.EventId == id);
            if (evt == null) return NotFound();
            return View(evt);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var evt = await _context.Events.FindAsync(id);
            if (evt == null) return NotFound();
            return View(evt);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Event evt)
        {
            if (id != evt.EventId) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(evt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(evt);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var evt = await _context.Events.Include(e => e.Booking).FirstOrDefaultAsync(e => e.EventId == id);
            if (evt == null) return NotFound();

            if (evt.Booking != null)
            {
                TempData["Error"] = "Cannot delete event with existing booking";
                return RedirectToAction(nameof(Index));
            }

            return View(evt);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var evt = await _context.Events.FindAsync(id);
            if (evt != null)
            {
                _context.Events.Remove(evt);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}