using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace RunnerGame.MCP
{
    public class MCPPlayerAnalytics : MonoBehaviour
    {
        [Header("Player Preferences")]
        public PlayerProfile currentProfile;
        public List<PlayerSession> sessionHistory = new List<PlayerSession>();
        
        [Header("Behavioral Analysis")]
        public float playstyleScore = 0.5f; // 0 = Cautious, 1 = Aggressive
        public float skillProgression = 0f;
        public string preferredDifficulty = "Medium";
        
        [Header("Personalization Settings")]
        public bool enablePersonalizedContent = true;
        public bool enableDynamicUI = true;
        public bool enableSmartRecommendations = true;
        
        private Dictionary<string, float> preferenceWeights = new Dictionary<string, float>();
        private List<PlayerAction> currentSessionActions = new List<PlayerAction>();
        private float sessionStartTime;
        
        public static MCPPlayerAnalytics Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeProfile();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            sessionStartTime = Time.time;
            StartNewSession();
        }
        
        private void InitializeProfile()
        {
            LoadPlayerProfile();
            InitializePreferenceWeights();
        }
        
        private void LoadPlayerProfile()
        {
            currentProfile = new PlayerProfile
            {
                playerId = SystemInfo.deviceUniqueIdentifier,
                totalPlayTime = PlayerPrefs.GetFloat("TotalPlayTime", 0f),
                highScore = PlayerPrefs.GetInt("HighScore", 0),
                preferredControlScheme = PlayerPrefs.GetString("PreferredControls", "Touch"),
                favoriteTheme = PlayerPrefs.GetString("FavoriteTheme", "Default")
            };
            
            playstyleScore = PlayerPrefs.GetFloat("PlaystyleScore", 0.5f);
            skillProgression = PlayerPrefs.GetFloat("SkillProgression", 0f);
        }
        
        private void InitializePreferenceWeights()
        {
            preferenceWeights["speed"] = PlayerPrefs.GetFloat("Pref_Speed", 0.5f);
            preferenceWeights["obstacles"] = PlayerPrefs.GetFloat("Pref_Obstacles", 0.5f);
            preferenceWeights["collectibles"] = PlayerPrefs.GetFloat("Pref_Collectibles", 0.5f);
            preferenceWeights["combat"] = PlayerPrefs.GetFloat("Pref_Combat", 0.5f);
        }
        
        private void StartNewSession()
        {
            PlayerSession session = new PlayerSession
            {
                sessionId = System.Guid.NewGuid().ToString(),
                startTime = System.DateTime.Now,
                initialSkillLevel = skillProgression
            };
            
            sessionHistory.Add(session);
            currentSessionActions.Clear();
        }
        
        public void RecordPlayerAction(string actionType, Vector3 position, float reactionTime = 0f)
        {
            PlayerAction action = new PlayerAction
            {
                actionType = actionType,
                timestamp = Time.time - sessionStartTime,
                position = position,
                reactionTime = reactionTime,
                success = true // Can be set based on outcome
            };
            
            currentSessionActions.Add(action);
            AnalyzeActionPattern(action);
        }
        
        private void AnalyzeActionPattern(PlayerAction action)
        {
            switch (action.actionType)
            {
                case "obstacle_avoid":
                    if (action.reactionTime < 0.8f)
                    {
                        playstyleScore = Mathf.Lerp(playstyleScore, 0.8f, 0.1f); // More aggressive
                        skillProgression += 0.01f;
                    }
                    else
                    {
                        playstyleScore = Mathf.Lerp(playstyleScore, 0.3f, 0.1f); // More cautious
                    }
                    break;
                    
                case "collectible_pickup":
                    preferenceWeights["collectibles"] = Mathf.Min(1f, preferenceWeights["collectibles"] + 0.05f);
                    break;
                    
                case "enemy_combat":
                    preferenceWeights["combat"] = Mathf.Min(1f, preferenceWeights["combat"] + 0.03f);
                    if (action.success)
                    {
                        playstyleScore = Mathf.Lerp(playstyleScore, 0.9f, 0.15f);
                        skillProgression += 0.02f;
                    }
                    break;
                    
                case "speed_boost":
                    preferenceWeights["speed"] = Mathf.Min(1f, preferenceWeights["speed"] + 0.04f);
                    playstyleScore = Mathf.Lerp(playstyleScore, 0.7f, 0.1f);
                    break;
            }
            
            // Update preferred difficulty based on performance
            UpdatePreferredDifficulty();
        }
        
        private void UpdatePreferredDifficulty()
        {
            float avgSuccess = currentSessionActions.Count > 0 ? 
                currentSessionActions.Count(a => a.success) / (float)currentSessionActions.Count : 0.5f;
            
            if (avgSuccess > 0.8f && skillProgression > 0.7f)
            {
                preferredDifficulty = "Hard";
            }
            else if (avgSuccess < 0.4f || skillProgression < 0.3f)
            {
                preferredDifficulty = "Easy";
            }
            else
            {
                preferredDifficulty = "Medium";
            }
        }
        
        public PersonalizationRecommendations GetPersonalizedRecommendations()
        {
            PersonalizationRecommendations recommendations = new PersonalizationRecommendations();
            
            // Recommend themes based on playstyle
            if (playstyleScore > 0.7f)
            {
                recommendations.recommendedTheme = "Action"; // High-energy theme
                recommendations.recommendedMusicTempo = "Fast";
            }
            else if (playstyleScore < 0.3f)
            {
                recommendations.recommendedTheme = "Zen"; // Calm theme
                recommendations.recommendedMusicTempo = "Slow";
            }
            else
            {
                recommendations.recommendedTheme = "Adventure";
                recommendations.recommendedMusicTempo = "Medium";
            }
            
            // Recommend difficulty adjustments
            recommendations.difficultyMultiplier = GetOptimalDifficultyMultiplier();
            
            // Recommend power-ups based on preferences
            recommendations.priorityPowerUps = GetPreferredPowerUps();
            
            // UI customization recommendations
            if (enableDynamicUI)
            {
                recommendations.uiScale = GetOptimalUIScale();
                recommendations.controlSensitivity = GetOptimalSensitivity();
            }
            
            return recommendations;
        }
        
        private float GetOptimalDifficultyMultiplier()
        {
            float baseMultiplier = 1f;
            
            // Adjust based on skill progression
            baseMultiplier *= (0.8f + skillProgression * 0.4f);
            
            // Adjust based on recent performance
            if (currentSessionActions.Count > 5)
            {
                float recentSuccess = currentSessionActions.TakeLast(5).Count(a => a.success) / 5f;
                if (recentSuccess > 0.8f)
                {
                    baseMultiplier *= 1.2f; // Increase challenge
                }
                else if (recentSuccess < 0.4f)
                {
                    baseMultiplier *= 0.8f; // Decrease challenge
                }
            }
            
            return Mathf.Clamp(baseMultiplier, 0.5f, 2f);
        }
        
        private List<string> GetPreferredPowerUps()
        {
            List<string> powerUps = new List<string>();
            
            if (preferenceWeights["speed"] > 0.6f)
                powerUps.Add("SpeedBoost");
            
            if (preferenceWeights["combat"] > 0.6f)
                powerUps.Add("DamageBoost");
            
            if (preferenceWeights["collectibles"] > 0.6f)
                powerUps.Add("CoinMagnet");
            
            if (playstyleScore < 0.4f) // Cautious players
                powerUps.Add("Shield");
            
            return powerUps;
        }
        
        private float GetOptimalUIScale()
        {
            // Adjust UI scale based on device and player behavior
            float baseScale = 1f;
            
            if (Application.isMobilePlatform)
            {
                baseScale = 1.2f; // Larger for mobile
            }
            
            // Adjust based on reaction times
            float avgReactionTime = currentSessionActions.Where(a => a.reactionTime > 0)
                .Average(a => a.reactionTime);
            
            if (avgReactionTime > 1.5f) // Slower reactions
            {
                baseScale *= 1.15f; // Larger UI elements
            }
            
            return baseScale;
        }
        
        private float GetOptimalSensitivity()
        {
            float baseSensitivity = 1f;
            
            // Adjust based on playstyle
            if (playstyleScore > 0.7f) // Aggressive players
            {
                baseSensitivity = 1.3f; // Higher sensitivity
            }
            else if (playstyleScore < 0.3f) // Cautious players
            {
                baseSensitivity = 0.8f; // Lower sensitivity
            }
            
            return baseSensitivity;
        }
        
        public void EndSession()
        {
            if (sessionHistory.Count > 0)
            {
                PlayerSession currentSession = sessionHistory.Last();
                currentSession.endTime = System.DateTime.Now;
                currentSession.finalSkillLevel = skillProgression;
                currentSession.actionsPerformed = currentSessionActions.Count;
                
                // Update total play time
                currentProfile.totalPlayTime += (float)(currentSession.endTime - currentSession.startTime).TotalMinutes;
                
                SavePlayerProfile();
            }
        }
        
        private void SavePlayerProfile()
        {
            PlayerPrefs.SetFloat("TotalPlayTime", currentProfile.totalPlayTime);
            PlayerPrefs.SetInt("HighScore", currentProfile.highScore);
            PlayerPrefs.SetString("PreferredControls", currentProfile.preferredControlScheme);
            PlayerPrefs.SetString("FavoriteTheme", currentProfile.favoriteTheme);
            
            PlayerPrefs.SetFloat("PlaystyleScore", playstyleScore);
            PlayerPrefs.SetFloat("SkillProgression", skillProgression);
            
            foreach (var pref in preferenceWeights)
            {
                PlayerPrefs.SetFloat($"Pref_{pref.Key}", pref.Value);
            }
            
            PlayerPrefs.Save();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SavePlayerProfile();
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                SavePlayerProfile();
            }
        }
        
        [System.Serializable]
        public class PlayerProfile
        {
            public string playerId;
            public float totalPlayTime;
            public int highScore;
            public string preferredControlScheme;
            public string favoriteTheme;
        }
        
        [System.Serializable]
        public class PlayerSession
        {
            public string sessionId;
            public System.DateTime startTime;
            public System.DateTime endTime;
            public float initialSkillLevel;
            public float finalSkillLevel;
            public int actionsPerformed;
        }
        
        [System.Serializable]
        public class PlayerAction
        {
            public string actionType;
            public float timestamp;
            public Vector3 position;
            public float reactionTime;
            public bool success;
        }
        
        [System.Serializable]
        public class PersonalizationRecommendations
        {
            public string recommendedTheme;
            public string recommendedMusicTempo;
            public float difficultyMultiplier;
            public List<string> priorityPowerUps = new List<string>();
            public float uiScale;
            public float controlSensitivity;
        }
    }
} 