using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Unit
{

    public Player aggroAlly;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {

        base.Update();

    }

    public override IEnumerator FaintAnimation()
    {

        // Update existing enemies.
        Game.manager.enemies.Remove(this);

        yield return StartCoroutine(base.FaintAnimation());

    }
}
