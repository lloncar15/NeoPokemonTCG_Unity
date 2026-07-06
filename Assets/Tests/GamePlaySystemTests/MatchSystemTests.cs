using GimGim.ActionSystem;
using GimGim.AspectContainer;
using GimGim.EventSystem;
using GimGim.GameplaySystems;
using GimGim.Model;
using NUnit.Framework;

public class MatchSystemTests {
    private IContainer _game;
    private ActionSystem _actionSystem;
    private GameDataSystem _gameDataSystem;
    private MatchSystem _matchSystem;

    [SetUp]
    public void SetUp() {
        typeof(NotificationEventSystem)
            .GetField("_instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.SetValue(null, null);

        _game = new Container();
        _actionSystem = _game.AddAspect<ActionSystem>();
        _gameDataSystem = _game.AddAspect<GameDataSystem>();
        _matchSystem = _game.AddAspect<MatchSystem>();

        _gameDataSystem.Awake();
        _matchSystem.Awake();

        ActionSystem.OrderOfPlayCounter.Reset();
    }

    [TearDown]
    public void TearDown() {
        _matchSystem.Destroy();
        _gameDataSystem.Destroy();
    }

    private void SimulateUpdate(int maxFrames = 1000) {
        int frameCounter = 0;
        while (_actionSystem.IsActive && frameCounter < maxFrames) {
            frameCounter++;
            _actionSystem.Update();
        }
    }

    [Test]
    public void ChangeTurnFlipsCurrentPlayer() {
        Match match = _game.GetMatch();
        Assert.AreEqual(0, match.CurrentPlayerIndex, "Match should start with player 0");

        _matchSystem.ChangeTurn();
        SimulateUpdate();

        Assert.AreEqual(1, match.CurrentPlayerIndex, "Turn should have passed to player 1");
        Assert.IsFalse(_actionSystem.IsActive, "Action system should be idle after the action resolves");
    }

    [Test]
    public void ChangeTurnTwiceReturnsToFirstPlayer() {
        Match match = _game.GetMatch();

        _matchSystem.ChangeTurn();
        SimulateUpdate();
        _matchSystem.ChangeTurn();
        SimulateUpdate();

        Assert.AreEqual(0, match.CurrentPlayerIndex, "Turn should have returned to player 0");
    }

    [Test]
    public void ChangeTurnToSpecificPlayerIndex() {
        Match match = _game.GetMatch();

        _matchSystem.ChangeTurn(1);
        SimulateUpdate();

        Assert.AreEqual(1, match.CurrentPlayerIndex, "Turn should have been given to the requested player");
        Assert.AreNotSame(match.CurrentPlayer, match.OpponentPlayer, "Current and opponent player should differ");
    }
}
