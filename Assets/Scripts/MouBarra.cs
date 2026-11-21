using UnityEngine;

public class MouBarra : MonoBehaviour
{ 
    public float speed, Length;
    public bool Paused;
    public bool iniciat;
    public float progress = 0f;
    public Vector2 pointA;
    public Vector2 pointB;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pointA = new Vector2(transform.position.x, transform.position.y);
        pointB = new Vector2(pointA.x+Length, transform.position.y);
        float time = Mathf.PingPong(progress, 1);
        transform.position = Vector2.Lerp(pointA, pointB, time);
    }

    // Update is called once per frame
    void Update()
    {
        if(!Paused)
        {
            progress = (progress+Time.deltaTime*speed)%2;
            float time = Mathf.PingPong(progress, 1);
            transform.position = Vector2.Lerp(pointA, pointB, time);
        }
    }
}
