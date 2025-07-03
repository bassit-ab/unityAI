using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace RunnerGame.MCP
{
    public class MCPProceduralLevelGenerator : MonoBehaviour
    {
        [Header("Generation Parameters")]
        public GameObject[] obstaclePrefabs;
        public GameObject[] enemyPrefabs;
        public GameObject[] boosterPrefabs;
        public GameObject[] environmentPrefabs;
        
        [Header("AI Pattern Recognition")]
        public List<LevelPattern> learnedPatterns = new List<LevelPattern>();
        public int maxPatternHistory = 50;
        
        [Header("Generation Rules")]
        public float minObstacleSpacing = 5f;
        public float maxObstacleSpacing = 15f;
        public float difficultySpikeProbability = 0.1f;
        public float recoveryZoneProbability = 0.15f;
        
        private MCPDifficultyManager difficultyManager;
        private Queue<Vector3> recentObstaclePositions = new Queue<Vector3>();
        private Dictionary<string, float> patternSuccessRates = new Dictionary<string, float>();
        
        private void Start()
        {
            difficultyManager = MCPDifficultyManager.Instance;
            LoadLearnedPatterns();
        }
        
        public void GenerateLevelSegment(Vector3 startPosition, float segmentLength)
        {
            float currentPosition = startPosition.z;
            float endPosition = startPosition.z + segmentLength;
            
            while (currentPosition < endPosition)
            {
                LevelChunk chunk = GenerateIntelligentChunk(currentPosition);
                SpawnChunk(chunk);
                
                currentPosition += chunk.length;
            }
        }
        
        private LevelChunk GenerateIntelligentChunk(float position)
        {
            LevelChunk chunk = new LevelChunk();
            chunk.startPosition = position;
            
            float playerStress = CalculatePlayerStress();
            float difficultyMultiplier = difficultyManager?.obstacleFrequencyMultiplier ?? 1f;
            
            if (playerStress > 0.8f && Random.value < recoveryZoneProbability)
            {
                chunk = GenerateRecoveryChunk(position);
            }
            else if (playerStress < 0.3f && Random.value < difficultySpikeProbability)
            {
                chunk = GenerateChallengeChunk(position, difficultyMultiplier);
            }
            else
            {
                chunk = GenerateBalancedChunk(position, difficultyMultiplier);
            }
            
            RecordPatternGeneration(chunk);
            return chunk;
        }
        
        private LevelChunk GenerateRecoveryChunk(float position)
        {
            LevelChunk chunk = new LevelChunk();
            chunk.startPosition = position;
            chunk.length = Random.Range(15f, 25f);
            
            for (int i = 0; i < Random.Range(2, 4); i++)
            {
                GameObject booster = boosterPrefabs[Random.Range(0, boosterPrefabs.Length)];
                Vector3 boosterPos = new Vector3(
                    Random.Range(-3f, 3f),
                    0.5f,
                    position + i * 8f
                );
                chunk.elements.Add(new ChunkElement { prefab = booster, position = boosterPos });
            }
            
            return chunk;
        }
        
        private LevelChunk GenerateChallengeChunk(float position, float difficultyMultiplier)
        {
            LevelChunk chunk = new LevelChunk();
            chunk.startPosition = position;
            chunk.length = Random.Range(20f, 35f);
            
            GenerateDenseObstaclePattern(chunk, difficultyMultiplier);
            return chunk;
        }
        
        private LevelChunk GenerateBalancedChunk(float position, float difficultyMultiplier)
        {
            LevelChunk chunk = new LevelChunk();
            chunk.startPosition = position;
            chunk.length = Random.Range(25f, 40f);
            
            float currentPos = position;
            while (currentPos < position + chunk.length)
            {
                float spacing = Mathf.Lerp(maxObstacleSpacing, minObstacleSpacing, difficultyMultiplier);
                spacing *= Random.Range(0.8f, 1.2f);
                
                if (Random.value < 0.7f)
                {
                    GameObject obstacle = SelectOptimalObstacle(currentPos);
                    Vector3 obstaclePos = new Vector3(
                        Random.Range(-2f, 2f),
                        0f,
                        currentPos
                    );
                    chunk.elements.Add(new ChunkElement { prefab = obstacle, position = obstaclePos });
                }
                
                if (Random.value < 0.3f)
                {
                    GameObject booster = boosterPrefabs[Random.Range(0, boosterPrefabs.Length)];
                    Vector3 boosterPos = new Vector3(
                        Random.Range(-2f, 2f),
                        0.5f,
                        currentPos + 2f
                    );
                    chunk.elements.Add(new ChunkElement { prefab = booster, position = boosterPos });
                }
                
                currentPos += spacing;
            }
            
            return chunk;
        }
        
        private GameObject SelectOptimalObstacle(float position)
        {
            Vector3 lastObstaclePos = recentObstaclePositions.Count > 0 ? 
                recentObstaclePositions.Last() : Vector3.zero;
            
            float distanceFromLast = position - lastObstaclePos.z;
            
            if (distanceFromLast < 10f)
            {
                return obstaclePrefabs[0];
            }
            else
            {
                return obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            }
        }
        
        private void SpawnChunk(LevelChunk chunk)
        {
            foreach (var element in chunk.elements)
            {
                GameObject spawned = Instantiate(element.prefab, element.position, Quaternion.identity);
                spawned.transform.SetParent(transform);
                
                if (element.prefab.name.Contains("Obstacle"))
                {
                    recentObstaclePositions.Enqueue(element.position);
                    if (recentObstaclePositions.Count > 10)
                    {
                        recentObstaclePositions.Dequeue();
                    }
                }
            }
        }
        
        private float CalculatePlayerStress()
        {
            if (difficultyManager == null) return 0.5f;
            
            float avgPerformance = difficultyManager.recentPerformance.Count > 0 ?
                difficultyManager.recentPerformance.Average() : 0.5f;
            
            float reactionStress = Mathf.InverseLerp(0.5f, 2f, difficultyManager.averageReactionTime);
            float streakStress = difficultyManager.currentStreak > 10 ? 0.2f : 0.8f;
            
            return (1f - avgPerformance) * 0.5f + reactionStress * 0.3f + streakStress * 0.2f;
        }
        
        private void RecordPatternGeneration(LevelChunk chunk)
        {
            LevelPattern pattern = new LevelPattern
            {
                id = System.Guid.NewGuid().ToString(),
                elementCount = chunk.elements.Count,
                spacing = CalculateAverageSpacing(chunk)
            };
            
            learnedPatterns.Add(pattern);
            
            if (learnedPatterns.Count > maxPatternHistory)
            {
                learnedPatterns.RemoveAt(0);
            }
        }
        
        private void GenerateDenseObstaclePattern(LevelChunk chunk, float difficultyMultiplier)
        {
            float density = 0.8f * difficultyMultiplier;
            float currentPos = chunk.startPosition;
            
            while (currentPos < chunk.startPosition + chunk.length)
            {
                if (Random.value < density)
                {
                    GameObject obstacle = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
                    Vector3 pos = new Vector3(Random.Range(-2f, 2f), 0f, currentPos);
                    chunk.elements.Add(new ChunkElement { prefab = obstacle, position = pos });
                }
                currentPos += Random.Range(3f, 8f);
            }
        }
        
        private float CalculateAverageSpacing(LevelChunk chunk)
        {
            if (chunk.elements.Count < 2) return 0f;
            
            float totalSpacing = 0f;
            for (int i = 1; i < chunk.elements.Count; i++)
            {
                totalSpacing += Vector3.Distance(chunk.elements[i-1].position, chunk.elements[i].position);
            }
            
            return totalSpacing / (chunk.elements.Count - 1);
        }
        
        private void LoadLearnedPatterns()
        {
            // Load patterns from save system
        }
        
        [System.Serializable]
        public class LevelChunk
        {
            public float startPosition;
            public float length;
            public List<ChunkElement> elements = new List<ChunkElement>();
        }
        
        [System.Serializable]
        public class ChunkElement
        {
            public GameObject prefab;
            public Vector3 position;
        }
        
        [System.Serializable]
        public class LevelPattern
        {
            public string id;
            public int elementCount;
            public float spacing;
        }
    }
} 