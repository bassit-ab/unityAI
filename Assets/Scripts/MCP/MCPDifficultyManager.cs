using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace RunnerGame.MCP
{
    public class MCPDifficultyManager : MonoBehaviour
    {
        [Header("Difficulty Metrics")]
        public float playerSkillLevel = 0.5f; // 0-1 range
        public float sessionLength = 0f;
        public int currentStreak = 0;
        public float averageReactionTime = 1f;
        
        [Header("Adaptive Parameters")]
        public AnimationCurve difficultyProgression;
        public float obstacleFrequencyMultiplier = 1f;
        public float enemyHealthMultiplier = 1f;
        public float rewardFrequencyMultiplier = 1f;
        
        [Header("Performance Tracking")]
        public Queue<float> recentPerformance = new Queue<float>();
        public int performanceSampleSize = 10;
        
        private float lastAdjustmentTime;
        private const float ADJUSTMENT_INTERVAL = 15f; // Adjust every 15 seconds
        
        public static MCPDifficultyManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDifficulty();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Update()
        {
            sessionLength += Time.deltaTime;
            
            if (Time.time - lastAdjustmentTime >= ADJUSTMENT_INTERVAL)
            {
                AdjustDifficulty();
                lastAdjustmentTime = Time.time;
            }
        }
        
        private void InitializeDifficulty()
        {
            // Load player skill from previous sessions
            playerSkillLevel = PlayerPrefs.GetFloat("PlayerSkillLevel", 0.5f);
            
            // Initialize difficulty curve
            if (difficultyProgression == null)
            {
                difficultyProgression = new AnimationCurve();
                difficultyProgression.AddKey(0f, 0.3f);    // Easy start
                difficultyProgression.AddKey(300f, 0.7f);  // Medium after 5 minutes
                difficultyProgression.AddKey(600f, 1f);    // Hard after 10 minutes
            }
        }
        
        public void RecordPlayerPerformance(float performanceScore)
        {
            recentPerformance.Enqueue(performanceScore);
            
            if (recentPerformance.Count > performanceSampleSize)
            {
                recentPerformance.Dequeue();
            }
            
            // Update skill level based on recent performance
            if (recentPerformance.Count >= 3)
            {
                float avgPerformance = recentPerformance.Average();
                playerSkillLevel = Mathf.Lerp(playerSkillLevel, avgPerformance, 0.1f);
                
                // Save updated skill level
                PlayerPrefs.SetFloat("PlayerSkillLevel", playerSkillLevel);
            }
        }
        
        private void AdjustDifficulty()
        {
            float baseDifficulty = difficultyProgression.Evaluate(sessionLength);
            float adjustedDifficulty = baseDifficulty * Mathf.Lerp(0.7f, 1.3f, playerSkillLevel);
            
            // Adjust game parameters
            obstacleFrequencyMultiplier = Mathf.Lerp(0.8f, 1.5f, adjustedDifficulty);
            enemyHealthMultiplier = Mathf.Lerp(0.7f, 1.4f, adjustedDifficulty);
            rewardFrequencyMultiplier = Mathf.Lerp(1.3f, 0.8f, adjustedDifficulty);
            
            // Broadcast difficulty change
            BroadcastDifficultyChange();
        }
        
        private void BroadcastDifficultyChange()
        {
            // Notify other systems about difficulty changes
            var gameManagers = FindObjectsOfType<MonoBehaviour>()
                .Where(mb => mb.GetType().Name.Contains("Manager"));
            
            foreach (var manager in gameManagers)
            {
                manager.SendMessage("OnDifficultyChanged", 
                    new DifficultyData 
                    { 
                        obstacleFrequency = obstacleFrequencyMultiplier,
                        enemyHealth = enemyHealthMultiplier,
                        rewardFrequency = rewardFrequencyMultiplier
                    }, 
                    SendMessageOptions.DontRequireReceiver);
            }
        }
        
        public void OnPlayerDeath()
        {
            RecordPlayerPerformance(0.1f); // Poor performance
            currentStreak = 0;
        }
        
        public void OnPlayerSuccess()
        {
            RecordPlayerPerformance(0.9f); // Good performance
            currentStreak++;
        }
        
        public void OnObstacleAvoid(float reactionTime)
        {
            averageReactionTime = Mathf.Lerp(averageReactionTime, reactionTime, 0.2f);
            float performanceScore = Mathf.InverseLerp(2f, 0.5f, reactionTime); // Better reaction = higher score
            RecordPlayerPerformance(performanceScore);
        }
        
        [System.Serializable]
        public struct DifficultyData
        {
            public float obstacleFrequency;
            public float enemyHealth;
            public float rewardFrequency;
        }
    }
}