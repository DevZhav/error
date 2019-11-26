using MLAPI;
using System.Collections;
using UnityEngine;

namespace Game
{
    public class Touchdown : Game
    {
        public enum TouchdownScores
        {
            s10 = 10,
            s8 = 8,
            s4 = 4,
            s2 = 2
        }

        [Header("Touchdown")]
        public TouchdownScores Scores;
        public GameObject BallPrefab;
        private Objects.Ball Ball;

        private void FixedUpdate()
        {
            CheckPause();
            CheckProgress();

            if (!nv_InProgress.Value)
                return;

            nv_CurrentTime.Value = CurrentTime;
        }

        public override void StartMatch()
        {
            nv_AlphaScore.Value = 0;
            nv_BetaScore.Value = 0;
            MaxScore = (byte)Scores;

            // Spawn the ball
            Transform ballSpawn = GameObject.FindGameObjectWithTag("Ball Spawn").transform;

            GameObject ball = Instantiate(BallPrefab, ballSpawn.position, ballSpawn.rotation);
            ball.GetComponent<NetworkedObject>().Spawn();
            Ball = ball.GetComponent<Objects.Ball>();

            base.StartMatch();
        }

        public override void StopMatch()
        {
            // Despawn the ball
            Destroy(Ball);

            base.StopMatch();
        }

        public override void PauseMatch(float seconds)
        {
            base.PauseMatch(seconds);
        }

        bool reset;
        public override void CheckPause()
        {
            base.CheckPause();

            // If the game is paused and less than 1 second is left before we unpause
            if (nv_InProgress.Value == false && nv_PauseTimeLeft.Value <= 1 && !reset)
            {
                reset = true;

                // Reset the ball
                Ball.GetComponent<Objects.Ball>().LocalResetBall();

                // Reset our players to their spawn
                // And reset their health and ammo
                Player[] p = FindObjectsOfType<Player>();
                for (int i = 0; i < p.Length; i++)
                {
                    p[i].nv_Health.Value = p[i].MaxHealth;
                    p[i].Spawn();
                }
            }
            else if (nv_InProgress.Value)
                reset = false;
        }

        public override void CheckProgress()
        {
            base.CheckProgress();

            // If we're at the end of the match time or one team has hit max score
            if (nv_CurrentTime.Value <= 0 || (nv_AlphaScore.Value >= MaxScore || nv_BetaScore.Value >= MaxScore))
            {
                // End the game
            }
            // If we're at half of the match
            else if (nv_CurrentTime.Value <= ((float)TimeLimit * 60)/2 || (nv_AlphaScore.Value > MaxScore / 2 || nv_BetaScore.Value > MaxScore / 2))
            {
                // maybe pause the game here
            }
        }

        public override void ResumeMatch()
        {
            base.ResumeMatch();
        }

        public void EndRound()
        {
            Debug.Log("End round!");
            PauseMatch(10);
        }

        public override void DoChecks()
        {
            base.DoChecks();
            // Check for game end information here
        }
    }
}