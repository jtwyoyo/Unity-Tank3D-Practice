using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MyCompany.Tank3D.CameraScript;

namespace MyCompany.Tank3D.Managers
{
    public struct GameConfig
    {
        public int RoundNumber;
        public WaitForSeconds StartWait;
        public WaitForSeconds EndWait;
        public TankManager RoundWinner;
        public TankManager GameWinner;
    }

    public class GameManager : MonoBehaviour
    {
        [SerializeField] private CameraControl m_CameraControl;
        [SerializeField] private Text m_MessageText;
        [SerializeField] private GameObject m_TankPrefab;
        [SerializeField] private TankManager[] m_Tanks;

        private const int m_NumRoundsToWin = 5;
        private const float m_StartDelay = 3f;
        private const float m_EndDelay = 3f;
        private const float k_MaxDepenetrationVelocity = float.PositiveInfinity;

        private GameConfig m_GameConfig;

        private void Awake()
        {
            Physics.defaultMaxDepenetrationVelocity = k_MaxDepenetrationVelocity;

            m_GameConfig.StartWait = new WaitForSeconds(m_StartDelay);
            m_GameConfig.EndWait = new WaitForSeconds(m_EndDelay);

            SpawnAllTanks();
            SetCameraTargets();

            StartCoroutine(GameLoop());
        }


        private void SpawnAllTanks()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].m_Instance =
                    Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
                m_Tanks[i].m_PlayerNumber = i + 1;
                m_Tanks[i].Setup();
            }
        }


        private void SetCameraTargets()
        {
            Transform[] targets = new Transform[m_Tanks.Length];

            for (int i = 0; i < targets.Length; i++)
            {
                targets[i] = m_Tanks[i].m_Instance.transform;
            }

            m_CameraControl.m_Targets = targets;
        }


        private IEnumerator GameLoop()
        {
            yield return StartCoroutine(RoundStarting());
            yield return StartCoroutine(RoundPlaying());
            yield return StartCoroutine(RoundEnding());

            if (m_GameConfig.GameWinner != null)
            {
                SceneManager.LoadScene(0);
            }
            else
            {
                StartCoroutine(GameLoop());
            }
        }


        private IEnumerator RoundStarting()
        {
            ResetAllTanks();
            DisableTankControl();

            yield return null;

            m_CameraControl.SetStartPositionAndSize();

            m_GameConfig.RoundNumber++;
            m_MessageText.text = "ROUND " + m_GameConfig.RoundNumber;

            yield return m_GameConfig.StartWait;
        }


        private IEnumerator RoundPlaying()
        {
            EnableTankControl();

            m_MessageText.text = string.Empty;

            while (!OneTankLeft())
            {
                yield return null;
            }
        }


        private IEnumerator RoundEnding()
        {
            DisableTankControl();

            m_GameConfig.RoundWinner = null;

            m_GameConfig.RoundWinner = GetRoundWinner();

            if (m_GameConfig.RoundWinner != null)
                m_GameConfig.RoundWinner.m_Wins++;

            m_GameConfig.GameWinner = GetGameWinner();

            string message = EndMessage();
            m_MessageText.text = message;

            yield return m_GameConfig.EndWait;
        }


        private bool OneTankLeft()
        {
            int numTanksLeft = 0;

            for (int i = 0; i < m_Tanks.Length; i++)
            {
                if (m_Tanks[i].m_Instance.activeSelf)
                    numTanksLeft++;
            }

            return numTanksLeft <= 1;
        }


        private TankManager GetRoundWinner()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                if (m_Tanks[i].m_Instance.activeSelf)
                    return m_Tanks[i];
            }

            return null;
        }


        private TankManager GetGameWinner()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                    return m_Tanks[i];
            }

            return null;
        }


        private string EndMessage()
        {
            string message = "DRAW!";

            if (m_GameConfig.RoundWinner != null)
                message = m_GameConfig.RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

            message += "\n\n\n\n";

            for (int i = 0; i < m_Tanks.Length; i++)
            {
                message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
            }

            if (m_GameConfig.GameWinner != null)
                message = m_GameConfig.GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

            return message;
        }


        private void ResetAllTanks()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].Reset();
            }
        }


        private void EnableTankControl()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].EnableControl();
            }
        }


        private void DisableTankControl()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].DisableControl();
            }
        }
    }
}
