﻿using System.Linq;
using System.Threading.Tasks;
using Emby.Server.Implementations.Localization;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.Localization
{
    public class LocalizationManagerTests
    {
        private LocalizationManager _localizationManager = null!;

        public LocalizationManagerTests()
        {
            var config = new ServerConfiguration() { UICulture = "de-DE" };
            Setup(config);
        }

        [Fact]
        public void GetCountries_All_Success()
        {
            var countries = _localizationManager.GetCountries();
            var countryInfos = countries.ToList();

            Assert.Equal(139, countryInfos.Count);

            var germany = countryInfos.FirstOrDefault(x => x.Name == "DE");
            Assert.NotNull(germany);
            Assert.Equal("Germany", germany!.DisplayName);
            Assert.Equal("DEU", germany!.ThreeLetterISORegionName);
            Assert.Equal("DE", germany!.TwoLetterISORegionName);
        }

        [Fact]
        public async Task GetCultures_All_Success()
        {
            await _localizationManager.LoadAll();
            var cultures = _localizationManager.GetCultures().ToList();

            Assert.Equal(189, cultures.Count);

            var germany = cultures.FirstOrDefault(x => x.TwoLetterISOLanguageName == "de");
            Assert.NotNull(germany);
            Assert.Equal("ger", germany!.ThreeLetterISOLanguageName);
            Assert.Equal("German", germany!.DisplayName);
            Assert.Equal("German", germany!.Name);
            Assert.Contains("deu", germany!.ThreeLetterISOLanguageNames);
            Assert.Contains("ger", germany!.ThreeLetterISOLanguageNames);
        }

        [Theory]
        [InlineData("de")]
        [InlineData("ger")]
        [InlineData("german")]
        public async Task FindLanguage_Valid_Success(string identifier)
        {
            await _localizationManager.LoadAll();

            var germany = _localizationManager.FindLanguageInfo(identifier);
            Assert.NotNull(germany);

            Assert.Equal("ger", germany!.ThreeLetterISOLanguageName);
            Assert.Equal("German", germany!.DisplayName);
            Assert.Equal("German", germany!.Name);
            Assert.Contains("deu", germany!.ThreeLetterISOLanguageNames);
            Assert.Contains("ger", germany!.ThreeLetterISOLanguageNames);
        }

        [Fact]
        public async Task ParentalRatings_Default_Success()
        {
            await _localizationManager.LoadAll();
            var ratings = _localizationManager.GetParentalRatings().ToList();

            Assert.Equal(23, ratings.Count);

            var tvma = ratings.FirstOrDefault(x => x.Name == "TV-MA");
            Assert.NotNull(tvma);
            Assert.Equal(9, tvma!.Value);
        }

        [Fact]
        public async Task ParentalRatings_ConfiguredCountryCode_Success()
        {
            Setup(new ServerConfiguration()
            {
                MetadataCountryCode = "DE"
            });
            await _localizationManager.LoadAll();
            var ratings = _localizationManager.GetParentalRatings().ToList();

            Assert.Equal(10, ratings.Count);

            var fsk = ratings.FirstOrDefault(x => x.Name == "FSK-12");
            Assert.NotNull(fsk);
            Assert.Equal(7, fsk!.Value);
        }

        [Theory]
        [InlineData("CA-R", "CA", 10)]
        [InlineData("FSK-16", "DE", 8)]
        [InlineData("FSK-18", "DE", 9)]
        [InlineData("FSK-18", "US", 9)]
        [InlineData("TV-MA", "US", 9)]
        [InlineData("XXX", "asdf", 100)]
        [InlineData("Germany: FSK-18", "DE", 9)]
        public async Task GetRatingLevelFromString_Valid_Success(string value, string countryCode, int expectedLevel)
        {
            Setup(new ServerConfiguration()
            {
                MetadataCountryCode = countryCode
            });
            await _localizationManager.LoadAll();
            var level = _localizationManager.GetRatingLevel(value);
            Assert.NotNull(level);
            Assert.Equal(expectedLevel, level!);
        }

        [Fact]
        public async Task GetRatingLevelFromString_Unrated_Success()
        {
            await _localizationManager.LoadAll();
            Assert.Null(_localizationManager.GetRatingLevel("n/a"));
        }

        [Theory]
        [InlineData("Default", "Default")]
        [InlineData("HeaderLiveTV", "Live TV")]
        public void GetLocalizedString_Valid_Success(string key, string expected)
        {
            Setup(new ServerConfiguration()
            {
                UICulture = "en-US"
            });

            var translated = _localizationManager.GetLocalizedString(key);
            Assert.NotNull(translated);
            Assert.Equal(expected, translated);
        }

        [Fact]
        public void GetLocalizedString_Invalid_Success()
        {
            Setup(new ServerConfiguration()
            {
                UICulture = "en-US"
            });

            var key = "SuperInvalidTranslationKeyThatWillNeverBeAdded";

            var translated = _localizationManager.GetLocalizedString(key);
            Assert.NotNull(translated);
            Assert.Equal(key, translated);
        }

        private void Setup(ServerConfiguration config)
        {
            var mockConfiguration = new Mock<IServerConfigurationManager>();
            mockConfiguration.SetupGet(x => x.Configuration).Returns(config);

            _localizationManager = new LocalizationManager(mockConfiguration.Object, new NullLogger<LocalizationManager>());
        }
    }
}
