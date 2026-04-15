using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(ApplicationDbContext context, ILogger<BookingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Bookings.Include(b => b.Venue).Include(b => b.Event).ToListAsync();
            return View(bookings);
        }

        public async Task<IActionResult> Create()
        {
            var venues = await _context.Venues.Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = v.VenueId.ToString(), Text = v.Name }).ToListAsync();
            var events = await _context.Events.Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = e.EventId.ToString(), Text = e.Name }).ToListAsync();
            ViewData["VenueId"] = venues;
            ViewData["EventId"] = events;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueId,EventId,Notes")] Booking booking)
        {
            // Validate selections
            if (booking.VenueId <= 0)
            {
                ModelState.AddModelError("VenueId", "Please select a venue.");
            }
            if (booking.EventId <= 0)
            {
                ModelState.AddModelError("EventId", "Please select an event.");
            }

            // Log incoming values to help debug form-post issues
            _logger.LogDebug("Create Booking POST: VenueId={VenueId}, EventId={EventId}, Notes={Notes}", booking.VenueId, booking.EventId, booking.Notes);
            // Log raw form values
            try
            {
                var formEntries = Request.Form.Select(kv => $"{kv.Key}={kv.Value}");
                _logger.LogDebug("Request.Form entries: {FormEntries}", string.Join(";", formEntries));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read Request.Form");
            }

            // Only proceed with event-specific checks when a valid event id is supplied
            Event? evt = null;
            if (booking.EventId > 0)
            {
                evt = await _context.Events.FindAsync(booking.EventId);
                if (evt == null)
                {
                    ModelState.AddModelError("EventId", "Selected event not found.");
                }
            }

            // Check if event already booked
            if (booking.EventId > 0 && await _context.Bookings.AnyAsync(b => b.EventId == booking.EventId))
            {
                ModelState.AddModelError("EventId", "This event is already booked to a venue.");
            }

            // Check for venue double-booking when event exists
            if (evt != null)
            {
                var conflict = await _context.Bookings
                    .Include(b => b.Event)
                    .Where(b => b.VenueId == booking.VenueId && b.EventId != booking.EventId)
                    .Where(b => (evt.StartDate < b.Event.EndDate && evt.EndDate > b.Event.StartDate))
                    .FirstOrDefaultAsync();

                if (conflict != null)
                {
                    ModelState.AddModelError("VenueId", "This venue is already booked for these dates.");
                }
            }

            // Log ModelState errors for debugging
            foreach (var kv in ModelState)
            {
                var errs = string.Join(";", kv.Value.Errors.Select(e => e.ErrorMessage));
                if (!string.IsNullOrEmpty(errs)) _logger.LogDebug("ModelState[{Key}] = {Errors}", kv.Key, errs);
            }

            if (ModelState.IsValid)
            {
                booking.BookingDate = DateTime.Now;
                booking.Status = "Confirmed";
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Collect model state errors for debugging/helpful feedback
            var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            if (!string.IsNullOrEmpty(errors)) TempData["CreateErrors"] = errors;

            var venues = await _context.Venues.Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = v.VenueId.ToString(), Text = v.Name, Selected = v.VenueId == booking.VenueId }).ToListAsync();
            var events = await _context.Events.Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = e.EventId.ToString(), Text = e.Name, Selected = e.EventId == booking.EventId }).ToListAsync();
            ViewData["VenueId"] = venues;
            ViewData["EventId"] = events;
            return View(booking);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings.Include(b => b.Venue).Include(b => b.Event)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings.Include(b => b.Venue).Include(b => b.Event)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}