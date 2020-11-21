using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed = 10;
    public Rigidbody2D body;

    int numBounces = 0;

    public int bouncesTillDeath = 5;

    public float secondsTillDeath = 5;

    private float timeAlive = 0;

    public PlayerController shooter;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        body.velocity = new Vector2(transform.right.x, transform.right.y) * speed;
    }

    public void init(PlayerController _shooter) {
        shooter = _shooter;
    }

    // Update is called once per frame
    void Update()
    {
        timeAlive += Time.deltaTime;
        if(timeAlive > secondsTillDeath) {
            Destroy(gameObject);
        }
    }

    private void OnDestroy() {
        shooter?.onOwnBulletDestroyed();
    }

    void OnCollisionEnter2D(Collision2D collision) {
        numBounces++;

        if(collision.gameObject.tag != "wall") {
            Destroy(gameObject);
        }

        if(numBounces > bouncesTillDeath) {
            Destroy(gameObject);
        }
    }
}
