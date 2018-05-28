using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour {

    public static List<Interactable> interactables;

    public enum Type
    {
        Portal, Goal
    }

    public Type t;
    
    void Awake()
    {
        if(interactables == null)
        {
            interactables = new List<Interactable>();
        }

        interactables.Add(this);
    }

    public void Interact(PlayerController player)
    {
        switch (t)
        {
            case Type.Portal:
                player.SetDirection((PlayerController.Direction)(1 - (int)player.moving_towards));
                break;
            case Type.Goal:
                LevelLoader.instance.LoadNextLevel();
                break;
        }
    }

    public void DestroyObj()
    {
        interactables.Remove(this);
        Destroy(this.gameObject);
    }
}
