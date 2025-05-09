using System.Collections.Generic;
using System.Linq;
using GimGim.NotificationEventSystem;
using NUnit.Framework;

public class NotificationEventSystemTest 
{
    #region Dummy Events
    private interface ICombatEvent : IEvent {
        public UnitId Id { get; }
    }
    private interface IOtherEvent : IEvent {}

    private class EnemyDiedEvent : EventData, ICombatEvent {
        public UnitId Id { get; }

        public EnemyDiedEvent(object sender, UnitId enemyId) : base(sender) {
            Id = enemyId;
        }
    }

    private class PlayerEvent : EventData {
        public UnitId Id { get; }
        
        public PlayerEvent(object sender, UnitId playerId) : base(sender) {
            Id = playerId;
        }
    }

    private class OtherEvent : EventData, IOtherEvent, ICombatEvent {
        public UnitId Id { get; }
        public string Other { get; set; }

        public OtherEvent(object sender, UnitId unitId, string other) : base(sender) {
            Id = unitId;
            Other = other;
        }
    }
    
    private enum UnitId {
        Player = 1,
        Enemy = 2,
        Combat = 3,
        Other = 4
    }
    #endregion

    /// <summary>
    /// Dummy class to represent an object that subscribes and listens to events.
    /// </summary>
    private class CombatListener {
        public bool CombatEvent;
        public bool AnyEvent;
        public string String;
        public List<UnitId> EventPriorityList = new();

        private IEventSubscription _subscriptionCombat;
        private IEventSubscription _subscriptionCombatNegative;
        private IEventSubscription _subscriptionAny;
        
        public void SubscribeCombat(bool firesOnce = false, int priority = 0) {
            _subscriptionCombat = NotificationEventSystem.Subscribe(new EventSubscription<ICombatEvent>(OnCombatEvent, firesOnce, priority));
        }
        
        public void SubscribeCombatWithNegativePriority() {
            _subscriptionCombatNegative = NotificationEventSystem.Subscribe(new EventSubscription<ICombatEvent>(OnCombatNegativeEvent, false, -1));
        }
        
        public void SubscribeAny(int priority = 0) {
            _subscriptionAny = NotificationEventSystem.Subscribe(new EventSubscription<OtherEvent>(OnOtherEvent, priority));
        }

        public void Unsubscribe() {
            if (_subscriptionCombat != null) NotificationEventSystem.Unsubscribe(_subscriptionCombat);
            if (_subscriptionAny != null) NotificationEventSystem.Unsubscribe(_subscriptionAny);
            if (_subscriptionCombatNegative != null) NotificationEventSystem.Unsubscribe(_subscriptionCombatNegative);
        }

        public void OnCombatEvent(ICombatEvent otherEvent) {
            CombatEvent = true;
            EventPriorityList.Add(UnitId.Combat);
        }
        
        public void OnCombatNegativeEvent(ICombatEvent otherEvent) {
            CombatEvent = true;
            EventPriorityList.Add(UnitId.Other);
        }

        public void OnOtherEvent(OtherEvent otherEvent) {
            AnyEvent = true;
            String = otherEvent.Other;
        }
    }
    
    private CombatListener _listener;

