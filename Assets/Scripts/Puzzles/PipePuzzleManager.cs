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

    private void Awake()
    {
        for (int i = 0; i < Pieces.Count; i++)
        {
            Pieces[i].e_OnCheck = CheckIfDone;
        }
    }


    public void CheckIfDone()
    {
       //done
            E_OnPuzzleEnd?.Invoke();
       // else
         //   E_OnPuzzleReset?.Invoke();
    }

   
    
}
