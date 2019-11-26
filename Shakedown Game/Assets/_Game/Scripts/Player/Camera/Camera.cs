using Cinemachine;
using DG.Tweening;
using SCPE;
using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Player
{
    public class Camera : MonoBehaviour
    {
        [Header("References")]
        public UnityEngine.Camera Cam;
        public Transform FollowPosition;
        public CameraCollision CameraCollision;
        private Input Input;
        private Controller Controller;

        [Header("Post Processing")]
        Danger Danger;
        SpeedLines SpeedLines;
        DoubleVision DoubleVision;
        ChromaticAberration ChromaticAberration;

        private Vector3 originalCameraSpotPosition;
        private Vector3 originalCameraFollowPosition;

        private void OnEnable()
        {
            // Setup Post Processing references
            PostProcessVolume volume = FindObjectOfType<PostProcessVolume>();
            volume.profile.TryGetSettings(out Danger);
            volume.profile.TryGetSettings(out SpeedLines);
            volume.profile.TryGetSettings(out DoubleVision);
            volume.profile.TryGetSettings(out ChromaticAberration);
        }

        public void Initialize(Input input, Controller controller)
        {
            Input = input;
            Controller = controller;

            // Setup variables
            originalCameraSpotPosition = CameraCollision.CameraSpot.localPosition;
            originalCameraFollowPosition = CameraCollision.CameraFollow.localPosition;
        }

        public void UpdateCamera()
        {
            // Move the camera to the follow position
            transform.position = FollowPosition.position;
            // Rotate the camera
            transform.localRotation = Quaternion.Euler(Input.Pitch, Input.Yaw, transform.localRotation.z);

            // This whole section right here handles which side the camera is on
            Tuple<Vector3, Vector3> pos = GetNewCameraPosition(Input.CameraSide, originalCameraSpotPosition, originalCameraFollowPosition, CameraCollision);
            CameraCollision.CameraSpot.localPosition = Vector3.Lerp(CameraCollision.CameraSpot.localPosition, pos.Item1, 25 * Time.deltaTime);
            CameraCollision.CameraFollow.localPosition = Vector3.Lerp(CameraCollision.CameraFollow.localPosition, pos.Item2, 25 * Time.deltaTime);
            ////////////////////////////////////////////////////////////////////

            UpdateFOV();
            // This returns the danger back to normal
            //Lines.intensity.value = Mathf.Lerp(Lines.intensity.value, Controller.Motor.State.IsSprinting ? 1f : 0f, 15.0f * Time.deltaTime);
            SpeedLines.intensity.value = Mathf.MoveTowards(SpeedLines.intensity.value, Controller.Motor.State.IsSprinting ? 1f : 0f, 15.0f * Time.deltaTime);
            DoubleVision.intensity.value = Mathf.MoveTowards(DoubleVision.intensity.value, Controller.Motor.State.IsSprinting ? 0.3f : 0f, 15.0f * Time.deltaTime);

            Danger.size.value = Mathf.Lerp(Danger.size.value, 0f, 4.0f * Time.deltaTime);
            ChromaticAberration.intensity.value = Mathf.MoveTowards(ChromaticAberration.intensity.value, 0f, 1f * Time.deltaTime);
        }

        private Tuple<Vector3, Vector3> GetNewCameraPosition(bool right, Vector3 OriginalCameraSpotPosition, Vector3 OriginalCameraFollowPosition, CameraCollision CameraCollision)
        {
            Vector3 newCamPos;
            Vector3 newFollowPos;

            float dir = right ? 1 : -1;
            // Move the camera to the right
            newCamPos = new Vector3(dir * OriginalCameraSpotPosition.x, CameraCollision.CameraSpot.localPosition.y, CameraCollision.CameraSpot.localPosition.z);
            // Move the follow position to the right
            newFollowPos = new Vector3(dir * OriginalCameraFollowPosition.x, CameraCollision.CameraFollow.localPosition.y, CameraCollision.CameraFollow.localPosition.z);

            return new Tuple<Vector3, Vector3>(newCamPos, newFollowPos);
        }

        private void UpdateFOV()
        {
            if (Controller.Motor.State.IsSprinting)
            {
                SetFOV(85, 7);
            }
            else
            {
                SetFOV(Settings.GameSettingsManager.Instance.DisplaySettings.Save.FieldOfView, 10);
            }
        }
        private void SetFOV(float fov, float lerpSpeed)
        {
            float curFOV = Cam.fieldOfView;
            curFOV = Mathf.Lerp(curFOV, fov, lerpSpeed * Time.deltaTime);
            Cam.fieldOfView = curFOV;
        }

        /// <summary>
        /// Shake the camera smoothly
        /// </summary>
        /// <param name="strength">a good parameter would be 0.2f</param>
        public void Shake(float strength)
        {
            Cam.transform.DOComplete();
            Cam.transform.DOShakePosition(.2f, strength, 14, 90, false, true);
        }

        public void SetDanger(float damage)
        {
            Danger.color.value = damage > 0 ? new Color32(168, 0, 0, 255) : new Color32(0, 119, 204, 255);
            Danger.size.value = Mathf.Clamp(Mathf.Abs(damage) / 50, 0.0f, 1.0f);

            ChromaticAberration.intensity.value = Mathf.Clamp(Mathf.Abs(damage) / 50, 0.0f, 1.0f);
        }
    }
}