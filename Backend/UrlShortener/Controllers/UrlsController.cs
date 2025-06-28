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
    [Authorize]
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

        [HttpGet]
        [AllowAnonymous]
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
        public async Task<IActionResult> AddShortenedUrl([FromBody] CreateShortenedUrlRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("Token is invalid or missing NameIdentifier");

            var existingUrl = await _context.ShortenedUrls
                .FirstOrDefaultAsync(u => u.OriginalUrl == request.OriginalUrl);

            if (existingUrl != null && (existingUrl.CreatedByUserId == currentUserId || isAdmin))
            {
                return Conflict("This URL has already been shortened by you or an admin.");
            }

            var shortCode = _urlShorteningService.GenerateShortCode(request.OriginalUrl);
            while (await _context.ShortenedUrls.AnyAsync(u => u.ShortCode == shortCode))
            {
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

            if (urlToDelete.CreatedByUserId != currentUserId && !isAdmin)
            {
                return Forbid("You do not have permission to delete this URL.");
            }

            _context.ShortenedUrls.Remove(urlToDelete);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("redirect/{shortCode}")]
        [AllowAnonymous]
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
