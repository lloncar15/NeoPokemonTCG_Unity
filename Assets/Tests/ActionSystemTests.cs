using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GimGim.ActionSystem;
using GimGim.AspectContainer;
using GimGim.EventSystem;
using NUnit.Framework;

public class ActionSystemTests {
    private class TestAction : GameAction {
        public bool HasPrepared;
        public bool HasPerformed;
        public int ReactionDepth;
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
        // Constants for action ordering and priorities
        private const int ROOT_ACTION_ORDER = 0;
        private const int HIGH_PRIORITY = 10;
        private const int LOW_PRIORITY = -10;
        private const int NORMAL_PRIORITY = 0;
        private const int SLOW_VIEWER_FRAMES = 3;
        
        // Test configuration flags
        public bool UseViewer = true;
        public bool UseSlowViewer;
        public bool TriggerReactionChain;
        public int ReactionChainDepth;
        public bool AddPriorityReactions;
        public bool AddSamePriorityReactions;
        public bool SkipPostResolutionReactions;

        // Test state tracking
        public TestFlags ActionFlags = new();
        public readonly TestFlags ReactionFlags = new();
        public readonly List<TestAction> Reactions = new();
        public int MaxReactionDepth;
        public bool HasLoopedPostResolution;
        public bool PostResolutionReactionCreated;
        
        private int _currentSlowViewerFrame;
        private readonly List<IEventSubscription> _eventSubscriptions = new();
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
            
            TestFlags flags = action.OrderOfPlay == ROOT_ACTION_ORDER ? ref ActionFlags : ref ReactionFlags;
            flags.HasFlowStarted = true;

            if (!UseViewer) return;
            
            action.GetPhase(GameActionPhaseType.Prepare).Viewer = TestViewer;
            action.GetPhase(GameActionPhaseType.Perform).Viewer = TestViewer;
        }
        
        private void OnFlowCompleted(GameActionFlowCompletedEvent eventData) {
            IGameAction action = eventData.Action;
            
            TestFlags flags = action.OrderOfPlay == ROOT_ACTION_ORDER ? ref ActionFlags : ref ReactionFlags;
            flags.HasFlowCompleted = true;
        }
        
        private void OnActionPrepared(GameActionPreparedEvent eventData) {
            if (eventData.Action is not TestAction action) return;
            
            TestFlags flags = action.OrderOfPlay == ROOT_ACTION_ORDER ? ref ActionFlags : ref ReactionFlags;
            flags.HasPrepared = true;
            action.HasPrepared = true;
        }
        
        private void OnActionPerformed(GameActionPerformedEvent eventData) {
            if (eventData.Action is not TestAction action) return;
            
            TestFlags flags = action.OrderOfPlay == ROOT_ACTION_ORDER ? ref ActionFlags : ref ReactionFlags;
            flags.HasPerformed = true;
            action.HasPerformed = true;

            if (action.OrderOfPlay != ROOT_ACTION_ORDER) {
                Reactions.Add(action);
                MaxReactionDepth = Math.Max(MaxReactionDepth, action.ReactionDepth);
                
                if (TriggerReactionChain && action.ReactionDepth < ReactionChainDepth) {
                    TestAction reaction = new() {
                        Sender = action.Sender,
                        ReactionDepth = action.ReactionDepth + 1,
                    };
                    ((IContainer)action.Sender).GetAspect<ActionSystem>().AddReaction(reaction);
                }
            }
            else {
                AddReactions((IContainer)eventData.Sender);
            }
        }
        
        private void OnGameActionCompleted(GameActionCompletedEvent eventData) {
            ActionFlags.HasCompleted = true;
        }
        
        private void OnPostResolutionEvent(TestPostResolutionEvent eventData) {
            if (eventData.Action is not TestAction action) return;

            TestFlags flags = action.OrderOfPlay == ROOT_ACTION_ORDER ? ref ActionFlags : ref ReactionFlags;

            if (!flags.HasPostResolution && !SkipPostResolutionReactions) {
                TestAction reaction = new() {
                    Sender = action.Sender
                };
                ((IContainer)action.Sender).GetAspect<ActionSystem>().AddReaction(reaction);
                PostResolutionReactionCreated = true;
            }
            else {
                HasLoopedPostResolution = true;
            }
            
            flags.HasPostResolution = true;
        }

        IEnumerator TestViewer(IContainer game, GameAction action) {
            if (UseSlowViewer) {
                _currentSlowViewerFrame = 0;
                while (_currentSlowViewerFrame < SLOW_VIEWER_FRAMES) {
                    _currentSlowViewerFrame++;
                    yield return null;
                }
                yield return true;
            }
            else {
                yield return null;
                yield return true;
                yield return null;
            }
        }

