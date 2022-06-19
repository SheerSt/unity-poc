using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject gameManager;

    // Start is called before the first frame update
    void Awake()
    {

        if (Game.manager == null) Instantiate(gameManager);

    }

}
