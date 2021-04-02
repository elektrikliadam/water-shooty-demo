using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Camera : MonoBehaviour
{
    public Vector3 offset;
    public Transform target;
    private float localStage = 0;

    private void OnEnable()
    {
        PlayerControl.onStageChange += ChangeCameraAngle;
    }
    private void OnDisable()
    {
        PlayerControl.onStageChange -= ChangeCameraAngle;
    }

    private void ChangeCameraAngle()
    {
        localStage = target.GetComponent<PlayerControl>().stage;
        if (localStage == 2) offset = new Vector3(7, 2, 0);
        else if (localStage == 1) offset = new Vector3(7.95f, 2.65f, 3.78f);
    }

    void Update()
    {
        if (Input.GetKeyDown("r")) Restart();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void FixedUpdate()
    {
        if (target)
        {
            transform.LookAt(target.position);
            transform.position = Vector3.Lerp(transform.position, target.position + offset, 0.1f);
        }
    }

}
