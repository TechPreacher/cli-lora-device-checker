namespace cli_lora_device_checker
{
    using System;

    public static class Keygen
    {
        public static string Generate(int keyLength)
        {
            var randomKey = string.Empty;
            var rnd = new Random();

            for (int i = 0; i < keyLength; i++)
            { 
                var newKey = rnd.Next(0, 255);
                randomKey += newKey.ToString("X2");
            }

            return randomKey;
        }
    }
}
