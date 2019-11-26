using System;
using UnityEngine;

namespace Player
{
    public class Input : MonoBehaviour
    {
        public bool StopInput = false;  // We change this when we no longer want the player to do any input
        private Rewired.Player Inputs;

        [Header("Keyboard")]
        public Vector2 Direction;
        public Vector2 DirectionModified
        {
            get
            {
                float mod = (Direction.x != 0 && Direction.y != 0) ? 0.701f : 1.0f;
                Vector2 dir = new Vector2(Direction.x * mod, Direction.y * mod);
                return dir;
            }
        }

        public bool Jump;
        public bool HoldJump;
        public bool Sprint;

        public bool DodgeLeft;
        public bool DodgeRight;

        public bool CameraSide = true;  // true = right | false = left
        public int WeaponNumber;

        [Header("Mouse")]
        public float Yaw;
        public float Pitch;

        [Header("Combat")]
        public bool Reload;

        public bool HoldLeftFire;
        public bool LeftFireDown;
        public bool LeftFireUp;

        public bool HoldRightFire;
        public bool RightFireDown;
        public bool RightFireUp;

        public bool HeavyLeftFire;
        public bool HeavyRightFire;

        [Header("Options")]
        public float MouseScrollSpeed = 0.5f;

        public bool AnyAttack
        {
            get
            {
                return HoldLeftFire || LeftFireDown || HeavyLeftFire
                    || HoldRightFire || RightFireDown || HeavyRightFire;
            }
        }

        public void Initialize()
        {
            Inputs = Rewired.ReInput.players.GetPlayer(0);
        }

        public void UpdateKeyboard()
        {
            if (StopInput)
            {
                Direction = Vector2.zero;

                Jump = false;
                Sprint = false;

                DodgeLeft = false;
                DodgeRight = false;

                return;
            }

            // We get our horizontal/vertical inputs as integers so we don't have to deal with floating point value errors
            Direction.x = (int)(Sprint ? 0 : Inputs.GetAxis("Horizontal"));
            Direction.y = (int)(Sprint ? 1 : Inputs.GetAxis("Vertical"));

            Jump = Inputs.GetButtonDown("Jump");
            HoldJump = Inputs.GetButton("Jump");

            if (Inputs.GetButtonDoublePressDown("Vertical"))
                Sprint = true;
            else if (Inputs.GetButtonUp("Vertical") || Jump || AnyAttack)
                Sprint = false;

            // Reset the dodge bools before we use it
            DodgeLeft = false;
            DodgeRight = false;
            if (Settings.GameSettingsManager.Instance.ControlSettings.Save.ControlDoubleTapDodge != 0)
            {
                DodgeLeft = Inputs.GetNegativeButtonDoublePressDown("Horizontal") && !DodgeRight;
                DodgeRight = Inputs.GetButtonDoublePressDown("Horizontal") && !DodgeLeft;
            }
            else
            {
                DodgeLeft = Direction.x < 0 && Direction.y == 0 && Jump && !DodgeRight;
                DodgeRight = Direction.x > 0 && Direction.y == 0 && Jump && !DodgeLeft;
            }

            if (Inputs.GetButtonDown("Camera Side"))
                CameraSide = !CameraSide;

            // Select Weapon
            if (Inputs.GetButtonDown("Weapon 1"))
                WeaponNumber = 0;
            if (Inputs.GetButtonDown("Weapon 2"))
                WeaponNumber = 1;
            if (Inputs.GetButtonDown("Weapon 3"))
                WeaponNumber = 2;

            // Scroll Weapon
            if (Inputs.GetAxis("Scroll Weapon") * MouseScrollSpeed >= 1)
                WeaponNumber--;
            else if (Inputs.GetAxis("Scroll Weapon") * MouseScrollSpeed <= -1)
                WeaponNumber++;
            // Infinite scroll allows us to go to the first weapon if we attempt to scroll down on the last weapon
            if (Settings.GameSettingsManager.Instance.ControlSettings.Save.ControlWeaponSwitchInfiniteScroll != 0)
            {
                // Just reset the number
                WeaponNumber = WeaponNumber > 2 ? 0 : WeaponNumber < 0 ? 2 : WeaponNumber;
            }

            WeaponNumber = Mathf.Clamp(WeaponNumber, 0, 2);
        }

