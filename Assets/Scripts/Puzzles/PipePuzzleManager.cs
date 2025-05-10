using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


public class PipePuzzleManager : MonoBehaviour
{
    public List<PuzzleWrapper> Pieces = new List<PuzzleWrapper>();
    [SerializeField]
    PuzzleWrapper Goal;
    [SerializeField]
    PuzzleWrapper from;
    public UnityEvent E_OnPuzzleEnd = new UnityEvent();
    public UnityEvent E_OnPuzzleReset= new UnityEvent();

    public bool GoalConnected { set; get; }

    private void Awake()
    {
        for (int i = 0; i < Pieces.Count; i++)
        {
            Pieces[i].e_OnCheck = CheckIfDone;
        }
    }


    public void CheckIfDone()
    {
        GoalConnected = FindGoalFrom(from, from.EntryPoints[0].GetOpposite());
       //done
       if(GoalConnected)
        E_OnPuzzleEnd?.Invoke();
       else
         E_OnPuzzleReset?.Invoke();
    }

    private bool FindGoalFrom(PuzzleWrapper pipe, DIRECTIONS entryDir)
    {
        List<bool> goalConnected = new List<bool>();
        foreach (var connectedDir in pipe.ConnectedPoints)
        {
            if (connectedDir == entryDir)
                continue;

            PuzzleWrapper connectedPipe = pipe.GetNeighborPiece(connectedDir);
            goalConnected.Add(FindGoalFrom(connectedPipe, connectedDir.GetOpposite()));

        }

        return pipe == Goal || (goalConnected.Count > 0 && goalConnected.Any(x=>x));
    }
    
}
