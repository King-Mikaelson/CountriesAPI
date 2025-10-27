using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CountriesAPI.Models;
using CountriesAPI.Services;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CountryCurrencyAPI.Services
{
    public class ImageService : IImageService
    {
        private readonly string _cachePath;

        private readonly Font? _titleFont;
        private readonly Font? _textFont;
        private readonly ILogger<ImageService> _logger;

        public ImageService(
            ILogger<ImageService> logger
            )
        {
            _logger = logger;
            _cachePath = Path.Combine(Directory.GetCurrentDirectory(), "cache");

            try
            {
                Directory.CreateDirectory(_cachePath);

                var collection = new FontCollection();
                FontFamily family;
                var arialFamily = SystemFonts.Collection.Families.FirstOrDefault(f => f.Name == "Arial");
                if (arialFamily.Name == "Arial")
                {
                    family = arialFamily;
                }
                else
                {
                    family = SystemFonts.Collection.Families.First();
                }

                _titleFont = family.CreateFont(24, FontStyle.Bold);
                _textFont = family.CreateFont(16, FontStyle.Regular);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize ImageService");
                throw;
            }
        }

        public async Task GenerateSummaryImageAsync(List<Country> countries, DateTime lastRefreshedAt)
        {
            await Task.Run(() => GenerateSummaryImage(countries, lastRefreshedAt));
        }


        public void GenerateSummaryImage(List<Country> countries, DateTime lastRefreshedAt)
        {
            var imagePath = Path.Combine(_cachePath, "summary.png");

            using var image = new Image<Rgba32>(800, 600, Color.White);
            var graphicsOptions = new GraphicsOptions
            {
                Antialias = true
            };

            float y = 20;

            // Draw title
            image.Mutate(ctx => ctx.DrawText("Country Summary", _titleFont, Color.Black, new PointF(20, y)));
            y += 60;

            // Total countries
            image.Mutate(ctx => ctx.DrawText($"Total Countries: {countries.Count}", _textFont, Color.Black, new PointF(20, y)));
            y += 40;

            // Last refreshed
            image.Mutate(ctx => ctx.DrawText($"Last Refresh: {lastRefreshedAt:yyyy-MM-dd HH:mm:ss}", _textFont, Color.Black, new PointF(20, y)));
            y += 50;

            // Top 5 by GDP
            var topCountries = countries
                .Where(c => c.estimated_gdp.HasValue)
                .OrderByDescending(c => c.estimated_gdp)
                .Take(5)
                .ToList();

            image.Mutate(ctx => ctx.DrawText("Top 5 by Estimated GDP:", _textFont, Color.Black, new PointF(20, y)));
            y += 40;

            int rank = 1;
            foreach (var c in topCountries)
            {
                image.Mutate(ctx => ctx.DrawText($"{rank}. {c.name} — {c.estimated_gdp:N2}", _textFont, Color.Black, new PointF(40, y)));
                y += 30;
                rank++;
            }

            // Save image
            image.Save(imagePath);
        }

        public string? GetSummaryImagePath()
        {
            var imagePath = Path.Combine(_cachePath, "summary.png");
            return File.Exists(imagePath) ? imagePath : null;
        }
    }
}
