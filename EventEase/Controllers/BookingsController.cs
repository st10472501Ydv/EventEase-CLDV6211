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

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings – with search and consolidated display
        public async Task<IActionResult> Index(string searchString)
        {
            var bookings = _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b =>
                    b.Venue.Name.Contains(searchString) ||
                    b.Event.Name.Contains(searchString) ||
                    b.Status.Contains(searchString) ||
                    b.BookingId.ToString().Contains(searchString));  // partial match on booking ID
            }

            ViewData["CurrentFilter"] = searchString;
            return View(await bookings.ToListAsync());
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name");
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Name");
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,VenueId,EventId,BookingDate,Status,Notes")] Booking booking)
        {
            if (!ModelState.IsValid)
            {
                ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name", booking.VenueId);
                ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Name", booking.EventId);
                return View(booking);
            }

            // 1. Check that the selected event exists
            var selectedEvent = await _context.Events.FindAsync(booking.EventId);
            if (selectedEvent == null)
            {
                ModelState.AddModelError("EventId", "The selected event does not exist.");
                ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name", booking.VenueId);
                ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Name", booking.EventId);
                return View(booking);
            }

            // 2. One event → one booking rule
            bool eventAlreadyBooked = await _context.Bookings.AnyAsync(b => b.EventId == booking.EventId);
            if (eventAlreadyBooked)
            {
                ModelState.AddModelError("EventId", $"The event '{selectedEvent.Name}' is already booked. Each event can only be booked once.");
                ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name", booking.VenueId);
                ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Name", booking.EventId);
                return View(booking);
            }

            // 3. Double‑booking: same venue with overlapping date/time
            var conflictingBookings = await _context.Bookings
                .Include(b => b.Event)
                .Where(b => b.VenueId == booking.VenueId)
                .Where(b => b.Event.StartDate < selectedEvent.EndDate && b.Event.EndDate > selectedEvent.StartDate)
                .ToListAsync();

            if (conflictingBookings.Any())
            {
                ModelState.AddModelError("VenueId", $"The venue is already booked for an event that overlaps with '{selectedEvent.Name}'. Please choose a different venue or change the event dates.");
                ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name", booking.VenueId);
                ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Name", booking.EventId);
                return View(booking);
            }

            // 4. All good
            _context.Add(booking);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Booking created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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