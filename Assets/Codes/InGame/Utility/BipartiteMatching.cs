using UnityEngine;
using System.Collections.Generic;

public class BipartiteMatching<LT, RT>
{
    private Dictionary<LT, HashSet<RT>> edges;
    private HashSet<LT> visited;
    public Dictionary<RT, LT> RtoL;
    public Dictionary<LT, RT> LtoR;

    public BipartiteMatching()
    {
        edges = new Dictionary<LT, HashSet<RT>>();
        visited = new HashSet<LT>();
        RtoL = new Dictionary<RT, LT>();
        LtoR = new Dictionary<LT, RT>();
    }

    public void Clear(bool clearEdges = true)
    {
        if (clearEdges)
            edges.Clear();
        RtoL.Clear();
        LtoR.Clear();
    }
    
    public void AddEdge(LT lhs, RT rhs)
    {
        if (!edges.ContainsKey(lhs))
        {
            edges.Add(lhs, new HashSet<RT>());
        }
        edges[lhs].Add(rhs);
    }

    private bool Hungarian(LT lhs)
    {
        if (visited.Contains(lhs))
        {
            return false;
        }
        visited.Add(lhs);
        foreach (var rhs in edges[lhs])
        {
            if (!RtoL.ContainsKey(rhs) || Hungarian(RtoL[rhs]))
            {
                RtoL[rhs] = lhs;
                return true;
            }
        }
        return false;
    }

    public int Compute()
    {
        int ret = 0;
        foreach (var entry in edges)
        {
            visited.Clear();
            if (Hungarian(entry.Key))
            {
                ret++;
            }
        }
        foreach (var entry in RtoL)
        {
            LtoR.Add(entry.Value, entry.Key);
        }
        return ret;
    }
}
