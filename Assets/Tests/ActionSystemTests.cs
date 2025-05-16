using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GimGim.ActionSystem;
using GimGim.AspectContainer;
using GimGim.EventSystem;
using NUnit.Framework;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.TestTools;

public class ActionSystemTests {
    public class TestAction : GameAction {
        public bool HasPrepared;
        public bool HasPerformed;
    }

    public class TestPostResolution : PostResolutionEvent {
        public TestPostResolution(object sender, bool repeats) : base(sender, repeats) {
        }
    }
    
    public struct TestFlags {
        public bool HasFlowStarted;
        public bool HasFlowCompleted;
        public bool HasPrepared;
        public bool HasPerformed;
        public bool HasCompleted;
        public bool HasPostResolution;
    }

    private class TestSystem : Aspect {
        public const int ROOT_ACTION_ORDER = 0;
        public const int DEPTH_CHECK_PRIORITY = 1;
        public const int DEPTH_REACTION_ORDER = int.MinValue;

        public TestFlags ActionFlags = new();
        public TestFlags ReactionFlags = new();
        public List<TestAction> Reactions = new();
        public bool HasLoopedPostResolution;
        public bool HasSortedFiFo;
        
        List<IEventSubscription> _eventSubscriptions = new();
        public void OnEnable() {
            _eventSubscriptions.Add(new EventSubscription<GameActionFlowStartedEvent>(OnFlowStarted));
            _eventSubscriptions.Add(new EventSubscription<GameActionFlowCompletedEvent>(OnFlowCompleted));
            _eventSubscriptions.Add(new EventSubscription<GameActionPreparedEvent>(OnActionPrepared));
            _eventSubscriptions.Add(new EventSubscription<GameActionPerformedEvent>(OnActionPerformed));
            _eventSubscriptions.Add(new EventSubscription<GameActionCompletedEvent>(OnGameActionCompleted));
            _eventSubscriptions.Add(new EventSubscription<TestPostResolution>(OnPostResolution));
            
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
        
        public void OnFlowStarted(GameActionFlowStartedEvent eventData) {
            IGameAction action = eventData.Action;
            
            TestFlags flags = action.OrderOfPlay == ROOT_ACTION_ORDER ? ActionFlags : ReactionFlags;
            flags.HasFlowStarted = true;

            action.GetPhase(GameActionPhaseType.Prepare).Viewer = TestViewer;
            action.GetPhase(GameActionPhaseType.Perform).Viewer = TestViewer;
        }
        
        public void OnFlowCompleted(GameActionFlowCompletedEvent eventData) {
            IGameAction action = eventData.Action;
            TestFlags flags = action.OrderOfPlay == ROOT_ACTION_ORDER ? ActionFlags : ReactionFlags;
            flags.HasFlowCompleted = true;
        }
        
        private void OnActionPrepared(GameActionPreparedEvent eventData) {
            TestAction action = eventData.Action as TestAction;
            TestFlags flags = action.OrderOfPlay == ROOT_ACTION_ORDER ? ActionFlags : ReactionFlags;
            flags.HasPrepared = true;
            action.HasPrepared = true;
        }
        
        private void OnActionPerformed(GameActionPerformedEvent eventData) {
            TestAction action = eventData.Action as TestAction;
            TestFlags flags = action.OrderOfPlay == ROOT_ACTION_ORDER ? ActionFlags : ReactionFlags;
            flags.HasPerformed = true;
            action.HasPerformed = true;

            if (action.OrderOfPlay != ROOT_ACTION_ORDER) {
                Reactions.Add(action);
            }
            else {
                AddReactions((IContainer)eventData.Sender);
            }

            if (action.Priority == DEPTH_CHECK_PRIORITY) {
                TestAction reaction = new TestAction();
                reaction.OrderOfPlay = DEPTH_REACTION_ORDER;
                ((IContainer)action.Sender).GetAspect<ActionSystem>().AddReaction(reaction);
            }
            if (action.OrderOfPlay == DEPTH_REACTION_ORDER) {
                HasSortedFiFo = Reactions.Count == 2;
            }
        }
        
        private void OnGameActionCompleted(GameActionCompletedEvent eventData) {
            ActionFlags.HasCompleted = true;
        }
        
        private void OnPostResolution(TestPostResolution eventData) {
            TestAction action = eventData.Action as TestAction;
            TestFlags flags = action.OrderOfPlay == ROOT_ACTION_ORDER ? ActionFlags : ReactionFlags;

            if (flags.HasPostResolution == false) {
                TestAction reaction = new TestAction();
                reaction.OrderOfPlay = int.MaxValue;
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

    public void SimulateUpdate(int frames = 1000) {
        int frameCounter = 0;
        while (_actionSystem.IsActive && frameCounter < frames) {
            frameCounter++;
            _actionSystem.Update();
        }
    }

    public void AssertFlags(TestFlags expected, TestFlags flags) {
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
    public void TestActionNotifications() {
        _actionSystem.PerformGameAction(new TestAction());
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
