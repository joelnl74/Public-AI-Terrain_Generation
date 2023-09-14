namespace Models
{
    public class SettingsModel
    {
        // Initial parameters for all agents
        // These can be adjusted in the ingame UI

        // Coast agent
        public int borderSize = 20;
        
        // Beach agent
        public float beachMaxHeight = 10f;
        public float beachSealevel = 0.5f;
        public int numberOfBeaches = 6;
        public int inlandDistance = 5;
        
        // Mountain agent
        public int minAmountOfMountains = 9;
        public int maxAmountOfMountains = 12;

        public int maxHeight = 50;

        public int minLength = 300;
        public int maxLength = 500;

        public int mountainWidth = 50;
        
        // Hill agent
        public int minAmountOfHill = 10;
        public int maxAmountOfHill = 15;
        
        public int maxHillHeight = 40;
        
        public int minHillLength = 50;
        public int maxHillLength = 100;
        
        public int hillWidth = 150;
 
        // Noise agent
        public int randomNoiseGenerationPercentage = 10;

        public float randomNoiseMinHeight = 3.0f;
        public float randomNoiseMaxHeight = 10.0f;
        
        // Volcano agent
        public int calderaWidth = 5;
        public float calderaWidthRange = 2f;

        public int volcanoHeight = 70;
        public float volcanoHeightRange = 10;
        public int volcanoWidth = 50;
        
        public bool OneIsland = true;
    }
}
