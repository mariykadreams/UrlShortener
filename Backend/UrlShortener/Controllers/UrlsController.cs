using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UrlShortener.API.Data;
using UrlShortener.API.Models;
using UrlShortener.API.Services;

namespace UrlShortener.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All actions in this controller require authentication by default
    public class UrlsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUrlShorteningService _urlShorteningService;
        private readonly UserManager<IdentityUser> _userManager;

        public UrlsController(AppDbContext context, IUrlShorteningService urlShorteningService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _urlShorteningService = urlShorteningService;
            _userManager = userManager;
        }

        /// <summary>
        /// Gets all shortened URLs. Anonymous users can view, authorized users can view their own, and admins can view all.
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // Anonymous users can see the table
        public async Task<IActionResult> GetAllShortenedUrls()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var urls = await _context.ShortenedUrls.ToListAsync();

            var result = new List<object>();

            foreach (var url in urls)
            {
                string createdByName = "Anonymous";

                if (!string.IsNullOrEmpty(url.CreatedByUserId))
                {
                    var user = await _userManager.FindByIdAsync(url.CreatedByUserId);
                    createdByName = user?.UserName ?? "Unknown";
                }

                result.Add(new
                {
                    url.Id,
                    url.OriginalUrl,
                    url.ShortCode,
                    url.CreatedDate,
                    url.CreatedByUserId,
                    CreatedBy = createdByName
                });
            }

            return Ok(result);
        }


        /// <summary>
        /// Gets details for a specific shortened URL. Anonymous users cannot access this.
        /// </summary>
        /// <param name="id">The ID of the shortened URL.</param>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetShortenedUrlDetails(int id)
        {
            var url = await _context.ShortenedUrls
                                    .Include(su => su.CreatedByUser)
                                    .FirstOrDefaultAsync(u => u.Id == id);

            if (url == null)
            {
                return NotFound("Shortened URL not found.");
            }

            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            if (!isAuthenticated)
            {
                return Forbid("Only authenticated users can view URL details.");
            }

            return Ok(new
            {
                url.Id,
                url.OriginalUrl,
                url.ShortCode,
                url.CreatedDate,
                CreatedBy = url.CreatedByUser?.UserName ?? "Unknown"
            });
        }



        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> AddShortenedUrl([FromBody] CreateShortenedUrlRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            Console.WriteLine($"--- AddShortenedUrl Request ---");
            Console.WriteLine($"Token Info: OriginalUrl = {request.OriginalUrl}");
            Console.WriteLine($"Token Info: Retrieved User ID (ClaimTypes.NameIdentifier) = {currentUserId ?? "NULL/EMPTY"}");
            Console.WriteLine($"Token Info: Is User Admin? = {isAdmin}");
            Console.WriteLine($"-------------------------------");
            // --- END: Added Console.WriteLine statements ---

            Console.WriteLine($"CurrentUserId: {currentUserId}");

            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("Token is invalid or missing NameIdentifier");

            var existingUrl = await _context.ShortenedUrls
                .FirstOrDefaultAsync(u => u.OriginalUrl == request.OriginalUrl);

            if (existingUrl != null && (existingUrl.CreatedByUserId == currentUserId || isAdmin))
            {
                return Conflict("This URL has already been shortened by you or an admin.");
            }

            // FIX: Pass request.OriginalUrl to GenerateShortCode
            var shortCode = _urlShorteningService.GenerateShortCode(request.OriginalUrl);
            while (await _context.ShortenedUrls.AnyAsync(u => u.ShortCode == shortCode))
            {
                // If a collision occurs even with the hashing, generate a new one
                // This might indicate a very high collision rate or issues with the hashing input.
                shortCode = _urlShorteningService.GenerateShortCode(request.OriginalUrl + Guid.NewGuid().ToString());
            }

            var shortenedUrl = new ShortenedUrl
            {
                OriginalUrl = request.OriginalUrl,
                ShortCode = shortCode,
                CreatedDate = DateTime.UtcNow,
                CreatedByUserId = currentUserId
            };

            _context.ShortenedUrls.Add(shortenedUrl);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetShortenedUrlDetails), new { id = shortenedUrl.Id }, new
            {
                shortenedUrl.Id,
                shortenedUrl.OriginalUrl,
                shortenedUrl.ShortCode,
                shortenedUrl.CreatedDate,
                CreatedBy = User.Identity?.Name ?? "Unknown"
            });
        }




        /// <summary>
        /// Deletes a shortened URL. Only the creator or an admin can delete.
        /// </summary>
        /// <param name="id">The ID of the shortened URL to delete.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShortenedUrl(int id)
        {
            var urlToDelete = await _context.ShortenedUrls.FindAsync(id);

            if (urlToDelete == null)
            {
                return NotFound("Shortened URL not found.");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Authorization check: User can delete their own URLs, Admin can delete any
            if (urlToDelete.CreatedByUserId != currentUserId && !isAdmin)
            {
                return Forbid("You do not have permission to delete this URL.");
            }

            _context.ShortenedUrls.Remove(urlToDelete);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content for successful deletion
        }

        /// <summary>
        /// Redirects to the original URL given a short code. Accessible by everyone.
        /// </summary>
        /// <param name="shortCode">The short code to redirect from.</param>
        [HttpGet("redirect/{shortCode}")]
        [AllowAnonymous] // Everyone can access this for redirection
        public async Task<IActionResult> RedirectToOriginalUrl(string shortCode)
        {
            var url = await _context.ShortenedUrls.FirstOrDefaultAsync(u => u.ShortCode == shortCode);

            if (url == null)
            {
                return NotFound("Shortened URL not found.");
            }

            return Redirect(url.OriginalUrl);
        }
    }
}