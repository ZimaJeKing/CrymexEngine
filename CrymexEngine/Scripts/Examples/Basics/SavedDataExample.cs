using OpenTK.Mathematics;

namespace CrymexEngine.Scripts.Examples
{
    public class SavedDataExample : ScriptableBehaviour
    {
        protected override void Load()
        {
            // Save some data
            SavedData.WriteString("PlayerName", "Ben Dover");
            SavedData.WriteFloat("PlayerScore", 100f);
            SavedData.WriteVector3("PlayerPosition", new Vector3(1.0f, 2.0f, 3.0f));
            SavedData.WriteColorArray("PlayerSkin", [Color4.Blue, Color4.Red, Color4.Yellow]);

            // Retrieve the data
            string? playerName = SavedData.ReadString("PlayerName");
            float? playerScore = SavedData.ReadFloat("PlayerScore");
            Vector3? playerPosition = SavedData.ReadVector3("PlayerPosition");
            Color4[]? playerSkin = SavedData.ReadColorArray("PlayerSkin");

            // Print the data to the console
            Debug.Log($"Player Name: {playerName}");
            Debug.Log($"Player Score: {playerScore}");
            Debug.Log($"Player Position: {playerPosition}");
            
            Debug.Log("Player Skin: ");
            if (playerSkin != null)
                foreach (var color in playerSkin)
                    Debug.Log(color.ToString() + " ");
        }

        protected override void Update()
        {
        }
    }
}
