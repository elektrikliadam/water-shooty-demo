using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
{
    private Quaternion startRotation;
    private Enemy enemy;

    private bool isBulletCoroutineExecuting;
    private bool isLevelCompleted;
    private bool reachedNextStage;
    private bool isStageCompleted;
    private bool isOnCombat = false;
    public bool isAlive = true;

    public int stage = 0;
    private int health = 2;

    private Vector3 lookPos;

    private Coroutine bulletCoroutine;
    private GameObject bulletResource;

    private Blockage blockage;
    private Transform[] stageLocations;
    public Transform stage0;
    public Transform stage1;
    public Transform stage2;

    private Animation anim;
    private Rigidbody rb;

    public Canvas levelPanel;

    [SerializeField]
    Transform fireLocation;
    [SerializeField]
    private float rotateSpeed = 10;


    public delegate void StageChangeAction();
    public static event StageChangeAction onStageChange;

    private void OnEnable()
    {
        Enemy.onKilled += EnemyKilled;
    }
    private void OnDisable()
    {
        Enemy.onKilled -= EnemyKilled;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animation>();
        stageLocations = new Transform[] { stage0, stage1, stage2 };
        startRotation = transform.rotation;
        bulletResource = Resources.Load<GameObject>("Prefabs/Bullet");
    }


    void Update()
    {
        if (isAlive)
        {
            if (Input.GetKeyDown("v")) EnemyKilled(); // Debug

            if (isOnCombat)
            {
                if (Input.GetMouseButton(0))
                {
                    lookPos = transform.position - enemy.transform.position;
                    lookPos.y = 0;
                    var targetRotation = Quaternion.LookRotation(lookPos);
                    RotateTowards(targetRotation * Quaternion.Euler(0, 0, -35));
                    if (Mathf.Abs(targetRotation.y - transform.rotation.y) < 2 && blockage != null)
                    {
                        blockage.GetComponent<BoxCollider>().enabled = false;
                        bulletCoroutine = StartCoroutine(FireAfterTime(lookPos, .3f));
                    }
                }
                else
                {

                    if (!isBulletCoroutineExecuting && transform.rotation != startRotation)
                    {
                        blockage.GetComponent<BoxCollider>().enabled = true;

                        RotateTowards(startRotation);
                    }
                }
            }
            else
            {
                if (isStageCompleted)
                {
                    if (transform.rotation != startRotation)
                    {
                        if (!isBulletCoroutineExecuting) RotateTowards(startRotation);
                    }
                    else
                    {
                        stage += 1;
                        onStageChange();
                        if (stage >= 3)
                        {
                            LevelCompleted();
                        }
                        isStageCompleted = false;
                    }
                }
                if (reachedNextStage)
                {
                    if (transform.rotation != startRotation)
                        RotateTowards(startRotation);
                    else
                    {
                        isOnCombat = true;
                        reachedNextStage = false;
                    }
                }
            }
        }
    }


    private void FixedUpdate()
    {
        if (!isOnCombat && !isLevelCompleted && isAlive && !reachedNextStage && !isStageCompleted)
        {
            lookPos = transform.position - stageLocations[stage].position;
            lookPos.y = 0;

            var targetRotation = Quaternion.LookRotation(lookPos);
            RotateTowards(targetRotation);
            if (Mathf.Abs(targetRotation.y - transform.rotation.y) < 2)
            {
                transform.position = Vector3.MoveTowards(transform.position, stageLocations[stage].position, Time.fixedDeltaTime * 30);
                if (transform.position.Equals(stageLocations[stage].position))
                {
                    reachedNextStage = true;

                }
            }
        }
    }


    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Bullet") && isAlive && isOnCombat)
        {
            if (health > 0)
            {
                health--;
            }
            else
            {
                Death();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Blockage") && isAlive)
        {
            blockage = other.GetComponent<Blockage>();
            enemy = blockage.enemy;
        }
    }


    private void EnemyKilled()
    {
        isStageCompleted = true;
        isOnCombat = false;
    }

    private void LevelCompleted()
    {
        if (anim.isPlaying) return;
        isLevelCompleted = true;
        anim.Play("VictoryDance");
        levelPanel.transform.Find("Text").GetComponent<Text>().text = "Level Completed";
        levelPanel.gameObject.SetActive(true);
    }


    private void Death()
    {
        isAlive = false;
        isOnCombat = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.AddForce(lookPos * 100 * Time.deltaTime, ForceMode.Impulse);
        rb.mass = 100;
        levelPanel.gameObject.SetActive(true);
    }




    void RotateTowards(Quaternion rotateTo)
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, rotateTo, Time.deltaTime * rotateSpeed);
    }

    IEnumerator FireAfterTime(Vector3 lookPos, float time)
    {
        if (isBulletCoroutineExecuting)
            yield break;
        isBulletCoroutineExecuting = true;
        yield return new WaitForSeconds(time);
        var bullet = Instantiate(bulletResource, fireLocation.position, Quaternion.identity);
        bullet.GetComponent<Renderer>().material.SetColor("_Color", Color.cyan);
        bullet.GetComponent<Rigidbody>().AddForce((-lookPos + new Vector3(0, 1, 1)) * 200 * Time.deltaTime, ForceMode.Impulse);
        isBulletCoroutineExecuting = false;
    }
}
