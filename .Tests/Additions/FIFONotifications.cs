using PureMVC.Patterns.Command;
using PureMVC.Patterns.Facade;
using PureMVC.Patterns.Mediator;

namespace PureMVCTests.Additions;

/// <summary>
/// It is important that notifications are processed in the order they are received.
/// This means that if a new notification is sent while the previous one is still being processed, it should be queued and processed in order.
/// </summary>
[TestClass]
public class FIFONotifications
{
    private const string NOTIFICATION1 = "Notification1";
    private const string NOTIFICATION2 = "Notification2";

    private class TestMediator : Mediator
    {
        public TestMediator(string mediatorName, Action onNotification, string sendNotification) : base(mediatorName) 
        { 
            _onNotification = onNotification;
            _sendNotification = sendNotification;
        }

        private readonly Action _onNotification;
        private readonly string _sendNotification;

        override public string[] ListNotificationInterests()
        {
            return new[] { this.MediatorName };
        }

        public override void HandleNotification(INotification notification)
        {
            if (!string.IsNullOrWhiteSpace(_sendNotification))
            {
                SendNotification(_sendNotification);
            }
            _onNotification?.Invoke();
        }
    }

    private class TestCommand : SimpleCommand
    {
        public TestCommand(Action onNotification)
        {
            _onNotification = onNotification;
        }

        private readonly Action _onNotification;

        public override void Execute(INotification notification)
        {
            _onNotification?.Invoke();
        }
    }

    [TestMethod]
    public void NotificationsOnMediators()
    {
        // Arrange
        var facade = Facade.GetInstance("FIFOFacade", key => new Facade(key));
        var notificationQueue = new Queue<string>();
        facade.RegisterMediator(new TestMediator(NOTIFICATION1, () => notificationQueue.Enqueue("Mediator1"), NOTIFICATION2));
        facade.RegisterMediator(new TestMediator(NOTIFICATION2, () => notificationQueue.Enqueue("Mediator2"), string.Empty));
        facade.RegisterCommand(NOTIFICATION2, () => new TestCommand(() => notificationQueue.Enqueue("Command")));

        // Act
        facade.SendNotification("Notification1");
        // Assert
        notificationQueue.Count.Should().Be(3, "Both commands should have been executed in order.");
        notificationQueue.Dequeue().Should().Be("Mediator1", "First notification should be handled by Mediator1.");
        notificationQueue.Dequeue().Should().Be("Mediator2", "Second notification should be handled by Mediator2.");
        notificationQueue.Dequeue().Should().Be("Command", "Command should be executed after both Mediators have handled their notifications.");
    }
}
