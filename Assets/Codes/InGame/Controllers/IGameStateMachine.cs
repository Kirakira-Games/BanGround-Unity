public interface IGameStateMachine
{
    GameStateMachine.State Base { get; }
    int Count { get; }
    GameStateMachine.State Current { get; }
    bool inSimpleState { get; }
    bool isPaused { get; }
    bool isRewinding { get; }

    bool AddState(GameStateMachine.State state);
    bool HasState(GameStateMachine.State state);
    void PopState(GameStateMachine.State state);
    void Transit(GameStateMachine.State prev, GameStateMachine.State next);
}
