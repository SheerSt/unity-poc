using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAlly : MonoBehaviour
{

    public bool attackHorizontal;
    public bool attackVertical;
    public bool damaged;

    private Animator pokemonAnimator;
    private Animator attackAnimator;

    // Start is called before the first frame update
    void Start()
    {
        
        pokemonAnimator = transform.Find("Drilbur").gameObject.GetComponent<Animator>();
        attackAnimator = transform.Find("AttackAnim").gameObject.GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
     
        if (attackHorizontal)
        {
            pokemonAnimator.SetTrigger("attack-horizontal");
            attackAnimator.SetTrigger("slash-horizontal");
            attackHorizontal = false;
        }
        if (attackVertical)
        {
            pokemonAnimator.SetTrigger("attack-vertical");
            attackAnimator.SetTrigger("slash-vertical");
            attackVertical = false;
        }
        if (damaged)
        {
            pokemonAnimator.SetTrigger("damaged");
            damaged = false;
        }

    }
}
