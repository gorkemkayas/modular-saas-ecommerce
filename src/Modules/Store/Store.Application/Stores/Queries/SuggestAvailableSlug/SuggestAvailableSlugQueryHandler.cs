using Store.Application.DTOs;
using Store.Domain.Stores;
using Store.Domain.ValueObjects;
using System.Text;
using System.Text.RegularExpressions;

namespace Store.Application.Stores.Queries.SuggestAvailableSlug
{
    public sealed class SuggestAvailableSlugQueryHandler
    {
        private readonly IStoreRepository _storeRepository;

        public SuggestAvailableSlugQueryHandler(IStoreRepository storeRepository)
        {
            _storeRepository = storeRepository;
        }

        public async Task<SlugSuggestionDto> Handle(
            SuggestAvailableSlugQuery query,
            CancellationToken cancellationToken = default)
        {
            var baseSlugValue = GenerateSlug(query.Name);
            var baseSlug = Slug.Create(baseSlugValue);

            if (!await _storeRepository.ExistsBySlugAsync(baseSlug, cancellationToken))
                return new SlugSuggestionDto(baseSlug.Value);

            for (var i = 2; i <= 1000; i++)
            {
                var candidate = Slug.Create($"{baseSlug.Value}-{i}");

                var exists = await _storeRepository.ExistsBySlugAsync(candidate, cancellationToken);
                if (!exists)
                    return new SlugSuggestionDto(candidate.Value);
            }

            throw new InvalidOperationException("Could not generate an available slug.");
        }

        private static string GenerateSlug(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name cannot be empty.");

            var normalized = value.Trim().ToLowerInvariant();

            normalized = RemoveDiacritics(normalized);

            normalized = normalized.Replace("ı", "i")
                                   .Replace("ğ", "g")
                                   .Replace("ü", "u")
                                   .Replace("ş", "s")
                                   .Replace("ö", "o")
                                   .Replace("ç", "c");

            normalized = Regex.Replace(normalized, @"[^a-z0-9\s-]", "");
            normalized = Regex.Replace(normalized, @"\s+", "-");
            normalized = Regex.Replace(normalized, @"-+", "-");
            normalized = normalized.Trim('-');

            if (string.IsNullOrWhiteSpace(normalized))
                throw new ArgumentException("A valid slug could not be generated from the provided name.");

            return normalized;
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
