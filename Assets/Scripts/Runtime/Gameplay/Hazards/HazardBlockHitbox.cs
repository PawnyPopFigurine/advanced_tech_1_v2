using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace JZK.Gameplay
{
    public class HazardBlockHitbox : MonoBehaviour
    {
        [SerializeField] HazardBlockController _controller;

        public void OnCollisionEnter2D(Collision2D collision)
        {
            _controller.OnCollide(collision.otherCollider.gameObject);
        }

        public void OnTriggerEnter2D(Collider2D collider)
        {
            _controller.OnCollide(collider.gameObject);
        }
    }
}