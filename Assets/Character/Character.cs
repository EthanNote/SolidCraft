using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

[RequireComponent(typeof(Rigidbody))]
public class Character : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        mouseLook.Init(transform, camera);
    }
    //public AnimationCurve FreeFallAcceleration;

    //float fallDuration = 0;
    public float camHight = 1.5f;
    public MouseLook mouseLook;
    public Transform camera;



    // Update is called once per frame
    void FixedUpdate()
    {
        mouseLook.LookRotation(transform, camera);
        int castCount = 10;
        float castRadius = 0.3f;
        RaycastHit hitinfo;

        float raylength = 5;
        //Mesure cam to ground distance in multiple reycast lines
        float maxGroundDistance = -2 * raylength;
        float minGroundDistance = 2 * raylength;
        for (int i = 0; i < castCount; i++)
        {
            Vector3 startPos = new Vector3(Mathf.Sin(Mathf.PI / castCount * i), 0, Mathf.Cos(Mathf.PI / castCount * i)) * castRadius + transform.position;
            if (Physics.Raycast(startPos, Vector3.down, out hitinfo, raylength))
            {
                float distance = transform.position.y - hitinfo.point.y;

                if (distance > maxGroundDistance)
                    maxGroundDistance = distance;
                if (distance < minGroundDistance)
                    minGroundDistance = distance;
            }
        }
        //print(maxGroundDistance);
        Vector3 v;


        if (maxGroundDistance > camHight)
        {
            //Freefall acceleration
            GetComponent<Rigidbody>().velocity += Vector3.down * 10 * Time.deltaTime;

            //Max fall speed limit
            v = GetComponent<Rigidbody>().velocity;
            if (v.y < -30)
                v.y = -30;
            GetComponent<Rigidbody>().velocity = v;
        }
        else if (maxGroundDistance > 0)
        {
            //Vertical stop
            v = GetComponent<Rigidbody>().velocity;
            v.y = 0;
            GetComponent<Rigidbody>().velocity = v;

            //Fit ground
            transform.position += Vector3.up * (camHight - maxGroundDistance);
        }

        //Horizontal Move
        Vector3 R = transform.right;
        R.y = 0;
        Vector3 F = new Vector3(-R.z, 0, R.x);
        Vector3 inputDirection = Input.GetAxis("Horizontal") * R + Input.GetAxis("Vertical") * F;

        v = GetComponent<Rigidbody>().velocity;
        float vy = v.y;
        v.y = 0;
        float s = v.magnitude;
        s -= 50 * Time.deltaTime;
        if (s < 0)
        {
            s = 0;
            if (inputDirection.sqrMagnitude < 0.001)
                GetComponent<Rigidbody>().velocity = Vector3.up * vy;
        }
        else
        {
            GetComponent<Rigidbody>().velocity = v.normalized * s + Vector3.up * vy;
        }

        GetComponent<Rigidbody>().velocity += inputDirection.normalized * 100 * Time.deltaTime;

        //Max Horizontal Speed Limit
        v = GetComponent<Rigidbody>().velocity;
        Vector3 vh = new Vector3(v.x, 0, v.z);
        s = vh.magnitude;
        //print(GetComponent<Rigidbody>().velocity);
        if (s > 5)
        {
            vh = vh.normalized * 5;
            vh.y = v.y;
            GetComponent<Rigidbody>().velocity = vh;
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetComponent<Rigidbody>().velocity = Vector3.up * 7;
        }
    }
}
