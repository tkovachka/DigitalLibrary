using LibraryApplication.Domain.DTO;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace LibraryApplication.Service.API
{
    public class GoogleBooksClient
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _http;
        private readonly string? _apiKey;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public GoogleBooksClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _configuration = config;
            _apiKey = _configuration["MyApiSettings:ApiKey"];
        }

        public async Task<List<BookDTO>> SearchAndParseAsync(string query, int totalMax = 10, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(query)) throw new ArgumentException("query required", nameof(query));
            if (totalMax <= 0) throw new ArgumentOutOfRangeException(nameof(totalMax));
            var results = new List<BookDTO>();
            int fetched = 0;
            int pageSize = Math.Min(40, Math.Max(1, totalMax)); // per-request max is 40
            int startIndex = 0;

            while (fetched < totalMax)
            {
                int requestSize = Math.Min(pageSize, totalMax - fetched);
                var url = BuildUrl(query, requestSize, startIndex);

                using var resp = await _http.GetAsync(url, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    var content = await resp.Content.ReadAsStringAsync(ct);
                    throw new HttpRequestException($"Google Books API returned {(int)resp.StatusCode}: {content}");
                }

                var stream = await resp.Content.ReadAsStreamAsync(ct);
                var root = await JsonSerializer.DeserializeAsync<ApiResponse>(stream, _jsonOptions, ct);

                if (root?.Items == null || root.Items.Count == 0) break;

                foreach (var item in root.Items)
                {
                    var dto = ParseVolumeItem(item);
                    if (dto != null) results.Add(dto);
                }
                fetched = results.Count;
                if (root.TotalItems <= startIndex + requestSize) break; // no more items on server
                startIndex += requestSize;
            }

            return results.Take(totalMax).ToList();
        }

        private string BuildUrl(string query, int maxResults, int startIndex)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("https://www.googleapis.com/books/v1/volumes");
            sb.Append("?q=").Append(Uri.EscapeDataString(query));
            sb.Append("&maxResults=").Append(maxResults);
            sb.Append("&startIndex=").Append(startIndex);
            // Request only the fields we need to reduce bandwidth (optional)
            sb.Append("&fields=totalItems,items(id,volumeInfo(title,subtitle,authors,publisher,publishedDate,industryIdentifiers,pageCount,language,averageRating,ratingsCount,description,imageLinks,categories))");
            if (!string.IsNullOrEmpty(_apiKey)) sb.Append("&key=").Append(_apiKey);
            return sb.ToString();

        }

        private BookDTO? ParseVolumeItem(VolumeItem item)
        {
            var info = item.VolumeInfo;
            if (info == null) return null;

            var dto = new BookDTO
            {
                Title = info.Title ?? string.Empty,
                Subtitle = info.Subtitle,
                Authors = info.Authors ?? new List<string>(),
                Publisher = info.Publisher,
                PublishedDateRaw = info.PublishedDate,
                PageCount = info.PageCount,
                Language = info.Language,
                AverageRating = info.AverageRating,
                RatingsCount = info.RatingsCount ?? 0,
                Description = info.Description,
                ThumbnailUrl = info.ImageLinks?.Thumbnail,
                Categories = info.Categories ?? new List<string>()
            };

            if (info.IndustryIdentifiers != null)
            {
                foreach (var id in info.IndustryIdentifiers)
                {
                    if (string.Equals(id?.Type, "ISBN_10", StringComparison.OrdinalIgnoreCase))
                        dto.Isbn10 = id?.Identifier;
                    else if (string.Equals(id?.Type, "ISBN_13", StringComparison.OrdinalIgnoreCase))
                        dto.Isbn13 = id?.Identifier;
                }
            }

            // Fallback: if ISBN-10 missing, use ISBN-13 into the Isbn10 slot.
            if (string.IsNullOrWhiteSpace(dto.Isbn10) && !string.IsNullOrWhiteSpace(dto.Isbn13))
                dto.Isbn10 = dto.Isbn13;

            // Parse PublishedDate (formats: yyyy, yyyy-MM, yyyy-MM-dd)
            dto.PublishedDate = TryParsePublishedDate(dto.PublishedDateRaw);

            return dto;
        }

        private static DateOnly? TryParsePublishedDate(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            raw = raw.Trim();
            // Accept "YYYY", "YYYY-MM", "YYYY-MM-DD"
            string[] formats = new[] { "yyyy", "yyyy-MM", "yyyy-MM-dd" };
            foreach (var fmt in formats)
            {
                if (DateOnly.TryParseExact(raw, fmt, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d))
                    return d;
            }
            // Last resort: try a loose parse
            if (DateTime.TryParse(raw, out var dt))
                return DateOnly.FromDateTime(dt);
            return null;
        }

    }
}
