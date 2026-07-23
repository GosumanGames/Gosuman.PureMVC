using System.Linq;
using PureMVC.Patterns.Facade;
using PureMVC.Testing;

namespace PureMVCTests.Testing;

[TestClass]
public class TestFacadeTests
{
    [TestMethod]
    public void WasSent_ReflectsDispatchedNotifications()
    {
        // Arrange
        using var facade = new TestFacade();

        // Act
        facade.SendNotification("SomeNotification");

        // Assert
        facade.WasSent("SomeNotification").Should().BeTrue();
        facade.WasSent("OtherNotification").Should().BeFalse();
    }

    [TestMethod]
    public void GetBody_ReturnsTheNotificationBody()
    {
        // Arrange
        using var facade = new TestFacade();

        // Act
        facade.SendNotification("SomeNotification", "the body");

        // Assert
        facade.GetBody<string>("SomeNotification").Should().Be("the body");
    }

    [TestMethod]
    public void GetBody_ReturnsNull_WhenNotificationWasNotSent()
    {
        // Arrange
        using var facade = new TestFacade();

        // Act / Assert
        facade.GetBody<string>("NeverSent").Should().BeNull();
    }

    [TestMethod]
    public void First_ReturnsTheEarliestMatchingNotification()
    {
        // Arrange
        using var facade = new TestFacade();

        // Act
        facade.SendNotification("Repeated", "first");
        facade.SendNotification("Repeated", "second");

        // Assert
        var first = facade.First("Repeated");
        first.Should().NotBeNull();
        first!.Body.Should().Be("first");
    }

    [TestMethod]
    public void SentNotifications_RecordsEveryDispatchInOrder()
    {
        // Arrange
        using var facade = new TestFacade();

        // Act
        facade.SendNotification("First");
        facade.SendNotification("Second");

        // Assert
        facade.SentNotifications.Select(n => n.Name).Should().Equal("First", "Second");
    }

    [TestMethod]
    public void TwoInstances_DoNotCollide()
    {
        // Arrange
        using var facadeA = new TestFacade();
        using var facadeB = new TestFacade();

        // Act
        facadeA.SendNotification("OnlyOnA");

        // Assert
        facadeA.WasSent("OnlyOnA").Should().BeTrue();
        facadeB.WasSent("OnlyOnA").Should().BeFalse();
    }

    [TestMethod]
    public void Dispose_RemovesTheCore()
    {
        // Arrange
        var facade = new TestFacade();
        var coreKey = facade.Key;
        Facade.HasCore(coreKey).Should().BeTrue();

        // Act
        facade.Dispose();

        // Assert
        Facade.HasCore(coreKey).Should().BeFalse();
    }
}
