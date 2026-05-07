using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;

namespace EventEase.Controllers
{
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;

        public VenuesController(ApplicationDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        // GET: Venues (with search)
        public async Task<IActionResult> Index(string searchString)
        {
            var venues = from v in _context.Venues select v;
            if (!string.IsNullOrEmpty(searchString))
                venues = venues.Where(v => v.Name.Contains(searchString) || v.Location.Contains(searchString));
            ViewData["CurrentFilter"] = searchString;
            return View(await venues.ToListAsync());
        }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var venue = await _context.Venues.FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null) return NotFound();
            return View(venue);
        }

        // GET: Venues/Create
        public IActionResult Create() => View();

        // POST: Venues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueId,Name,Location,Capacity,Description")] Venue venue, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(venue);
            if (imageFile != null && imageFile.Length > 0)
                venue.ImageUrl = await _blobService.UploadFileAsync(imageFile);
            _context.Add(venue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();
            return View(venue);
        }

        // POST: Venues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,Name,Location,Capacity,Description")] Venue venue, IFormFile? imageFile)
        {
            if (id != venue.VenueId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(venue);

            // Fetch the existing venue from the database (no tracking needed for update later)
            var existingVenue = await _context.Venues.FindAsync(id);
            if (existingVenue == null)
                return NotFound();

            // Update scalar properties
            existingVenue.Name = venue.Name;
            existingVenue.Location = venue.Location;
            existingVenue.Capacity = venue.Capacity;
            existingVenue.Description = venue.Description;

            // Handle image upload or retention
            if (imageFile != null && imageFile.Length > 0)
            {
                // Delete old blob if present
                if (!string.IsNullOrEmpty(existingVenue.ImageUrl))
                {
                    await _blobService.DeleteFileAsync(existingVenue.ImageUrl);
                }
                // Upload new blob and set filename
                existingVenue.ImageUrl = await _blobService.UploadFileAsync(imageFile);
            }
            // else: image stays exactly as it was (existingVenue.ImageUrl unchanged)

            try
            {
                _context.Update(existingVenue);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VenueExists(venue.VenueId))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var venue = await _context.Venues.Include(v => v.Bookings).FirstOrDefaultAsync(v => v.VenueId == id);
            if (venue == null) return NotFound();
            if (venue.Bookings.Any())
            {
                TempData["Error"] = "Cannot delete this venue because it has existing bookings.";
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues.Include(v => v.Bookings).FirstOrDefaultAsync(v => v.VenueId == id);
            if (venue == null) return NotFound();
            if (venue.Bookings.Any())
            {
                TempData["Error"] = "Cannot delete this venue because it has active bookings.";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                if (!string.IsNullOrEmpty(venue.ImageUrl))
                    await _blobService.DeleteFileAsync(venue.ImageUrl);
                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Unable to delete venue. It may be referenced by other records.";
            }
            return RedirectToAction(nameof(Index));
        }

        // Blob image serving
        public async Task<IActionResult> Image(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return NotFound();
            var (stream, contentType) = await _blobService.GetFileAsync(fileName);
            if (stream == null) return NotFound();
            return File(stream, contentType ?? "application/octet-stream");
        }

        private bool VenueExists(int id) => _context.Venues.Any(e => e.VenueId == id);
    }
}