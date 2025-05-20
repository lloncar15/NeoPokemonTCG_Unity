using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GimGim.ActionSystem;
using GimGim.AspectContainer;
using GimGim.EventSystem;
using GimGim.Utility.Logger;
using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.TestTools;

public class ActionSystemTests {
    private class TestAction : GameAction {
        public bool HasPrepared;
        public bool HasPerformed;
    }

    private class TestPostResolutionEvent : PostResolutionEvent {
        public TestPostResolutionEvent(object sender, bool repeats) : base(sender, repeats) {
        }
    }

    private class TestFlags {
        public bool HasFlowStarted;
        public bool HasPrepared;
        public bool HasPerformed;
        public bool HasPostResolution;
        public bool HasFlowCompleted;
        public bool HasCompleted;
    }

    private class TestSystem : Aspect {
        public const int RootActionOrder = 0;
        public const int DepthCheckPriority = 1;
        public const int DepthReactionOrder = int.MinValue;

        public TestFlags ActionFlags = new();
        public TestFlags ReactionFlags = new();
        public readonly List<TestAction> Reactions = new();
        public bool HasLoopedPostResolution;
        public bool HasSortedFiFo;

        public bool UseViewer = true;

        private List<IEventSubscription> _eventSubscriptions = new();
        public void OnEnable() {
            _eventSubscriptions.Add(new EventSubscription<GameActionFlowStartedEvent>(OnFlowStarted));
            _eventSubscriptions.Add(new EventSubscription<GameActionFlowCompletedEvent>(OnFlowCompleted));
            _eventSubscriptions.Add(new EventSubscription<GameActionPreparedEvent>(OnActionPrepared));
            _eventSubscriptions.Add(new EventSubscription<GameActionPerformedEvent>(OnActionPerformed));
            _eventSubscriptions.Add(new EventSubscription<GameActionCompletedEvent>(OnGameActionCompleted));
            _eventSubscriptions.Add(new EventSubscription<TestPostResolutionEvent>(OnPostResolutionEvent));
            
            foreach (IEventSubscription subscription in _eventSubscriptions) {
                NotificationEventSystem.Subscribe(subscription);
            }
        }

        public void OnDisable() {
            foreach (IEventSubscription subscription in _eventSubscriptions) {
                NotificationEventSystem.Unsubscribe(subscription);
            }
            _eventSubscriptions.Clear();
        }

        private void OnFlowStarted(GameActionFlowStartedEvent eventData) {
            IGameAction action = eventData.Action;
            
            TestFlags flags = action.OrderOfPlay == RootActionOrder ? ref ActionFlags : ref ReactionFlags;
            flags.HasFlowStarted = true;

            if (!UseViewer) return;
            
            action.GetPhase(GameActionPhaseType.Prepare).Viewer = TestViewer;
            action.GetPhase(GameActionPhaseType.Perform).Viewer = TestViewer;
        }
        
        private void OnFlowCompleted(GameActionFlowCompletedEvent eventData) {
            IGameAction action = eventData.Action;
            
            TestFlags flags = action.OrderOfPlay == RootActionOrder ? ref ActionFlags : ref ReactionFlags;
            flags.HasFlowCompleted = true;
        }
        
        private void OnActionPrepared(GameActionPreparedEvent eventData) {
            if (eventData.Action is not TestAction action) return;
            
            TestFlags flags = action.OrderOfPlay == RootActionOrder ? ref ActionFlags : ref ReactionFlags;
            flags.HasPrepared = true;
            action.HasPrepared = true;
        }
        
        private void OnActionPerformed(GameActionPerformedEvent eventData) {
            if (eventData.Action is not TestAction action) return;
            
            TestFlags flags = action.OrderOfPlay == RootActionOrder ? ref ActionFlags : ref ReactionFlags;
            flags.HasPerformed = true;
            action.HasPerformed = true;

            if (action.OrderOfPlay != RootActionOrder) {
                Reactions.Add(action);
            }
            else {
                AddReactions((IContainer)eventData.Sender);
            }

            if (action.Priority == DepthCheckPriority) {
                TestAction reaction = new TestAction();
                reaction.OrderOfPlay = DepthReactionOrder;
                ((IContainer)action.Sender).GetAspect<ActionSystem>().AddReaction(reaction);
            }
            if (action.OrderOfPlay == DepthReactionOrder) {
                HasSortedFiFo = Reactions.Count == 2;
            }
        }
        
