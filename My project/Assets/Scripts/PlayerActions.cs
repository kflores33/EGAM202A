using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    public enum ActionStates
    {
        Default,
        Parry,
        Launch
    }
    public ActionStates currentState;

    [Header("Keybinds")]
    public KeyCode parryKey = KeyCode.Mouse0;
    public KeyCode launchKey = KeyCode.Mouse1;

    [Header("Parry Variables")]
    public bool canParry;

    public float parryDuration = 1f;
    public float parryCooldown = 2f;

    public float enemyKnockback = 5f;
    public float knockbackRadius = 10f;

    public Coroutine knockbackAllowance;

    public Coroutine parryCooldownCoroutine;
    public Coroutine parryActiveCoroutine;

    public AuraTriggerDetection parryAura;

    [Header("Launch Variables")]
    public bool canLaunch;

    public int launchCharges = 0;

    public float playerLaunchForce;

    // Start is called before the first frame update
    void Start()
    {
        currentState = ActionStates.Default;
        canParry = true;
        parryAura.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState) 
        { 
            case ActionStates.Default:
                UpdateDefault(); break;
            case ActionStates.Parry:
                UpdateParry(); break;
            case ActionStates.Launch:
                UpdateLaunch(); break;
        }
    }

    void UpdateDefault()
    {
        // switch to parry state when parry button is pressed (and if cooldown is at 0)
        if (Input.GetKey(parryKey) && canParry) 
        {
            currentState = ActionStates.Parry;
        }
        // switch to launch state if button is pressed
        if (Input.GetKey(launchKey) && canLaunch)
        {

        }
    }
    void UpdateParry()
    {
        // parry orb shows up for a few seconds (makes player invulnerable to damage)
        StartParryState();

        // if there's a collision with an enemy, knock the enemy back and add a launch charge
        if (parryAura.hitWhileParry) 
        {
            ParrySuccess();
        }
    }
    void UpdateLaunch()
    {
        // make player invulnerable to damage and add force in the direction the player is facing
        // subtract from number of launch charges available
    }

    // starts the parry state & duration timer
    void StartParryState()
    {
        canParry = false;   

        parryAura.gameObject.SetActive(true);

        if (parryActiveCoroutine == null)
        {
            parryActiveCoroutine = StartCoroutine(ParryActiveCoroutine());
        }
    }

    // ends parry state and starts cooldown timer
    void DisableParry()
    {
        Debug.Log("parry disabled");
        parryAura.gameObject.SetActive(false );

        if (parryActiveCoroutine != null)
        {
            StopCoroutine(parryActiveCoroutine);
            parryActiveCoroutine = null;
        }
        if (knockbackAllowance != null)
        {
            StopCoroutine(knockbackAllowance);
            knockbackAllowance = null;  
        }

        // return to default state
        currentState = ActionStates.Default;

        // start cooldown (move to after parry is finished executing)
        if (parryCooldownCoroutine == null)
        {
            parryCooldownCoroutine = StartCoroutine(ParryCooldownCoroutine());
        }
    }

    // resets parry and ends cooldown coroutine
    void ResetParry()
    {
        Debug.Log("parry enabled");
        canParry = true;    

        if (parryCooldownCoroutine != null)
        {
            StopCoroutine(parryCooldownCoroutine);
            parryCooldownCoroutine = null;
        }        
    }

    void ParrySuccess()
    {
        if(parryAura.currentEnemy != null)
        {
            Knockback(parryAura.currentEnemy);

            DisableParry();
        }
    }

    // starts cooldown
    IEnumerator ParryCooldownCoroutine()
    {
        yield return new WaitForSeconds(parryCooldown);

        //Debug.Log("can parry again");
        ResetParry();        

        yield break;
    }

    // waits till end of duration to disable parry state
    IEnumerator ParryActiveCoroutine() 
    {
        yield return new WaitForSeconds(parryDuration);

        DisableParry();        

        yield break;
    }

    void Knockback(EnemyAI enemy)
    {
        enemy.GetComponent<Rigidbody>().isKinematic = false;

        // calculates the direction the enemy is in in relation to player
        Vector3 direction = (transform.position - enemy.transform.position).normalized;

        // adds knockback
        enemy.GetComponent<Rigidbody>().AddForce(direction * enemyKnockback, ForceMode.Impulse);

        if (knockbackAllowance == null)
        {
            knockbackAllowance = StartCoroutine(waitToResetRb(enemy));
        }

        Debug.Log("bababaBOOM");
    }

    IEnumerator waitToResetRb(EnemyAI enemy)
    {
        yield return new WaitForSeconds(3);

        enemy.GetComponent<Rigidbody>().isKinematic = true;
        parryAura.currentEnemy = null;

        yield break;
    }
}
