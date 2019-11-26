using UnityEngine;

namespace Player
{
    public class CameraCollision : MonoBehaviour
    {
        [Header("References")]
        public Transform FocusPoint;
        public Transform CameraFollow;
        public Transform CameraSpot;

        [Header("Settings")]
        public LayerMask MaskedLayers;
        public float DetectionRadius = 0.3f;
        public float ZoomIntensity = 1.0f;
        public float CushionOffset = 3.0f;

        [Header("Private Variables")]
        private RaycastHit Hit;

        private void Update()
        {
            UpdateCamPos();
        }

        private void UpdateCamPos()
        {
            // CameraFollow will always look at our camera
            CameraFollow.transform.LookAt(CameraSpot.transform);

            // Diatance between CameraFollow and CameraSpot
            float distFromCamSpot = Vector3.Distance(CameraFollow.position, CameraSpot.position);
            // Distance between CameraFolow and this object (camera)
            float distFromCamera = Vector3.Distance(CameraFollow.position, transform.position);

            // SphereCast from CameraFollow to CameraSpot
            if (Physics.SphereCast(CameraFollow.position, DetectionRadius, CameraFollow.forward, out Hit, distFromCamSpot + CushionOffset, MaskedLayers))
            {
                // Get distance between CameraFollow and Hit.point
                var distFromHit = Vector3.Distance(CameraFollow.position, Hit.point);

                if (distFromHit < distFromCamera)
                {
                    // If the camera is behind an object, immediately put it infront
                    if (distFromCamera > CushionOffset)
                    {
                        transform.position = Hit.point + CushionOffset * -CameraFollow.forward;
                    }
                    // If the camera is behind an object
                    // But the player is up against a wall
                    else
                    {
                        // Place the camera on top of the FocusPoint
                        transform.position = CameraFollow.position;

                        // Here we can move the focus point ontop of the player's head to avoid it going inside of him
                        // Or we can disable the player's mesh all together
                    }
                }
                else
                {
                    // If the camera is already infront of the obstacle
                    // And we are moving closer towards it (like we're walking backward)
                    // Move the camera forward
                    if (distFromHit > CushionOffset)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, Hit.point + CushionOffset * -CameraFollow.forward, 5 * Time.deltaTime);
                    }
                    // If the player is up against a wall
                    // Place the camera on our focus point
                    else
                    {
                        transform.position = CameraFollow.position;
                    }
                }
            }
            else
            {
                // Ease the camera back to CameraSpot
                transform.position = Vector3.MoveTowards(transform.position, CameraSpot.position, 5 * Time.deltaTime);
            }
        }

        public void UpdateCamPosInstant()
        {
            // CameraFollow will always look at our camera
            CameraFollow.transform.LookAt(CameraSpot.transform);

            // Diatance between CameraFollow and CameraSpot
            float distFromCamSpot = Vector3.Distance(CameraFollow.position, CameraSpot.position);
            // Distance between CameraFolow and this object (camera)
            float distFromCamera = Vector3.Distance(CameraFollow.position, transform.position);

            // SphereCast from CameraFollow to CameraSpot
            if (Physics.SphereCast(CameraFollow.position, DetectionRadius, CameraFollow.forward, out Hit, distFromCamSpot + CushionOffset, MaskedLayers))
            {
                // Get distance between CameraFollow and Hit.point
                var distFromHit = Vector3.Distance(CameraFollow.position, Hit.point);

                if (distFromHit < distFromCamera)
                {
                    // If the camera is behind an object, immediately put it infront
                    if (distFromCamera > CushionOffset)
                    {
                        transform.position = Hit.point + CushionOffset * -CameraFollow.forward;
                    }
                    // If the camera is behind an object
                    // But the player is up against a wall
                    else
                    {
                        // Place the camera on top of the FocusPoint
                        transform.position = CameraFollow.position;

                        // Here we can move the focus point ontop of the player's head to avoid it going inside of him
                        // Or we can disable the player's mesh all together
                    }
                }
                else
                {
                    // If the camera is already infront of the obstacle
                    // And we are moving closer towards it (like we're walking backward)
                    // Move the camera forward
                    if (distFromHit > CushionOffset)
                    {
                        transform.position = Hit.point + CushionOffset * -CameraFollow.forward;
                    }
                    // If the player is up against a wall
                    // Place the camera on our focus point
                    else
                    {
                        transform.position = CameraFollow.position;
                    }
                }
            }
            else
            {
                // Ease the camera back to CameraSpot
                transform.position = CameraSpot.position;
            }
        }
    }
}