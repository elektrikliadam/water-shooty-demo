using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public bool isAlive = true;
    private bool onCombat;
    private bool isShooting;
    private bool isBulletCoroutineExecuting;

    public int health;
    private int bulletDirection = 0;

    private Coroutine bulletCoroutine;
    private Coroutine fireCoroutine;

    private TextMesh healthBarText;
    private RectTransform healthBar;

    private Rigidbody rb;
    private GameObject bulletResource;

    private Vector3 lookPos;

    [SerializeField]
    private PlayerControl target;
    [SerializeField]
    private Transform fireLocation;
    [SerializeField]
    private float rotateSpeed = 10, bulletSpeed = 200;

    public delegate void EnemyKilledAction();
    public static event EnemyKilledAction onKilled;

    void Start()
    {
        bulletResource = Resources.Load<GameObject>("Prefabs/Bullet");
        healthBar = transform.Find("HealthBar").GetComponent<RectTransform>();
        healthBarText = transform.Find("HealthBar").GetComponent<TextMesh>();
        rb = GetComponent<Rigidbody>();
    }


    void Update()
    {
        if (isAlive)
        {
            lookPos = transform.position - target.transform.position;
            lookPos.y = 0;

            var targetRotation = Quaternion.LookRotation(lookPos);
            healthBar.rotation = Quaternion.RotateTowards(healthBar.rotation, targetRotation, 10);

            if (isShooting & target.isAlive)
            {
                RotateTowards(targetRotation);
                if (Mathf.Abs(targetRotation.y - transform.rotation.y) < 2) // Is enemy turn enough to target
                    bulletCoroutine = StartCoroutine(FireAfterTime(lookPos, .2f));
            }
        }
    }


    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Bullet") && isAlive)
        {
            if (health > 1)
            {
                health -= 1;
                healthBarText.text = health.ToString();
                if (!onCombat)
                {
                    onCombat = true;
                    fireCoroutine = StartCoroutine(StartFiring());
                }
            }
            else
            {
                Killed();
            }
        }
    }


    private void Killed()
    {
        isAlive = false;
        isShooting = false;
        StopCoroutine(fireCoroutine);
        healthBarText.gameObject.SetActive(false);
        rb.useGravity = true;
        rb.isKinematic = false;
        var lookPos = transform.position - target.transform.position;
        rb.AddForce(lookPos * 30 * Time.deltaTime, ForceMode.Impulse);
        onKilled();
    }


    void RotateTowards(Quaternion rotateTo)
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, rotateTo, Time.deltaTime * rotateSpeed);
    }


    private IEnumerator StartFiring()
    {
        while (true)
        {
            isShooting = true;
            yield return new WaitForSeconds(1);
            isShooting = false;
            yield return new WaitForSeconds(3);
        }
    }


    private IEnumerator FireAfterTime(Vector3 lookPos, float time)
    {
        if (isBulletCoroutineExecuting)
            yield break;
        isBulletCoroutineExecuting = true;
        yield return new WaitForSeconds(time);
        if (bulletDirection == 3) bulletDirection = 0;
        var bullet = Instantiate(bulletResource, fireLocation.position, Quaternion.identity);
        bullet.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
        bullet.GetComponent<Rigidbody>().AddForce((-lookPos + new Vector3(bulletDirection++, 0, 0)) * bulletSpeed * Time.deltaTime, ForceMode.Impulse);

        isBulletCoroutineExecuting = false;
    }

}
