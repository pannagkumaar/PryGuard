using System;

namespace PryGuard.Core.ChromeApi.Settings
{
    public enum EChromeLanguage
    {
        Ru,
        EnUsa,
        EnGb,
        Sw,
        De,
        Fr,
        It,
        Kz,
    }
    
    public static class EChromeLanguageExtensions
    {
        private static Random random = new Random();

        // Method to get a random language
        public static EChromeLanguage GetRandomLanguage()
        {
            Array values = Enum.GetValues(typeof(EChromeLanguage));
            int randomIndex = random.Next(values.Length);
            return (EChromeLanguage)values.GetValue(randomIndex);
        }

        // Method to get a specific enum value by index
        public static EChromeLanguage GetValue(int index)
        {
            Array values = Enum.GetValues(typeof(EChromeLanguage));
            if (index < 0 || index >= values.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range for the enum values.");
            }
            return (EChromeLanguage)values.GetValue(index);
        }

        
    }
}
