using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PipeConnectorFixed : PuzzleWrapper
{
    public bool isPipe = true;
    public bool isEnd = false;
    public bool isConnected = false;

  
    public UnityEvent E_OnConnected = new UnityEvent();
    public UnityEvent E_OnDisconnect = new UnityEvent();

  
    public override void ConnectDirection(DIRECTIONS dir)
    {
        base.ConnectDirection(dir);

      //  Debug.Log("CHOKE");
        if (ConnectedPoints.Count > 0)
        {
            isConnected = true;
            E_OnConnected?.Invoke();
        }
       
          
    }
    public override void DisconnectDirection(DIRECTIONS dir)
    {
        base.DisconnectDirection(dir);
        if (ConnectedPoints.Count <= 0)
        {

            isConnected = false;
            E_OnDisconnect?.Invoke();
        }
    }

}