        public void UpdateMouse()
        {
            if (StopInput)
            {
                return;
            }

            bool yawInverse = Settings.GameSettingsManager.Instance.ControlSettings.Save.ControlInvertedMouseX != 0;
            Yaw += (yawInverse ? -1 : 1) * (Inputs.GetAxisRaw("MouseX") * (float)Settings.GameSettingsManager.Instance.ControlSettings.Save.ControlMouseXSensitivity / 100);
            Yaw %= 360.0f;

            bool pitchInverse = Settings.GameSettingsManager.Instance.ControlSettings.Save.ControlInvertedMouseY != 0;
            Pitch += (pitchInverse ? -1 : 1) * (-Inputs.GetAxisRaw("MouseY") * (float)Settings.GameSettingsManager.Instance.ControlSettings.Save.ControlMouseYSensitivity / 100);
            Pitch = Mathf.Clamp(Pitch, -90.0f, 90.0f);
        }

        public void UpdateCombat(bool hasHeavyAttack)
        {
            if (StopInput)
            {
                Reload = false;

                HoldLeftFire = false;
                LeftFireDown = false;
                LeftFireUp = false;

                HoldRightFire = false;
                RightFireDown = false;
                RightFireUp = false;

                return;
            }

            Reload = Inputs.GetButtonDown("Reload");

            HoldLeftFire = Inputs.GetButton("Left Fire");
            LeftFireDown = !hasHeavyAttack && Inputs.GetButtonDown("Left Fire");
            LeftFireUp = Inputs.GetButtonUp("Left Fire");

            HoldRightFire = Inputs.GetButton("Right Fire");
            RightFireDown = !hasHeavyAttack && Inputs.GetButtonDown("Right Fire");
            RightFireUp = Inputs.GetButtonUp("Right Fire");
        }

        int leftHeldFrames;
        bool leftAlreadyHit;
        int rightHeldFrames;
        bool rightAlreadyHit;
        public void UpdateHeavyCombat()
        {
            if (StopInput)
            {
                HeavyLeftFire = false;
                HeavyRightFire = false;

                return;
            }

            HeavyLeftFire = Inputs.GetButton("Left Fire");
            HeavyRightFire = Inputs.GetButton("Right Fire");

            CheckLeftHeavy();
            CheckRightHeavy();
        }

        private void CheckLeftHeavy()
        {
            // We check if our frames are greater than 10 first
            // If it is, then we've held down the left click for the right amount of time
            if (leftHeldFrames >= 18)
            {
                HeavyLeftFire = true;
                leftHeldFrames = 0;
                // We use already hit to stop the player from holding down left click
                // and continuously using heavy attacks
                leftAlreadyHit = true;
            }
            else
                HeavyLeftFire = false;

            // If we're holding down left click
            // increase the amount of frames we've held it for
            // This will be used later to check if it's greater than 10
            // to do the attack
            if (HoldLeftFire)
            {
                if (!leftAlreadyHit)
                    leftHeldFrames++;
            }
            // If we let go of left click early
            else
            {
                // But we did hold it down for a bit
                // we should still do the attack
                //if (leftHeldFrames > 0 && leftHeldFrames < 10)
                //    HeavyLeftFire = true;
                if (leftHeldFrames > 0)
                    LeftFireDown = true;
                else
                    LeftFireDown = false;

                leftHeldFrames = 0;
                leftAlreadyHit = false;
            }
        }

        private void CheckRightHeavy()
        {
            // We check if our frames are greater than 10 first
            // If it is, then we've held down the left click for the right amount of time
            if (rightHeldFrames >= 18)
            {
                HeavyRightFire = true;
                rightHeldFrames = 0;
                // We use already hit to stop the player from holding down left click
                // and continuously using heavy attacks
                rightAlreadyHit = true;
            }
            else
                HeavyRightFire = false;

            // If we're holding down left click
            // increase the amount of frames we've held it for
            // This will be used later to check if it's greater than 10
            // to do the attack
            if (HoldRightFire)
            {
                if (!rightAlreadyHit)
                    rightHeldFrames++;
            }
            // If we let go of left click early
            else
            {
                // But we did hold it down for a bit
                // we should still do the attack
                //if (rightHeldFrames > 0 && rightHeldFrames < 10)
                //    HeavyRightFire = true;
                if (rightHeldFrames > 0)
                    RightFireDown = true;
                else
                    RightFireDown = false;

                rightHeldFrames = 0;
                rightAlreadyHit = false;
            }
        }
    }
}