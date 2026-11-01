using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace QuestForge.E2ETests;

public sealed class QuestFlowTests : PageTest
{
    [Fact]
    public async Task User_can_register_create_and_complete_a_quest()
    {
        var baseUrl = Environment.GetEnvironmentVariable("QUESTFORGE_BASE_URL") ?? "http://localhost:8080";
        await Page.GotoAsync($"{baseUrl}/Account/Register");
        var email = $"e2e-{Guid.NewGuid():N}@example.test";
        await Page.GetByLabel("Email").FillAsync(email);
        await Page.GetByLabel("Password", new() { Exact = true }).FillAsync("QuestForge1!");
        await Page.GetByLabel("Confirm password").FillAsync("QuestForge1!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Page.GotoAsync($"{baseUrl}/quests");
        await Page.WaitForTimeoutAsync(1_000);
        await Page.GetByLabel("Title").FillAsync("Finish E2E scenario");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Add to quest log" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Status)).ToContainTextAsync("Quest added.");
        var questCard = Page.Locator("article.quest-card").Filter(new() { HasText = "Finish E2E scenario" });
        await questCard.GetByRole(AriaRole.Button, new() { Name = "Complete" }).ClickAsync();
        await Expect(Page.GetByText("XP awarded once")).ToBeVisibleAsync();
    }
}