    /// <summary>
    /// Sets up the test environment by resetting the NotificationEventSystem instance and initializing a listener.
    /// </summary>
    [SetUp]
    public void SetUp() {
        _listener = new CombatListener();
        
        typeof(NotificationEventSystem)
            .GetField("_instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.SetValue(null, null);
    }
    
    /// <summary>
    /// Tests that a listener can subscribe to an event and receive it when posted.
    /// </summary>
    [Test]
    public void TestSubscriptionToSystem() {
        _listener.SubscribeAny();
        NotificationEventSystem.PostEvent(new OtherEvent(this, UnitId.Player, "Test"));
        NotificationEventSystem.Instance.Flush();
        
        Assert.IsTrue(_listener.AnyEvent, "Listener should have received the event");
        Assert.IsNotNull(_listener.String, "Listener should have received the event with the correct string");
    }

    /// <summary>
    /// Tests that a listener can unsubscribe from an event and no longer receive it.
    /// </summary>
    [Test]
    public void TestUnsubscriptionFromSystem() {
        _listener.SubscribeAny();
        _listener.Unsubscribe();
        NotificationEventSystem.PostEvent(new OtherEvent(this, UnitId.Player, "Test"));
        NotificationEventSystem.Instance.Flush();
        
        Assert.IsFalse(_listener.AnyEvent, "Listener should not have received the event");
    }
    
    /// <summary>
    /// Tests that a listener can subscribe to a specific combat event and receive it.
    /// </summary>
    [Test]
    public void TestSubscriptionToCombatEvent() {
        _listener.SubscribeCombat();
        NotificationEventSystem.PostEvent(new EnemyDiedEvent(this, UnitId.Enemy));
        NotificationEventSystem.Instance.Flush();
        
        Assert.IsTrue(_listener.CombatEvent, "Listener should have received the event");
    }
    
    /// <summary>
    /// Tests that a listener subscribed to one event does not receive unrelated events.
    /// </summary>
    [Test]
    public void TestSubscriptionToOneButFiringOtherEvent() {
        _listener.SubscribeCombat();
        NotificationEventSystem.PostEvent(new PlayerEvent(this, UnitId.Player));
        NotificationEventSystem.Instance.Flush();
        
        Assert.IsFalse(_listener.CombatEvent, "Listener should not have received the event");
    }

    /// <summary>
    /// Tests that a listener subscribed to a combat event receives it when another event type is posted.
    /// </summary>
    [Test]
    public void TestCombatEventFiringOnOtherEvent() {
        _listener.SubscribeCombat();
        NotificationEventSystem.PostEvent(new OtherEvent(this, UnitId.Enemy, "Test"));
        NotificationEventSystem.Instance.Flush();
        
        Assert.IsTrue(_listener.CombatEvent, "Listener should have received the event");
    }
    
    /// <summary>
    /// Tests that a one-time subscription is removed after the first event dispatch.
    /// </summary>
    [Test]
    public void TestSubscriptionToOneOffEvent() {
        _listener.SubscribeCombat(true);
        NotificationEventSystem.PostEvent(new EnemyDiedEvent(this, UnitId.Enemy));
        NotificationEventSystem.Instance.Flush();
        
        Assert.IsTrue(_listener.CombatEvent, "Listener should have received the event");
        _listener.CombatEvent = false;
        
        NotificationEventSystem.PostEvent(new EnemyDiedEvent(this, UnitId.Enemy));
        NotificationEventSystem.Instance.Flush();
        
        Assert.IsFalse(_listener.CombatEvent, "Listener should not have received the event");
    }

	/// <summary>
    /// Tests that a listener can receive the same event multiple times.
    /// </summary>
    [Test]
    public void TestListenerListensTwice() {
        _listener.SubscribeCombat();
        NotificationEventSystem.PostEvent(new EnemyDiedEvent(this, UnitId.Enemy));
        NotificationEventSystem.Instance.Flush();
        
        Assert.IsTrue(_listener.CombatEvent, "Listener should have received the event");
        _listener.CombatEvent = false;

        NotificationEventSystem.PostEvent(new EnemyDiedEvent(this, UnitId.Enemy));
        NotificationEventSystem.Instance.Flush();
        
        Assert.IsTrue(_listener.CombatEvent, "Listener should have received the event");
    }
	
    /// <summary>
    /// Tests that subscriptions are processed in the order of their priority.
    /// </summary>
	[Test]
	public void TestSubscriptionPriority() {
        _listener.SubscribeCombatWithNegativePriority();
        _listener.SubscribeCombat(false, 3);
        NotificationEventSystem.PostEvent(new EnemyDiedEvent(this, UnitId.Enemy));
        NotificationEventSystem.Instance.Flush();
        
        Assert.AreEqual(UnitId.Combat, _listener.EventPriorityList.First(), "Listener should have received the event with the highest priority first");
        
        _listener.Unsubscribe();
        _listener.EventPriorityList.Clear();
        
        _listener.SubscribeCombatWithNegativePriority();
        _listener.SubscribeCombat(false, -3);
        NotificationEventSystem.PostEvent(new EnemyDiedEvent(this, UnitId.Enemy));
        NotificationEventSystem.Instance.Flush();
        
        Assert.AreEqual(UnitId.Other, _listener.EventPriorityList.First(), "Listener should have received the events in order of subscription");
	}
}
