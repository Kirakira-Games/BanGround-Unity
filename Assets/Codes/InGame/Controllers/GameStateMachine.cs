using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Layout: A stack of states. The base must be Loading, Playing, or Finished.
/// </summary>
public class GameStateMachine : IGameStateMachine
{
    public enum State { Loading, Paused, Playing, Rewinding, Finished }
    private LinkedList<State> stateStack;

    /// <summary>
    /// Current state.
    /// </summary>
    public State Current => stateStack.Last.Value;
    public State Base => stateStack.First.Value;

    public bool isPaused => Current == State.Paused;
    public bool isRewinding => HasState(State.Rewinding);
    public bool inSimpleState => Count == 1;
    public int Count => stateStack.Count;

    public bool HasState(State state)
    {
        return stateStack.Any(s => s == state);
    }

    public GameStateMachine()
    {
        stateStack = new LinkedList<State>();
        stateStack.AddLast(State.Loading);
    }

    public bool AddState(State state)
    {
        if (Current == state)
        {
            return false;
        }
        stateStack.AddLast(state);
        return true;
    }

    public void Transit(State prev, State next)
    {
        Debug.Assert(Current == prev);
        Debug.Assert(stateStack.Count == 1);
        stateStack.Last.Value = next;
    }

    public void PopState(State state)
    {
        Debug.Assert(state == Current);
        stateStack.RemoveLast();
    }

    public override string ToString()
    {
        return string.Join(",", stateStack.Select(i => i.ToString()).ToArray());
    }
}