        private void OnGameActionCompleted(GameActionCompletedEvent eventData) {
            ActionFlags.HasCompleted = true;
        }
        
        private void OnPostResolutionEvent(TestPostResolutionEvent eventData) {
            if (eventData.Action is not TestAction action) return;

            TestFlags flags = action.OrderOfPlay == RootActionOrder ? ref ActionFlags : ref ReactionFlags;

            if (flags.HasPostResolution == false) {
                TestAction reaction = new TestAction {
                    OrderOfPlay = int.MaxValue
                };
                ((IContainer)action.Sender).GetAspect<ActionSystem>().AddReaction(reaction);
            }
            else {
                HasLoopedPostResolution = true;
            }
            
            flags.HasPostResolution = true;
        }

        IEnumerator TestViewer(IContainer game, GameAction action) {
            yield return null;
            yield return true;
            yield return null;
        }

        void AddReactions(IContainer game) {
            for (int i = 0; i < 5; ++i) {
                TestAction reaction = new TestAction();
                game.GetAspect<ActionSystem>().AddReaction(reaction);
            }
        }
    }

    private void SimulateUpdate(int frames = 1000) {
        int frameCounter = 0;
        while (_actionSystem.IsActive && frameCounter < frames) {
            frameCounter++;
            _actionSystem.Update();
        }
    }

    private static void AssertFlags(TestFlags expected, TestFlags flags) {
        Assert.AreEqual(expected.HasFlowStarted, flags.HasFlowStarted, "FlowStarted");
        Assert.AreEqual(expected.HasFlowCompleted, flags.HasFlowCompleted, "FlowCompleted");
        Assert.AreEqual(expected.HasPrepared, flags.HasPrepared, "Prepared");
        Assert.AreEqual(expected.HasPerformed, flags.HasPerformed, "Performed");
        Assert.AreEqual(expected.HasCompleted, flags.HasCompleted, "Completed");
        Assert.AreEqual(expected.HasPostResolution, flags.HasPostResolution, "PostResolution");
    }

    private IContainer _game;
    private ActionSystem _actionSystem;
    private TestSystem _testSystem;

    [SetUp]
    public void SetUp() {
        typeof(NotificationEventSystem)
            .GetField("_instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.SetValue(null, null);

        _game = new Container();
        _actionSystem = _game.AddAspect<ActionSystem>();
        _testSystem = _game.AddAspect<TestSystem>();
        _testSystem.OnEnable();
        
        ActionSystem.OrderOfPlayCounter.Reset();
        _actionSystem.RegisterPostResolutionEvent(new TestPostResolutionEvent(this, true));
    }
    
    [TearDown]
    public void TearDown() {
        _testSystem.OnDisable();
    }

    [Test]
    public void TestActionSystemTracksActiveState() {
        _actionSystem.PerformGameAction(new TestAction());
        Assert.IsTrue(_actionSystem.IsActive);
        SimulateUpdate();
        Assert.IsFalse(_actionSystem.IsActive);
    }

    [Test]
    public void TestActionNotificationsWithoutViewers() {
        TestAction action = new() {
            Sender = _game
        };

        _testSystem.UseViewer = false;
        
        _actionSystem.PerformGameAction(action);
        SimulateUpdate();
        TestFlags flags = _testSystem.ActionFlags;

        TestFlags expectedFlags = new TestFlags {
            HasFlowStarted = true,
            HasFlowCompleted = true,
            HasPrepared = true,
            HasPerformed = true,
            HasCompleted = true,
            HasPostResolution = true
        };
        AssertFlags(expectedFlags,flags);
    }

    [Test]
    public void TestActionNotifications() {
        TestAction action = new() {
            Sender = _game
        };

        _actionSystem.PerformGameAction(action);
        SimulateUpdate();
        TestFlags flags = _testSystem.ActionFlags;

        TestFlags expectedFlags = new TestFlags {
            HasFlowStarted = true,
            HasFlowCompleted = true,
            HasPrepared = true,
            HasPerformed = true,
            HasCompleted = true,
            HasPostResolution = true
        };
        AssertFlags(expectedFlags,flags);
    }
}
