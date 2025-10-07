using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace SafeKeyRecorder.Tests.UiAutomation;

public class BackgroundBannerToggleTests : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    [Fact(Skip = "Assignment scope does not include Playwright UI automation")]
    public async Task Toggle_ShouldShowAndHideBackgroundBanner()
    {
        if (_browser is null)
        {
            Assert.Fail("Browser not initialized");
        }

        var page = await _browser.NewPageAsync();
        await page.GotoAsync("http://localhost:5000");

        await page.ClickAsync("#start-consent");
        await page.ClickAsync("#allow-background-toggle");
        await page.ClickAsync("#consent-continue");

        var bannerVisible = await page.IsVisibleAsync("#background-banner");
        Assert.True(bannerVisible);

        await page.ClickAsync("#background-toggle");
        bannerVisible = await page.IsVisibleAsync("#background-banner");
        Assert.False(bannerVisible);
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.DisposeAsync();
        }

        _playwright?.Dispose();
    }
}
