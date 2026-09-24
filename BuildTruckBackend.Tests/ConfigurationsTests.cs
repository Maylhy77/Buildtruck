using BuildTruckConfigurationService.Configurations.Domain.Model.Aggregates;
using BuildTruckConfigurationService.Configurations.Domain.Model.ValueObjects;
using BuildTruckConfigurationService.Configurations.Interfaces.REST.Resources;
using BuildTruckConfigurationService.Configurations.Interfaces.REST.Transform;
using Xunit;

namespace BuildTruckBackend.Tests;

public class ConfigurationsTests
{
    [Fact]
    public void TutorialProgress_MarkCompleted_PreservesImmutableBehavior()
    {
        var original = new TutorialProgress();

        var updated = original.MarkCompleted("admin");

        Assert.False(original.IsCompleted("admin"));
        Assert.True(updated.IsCompleted("admin"));
    }

    [Fact]
    public void TutorialProgress_RejectsUnknownTutorials()
    {
        Assert.Throws<ArgumentException>(() =>
            new TutorialProgress("{\"unknown\":true}"));
    }

    [Fact]
    public void CreateAssembler_MapsExistingRestContract()
    {
        var resource = new CreateConfigurationSettingsResource
        {
            UserId = 7,
            Theme = "Dark",
            Plan = "Business",
            NotificationsEnable = "true",
            EmailNotifications = "false",
            TutorialsCompleted = "{\"manager\":true}"
        };

        var command = CreateConfigurationSettingsCommandFromResourceAssembler
            .ToCommandFromResource(resource);

        Assert.Equal(7, command.UserIds);
        Assert.Equal(Theme.Dark, command.Themes);
        Assert.Equal(Plan.Business, command.Plans);
        Assert.True(command.NotificationsEnables);
        Assert.False(command.EmailNotification);
        Assert.True(command.TutorialsCompleted.IsCompleted("manager"));
    }

    [Fact]
    public void ResourceAssembler_PreservesStringBooleanContract()
    {
        var settings = new ConfigurationSettings(
            7,
            Theme.Auto,
            Plan.Enterprise,
            true,
            false,
            new TutorialProgress("{\"admin\":true}"))
        {
            Id = 3
        };

        var resource = ConfigurationSettingsResourceFromEntityAssembler
            .ToResourceFromEntity(settings);

        Assert.Equal("true", resource.NotificationsEnables);
        Assert.Equal("false", resource.EmailNotification);
        Assert.Equal("{\"admin\":true}", resource.TutorialsCompleted);
    }
}
