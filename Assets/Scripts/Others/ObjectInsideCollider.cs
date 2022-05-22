using System.Collections;
using UnityEngine;

namespace Others
{
    /// <summary>
    /// Detects collisions of the object it is attached to with other objects
    /// to enable combination.
    /// </summary>
    public class ObjectInsideCollider : MonoBehaviour
    {
        /// <summary>
        /// Temporary store of the name of the collided object.
        /// </summary>
        /// <value></value>
        public string combinedWithName = "";
        /// <summary>
        /// Is set when after a valid combine check to improve performance.
        /// </summary>
        /// <value>Default false.</value>
        private bool colliderSleeps = false;
        /// <summary>
        /// Reset on FixedUpdate.
        /// </summary>
        /// <value>Default is false.</value>
        private bool collisionWasCheckedThisFrame = false;

        /// <summary>
        /// Sets collisionWasCheckedThisFrame to false.
        /// </summary>
        private void FixedUpdate()
        {
            collisionWasCheckedThisFrame = false;
        }

        /// <summary>
        /// Checks if the collided object is valid for combining.
        /// </summary>
        /// <param name="collision">Given from engine.</param>
        private void OnCollisionEnter(Collision collision)
        {
            //Return if the collision was already checked
            if (collisionWasCheckedThisFrame) return;

            //Check if this collider should be sleeping
            if (colliderSleeps) return;
        
            //Return if the colliding object is not an TrainARObject
            if (!collision.transform.CompareTag("TrainARObjectCollider")) return;
        
            //sleep the collider so no further checks are performed and wake it up delayed in a coroutine
            colliderSleeps = true;
            StartCoroutine(WakeColliderUp());
            //Handle the collision if the respective statemachine state is active
            var collidedARObject = collision.gameObject;
            collidedARObject.GetComponent<Interaction.TrainARObject>().Combine(this.combinedWithName, null);
            collisionWasCheckedThisFrame = true;
        }

        /// <summary>
        /// Sets colliderSleeps to false after 1 second.
        /// </summary>
        /// <returns></returns>
        private IEnumerator WakeColliderUp()
        {
            yield return new WaitForSeconds(1.0f);
            colliderSleeps = false;
        }
    }
}