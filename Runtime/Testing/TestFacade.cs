using System;
using System.Collections.Generic;
using System.Linq;
using PureMVC.Interfaces;
using PureMVC.Patterns.Facade;

namespace PureMVC.Testing
{
    /// <summary>
    /// A <c>Facade</c> subclass that records every <c>INotification</c> dispatched through it,
    /// for use in tests that assert on notification routing without wiring up real observers.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Each instance uses its own Multiton key (a fresh <c>Guid</c> by default), so
    ///         parallel or sequential test instances never collide with each other or with a
    ///         production Facade's Core. Dispose the instance (or call <c>RemoveCore</c> on its
    ///         key directly) when the test is done to release that Core.
    ///     </para>
    /// </remarks>
    public sealed class TestFacade : Facade, IDisposable
    {
        private readonly string key;
        private readonly List<INotification> sentNotifications = new List<INotification>();

        /// <summary>All notifications dispatched through this Facade, in dispatch order.</summary>
        public IReadOnlyList<INotification> SentNotifications => sentNotifications;

        /// <summary>The Multiton key backing this instance's Core.</summary>
        public string Key => key;

        /// <summary>
        /// Creates a <c>TestFacade</c> with its own unique Multiton key.
        /// </summary>
        public TestFacade() : this(Guid.NewGuid().ToString())
        {
        }

        /// <summary>
        /// Creates a <c>TestFacade</c> using the given Multiton key. Use this overload only
        /// when the test needs a specific, known key; otherwise prefer the parameterless
        /// constructor so parallel test instances can't collide.
        /// </summary>
        /// <param name="key">Multiton key for this instance's Core.</param>
        public TestFacade(string key) : base(key)
        {
            this.key = key;
        }

        /// <inheritdoc/>
        public override void NotifyObservers(INotification notification)
        {
            sentNotifications.Add(notification);
            base.NotifyObservers(notification);
        }

        /// <summary>Whether a notification with the given name was dispatched.</summary>
        /// <param name="notificationName">The notification name to look for.</param>
        public bool WasSent(string notificationName) =>
            sentNotifications.Any(n => n.Name == notificationName);

        /// <summary>
        /// The first dispatched notification with the given name, or <c>null</c> if none was sent.
        /// </summary>
        /// <param name="notificationName">The notification name to look for.</param>
        public INotification? First(string notificationName) =>
            sentNotifications.FirstOrDefault(n => n.Name == notificationName);

        /// <summary>
        /// The body of the first dispatched notification with the given name, cast to
        /// <typeparamref name="T"/>, or <c>null</c> if no such notification was sent.
        /// </summary>
        /// <param name="notificationName">The notification name to look for.</param>
        public T? GetBody<T>(string notificationName) where T : class =>
            First(notificationName)?.Body as T;

        /// <summary>Removes this instance's Core (Model, View, Controller and Facade for its key).</summary>
        public void Dispose() => RemoveCore(key);
    }
}
