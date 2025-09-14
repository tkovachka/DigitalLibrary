using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryApplication.Domain.DTO
{
    public class VolumeInfo
    {
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public List<string>? Authors { get; set; }
        public string? Publisher { get; set; }
        public string? PublishedDate { get; set; } // "YYYY" or "YYYY-MM" or "YYYY-MM-DD"
        public List<IndustryIdentifier>? IndustryIdentifiers { get; set; }
        public int? PageCount { get; set; }
        public string? Language { get; set; } // ISO 639-1
        public double? AverageRating { get; set; }
        public int? RatingsCount { get; set; }
        public string? Description { get; set; }
        public ImageLinks? ImageLinks { get; set; }
        public List<string>? Categories { get; set; }
    }
}