        void AddReactions(IContainer game) {
            if (AddPriorityReactions) {
                // Add reactions with different priorities
                int[] priorities = { NORMAL_PRIORITY, HIGH_PRIORITY, LOW_PRIORITY, NORMAL_PRIORITY, HIGH_PRIORITY };
                foreach (int prio in priorities) {
                    TestAction reaction = new() {
                        Sender = game,
                        Priority = prio
                    };
                    game.GetAspect<ActionSystem>().AddReaction(reaction);
                }
            }
            else {
                for (int i = 0; i < 5; ++i) {
                    TestAction reaction = new() {
                        Sender = game
                    };
                    game.GetAspect<ActionSystem>().AddReaction(reaction);
                }
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
    }
    
    [TearDown]
    public void TearDown() {
        _testSystem.OnDisable();
    }

    [Test]
    public void TestActionSystemTracksActiveState() {
        TestAction action = new() {
            Sender = _game
        };
        
        _actionSystem.PerformGameAction(action);
        Assert.IsTrue(_actionSystem.IsActive);
        SimulateUpdate();
        Assert.IsFalse(_actionSystem.IsActive);
    }

    [Test]
    public void TestActionNotificationsWithoutViewers() {
        TestAction action = new() {
            Sender = _game
        };
        
        TestPostResolutionEvent postResEvent = new(_game, false);
        _actionSystem.RegisterPostResolutionEvent(postResEvent);

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
        
        TestPostResolutionEvent postResEvent = new(_game, false);
        _actionSystem.RegisterPostResolutionEvent(postResEvent);

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
    public void TestReactionChain() {
        TestAction action = new() {
            Sender = _game
        };

        _testSystem.TriggerReactionChain = true;
        _testSystem.ReactionChainDepth = 3;
        _testSystem.UseViewer = false;
        
        _actionSystem.PerformGameAction(action);
        SimulateUpdate();
        
        // Verify the reaction chain
        Assert.AreEqual(3, _testSystem.MaxReactionDepth, "Should have 3 levels of reaction depth");
        Assert.IsTrue(_testSystem.ReactionFlags.HasPrepared, "Reactions should have prepared");
        Assert.IsTrue(_testSystem.ReactionFlags.HasPerformed, "Reactions should have performed");
    
        // Verify reaction order - the deepest reaction should be the last one processed
        Assert.AreEqual(3, _testSystem.Reactions.Last().ReactionDepth, 
            "The last reaction processed should be the deepest one");
    }

    [Test]
    public void TestPriorityOrder() {
        TestAction action = new() {
            Sender = _game
        };
        
        _testSystem.AddPriorityReactions = true;
        _testSystem.UseViewer = false;
        _testSystem.SkipPostResolutionReactions = true;
        
        _actionSystem.PerformGameAction(action);
        SimulateUpdate();
        
        List<int> priorities = _testSystem.Reactions.Select(r => r.Priority).ToList();
        
        // Verify that the priorities are in descending order
        for (int i = 0; i < priorities.Count - 1; i++) {
            Assert.IsTrue(priorities[i] >= priorities[i+1], 
                $"Reaction at index {i} with priority {priorities[i]} should be processed before " +
                $"reaction at index {i+1} with priority {priorities[i+1]}");
        }
    }

    [Test]
    public void TestPostResolutionEvents() {
        TestAction action = new() {
            Sender = _game
        };
        
        TestPostResolutionEvent postResEvent = new(_game, false);
        _actionSystem.RegisterPostResolutionEvent(postResEvent);
        
        _actionSystem.PerformGameAction(action);
        SimulateUpdate();
        
        // Verify post-resolution was triggered and created a reaction
        Assert.IsTrue(_testSystem.ActionFlags.HasPostResolution, "Post-resolution should have been triggered");
        Assert.IsTrue(_testSystem.PostResolutionReactionCreated, "Post-resolution should have created a reaction");
    }

    [Test]
    public void TestRepeatingPostResolutionEvents() {
        TestAction action = new() {
            Sender = _game
        };
        
        TestPostResolutionEvent postResEvent = new(_game, true);
        _actionSystem.RegisterPostResolutionEvent(postResEvent);
        
        _actionSystem.PerformGameAction(action);
        SimulateUpdate();
        
        Assert.IsTrue(_testSystem.HasLoopedPostResolution, "Post-resolution should have looped");
    }
    
    [Test]
    public void TestMultipleActionsInSequence() {
        // Create first test action
        TestAction action1 = new() {
            Sender = _game
        };

        // Perform first action
        _actionSystem.PerformGameAction(action1);
        SimulateUpdate();
    
        // Verify the first action completed
        Assert.IsTrue(action1.HasPerformed, "First action should have performed");
        Assert.IsTrue(_testSystem.ActionFlags.HasCompleted, "First action should be completed");
        Assert.IsFalse(_actionSystem.IsActive, "Action system should not be active after completion");
    
        // Reset flags
        _testSystem.ActionFlags = new TestFlags();
    
        // Create second test action
        TestAction action2 = new() {
            Sender = _game
        };

        // Perform second action
        _actionSystem.PerformGameAction(action2);
        SimulateUpdate();
    
        // Verify the second action completed
        Assert.IsTrue(action2.HasPerformed, "Second action should have performed");
        Assert.IsTrue(_testSystem.ActionFlags.HasCompleted, "Second action should be completed");
    }
}
