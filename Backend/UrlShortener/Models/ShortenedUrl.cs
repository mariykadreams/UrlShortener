using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace UrlShortener.API.Models
{
    public class ShortenedUrl
    {
        public int Id { get; set; }

        [Required]
        [Url]
        public string OriginalUrl { get; set; } = string.Empty;

        [Required]
        [StringLength(10, MinimumLength = 7)]
        public string ShortCode { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public string CreatedByUserId { get; set; } = string.Empty;
        public IdentityUser? CreatedByUser { get; set; } 
    }

    public class CreateShortenedUrlRequest
    {
        [Required]
        [Url]
        public string OriginalUrl { get; set; } = string.Empty;
    }
}