using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GW_1
{
    internal class LanguageDictionary
    {
        public Dictionary<string, Dictionary<string, string>> Translations { get; set; }

        public static LanguageDictionary LoadFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                string json = File.ReadAllText(filePath);
                var languageDictionary = JsonConvert.DeserializeObject<LanguageDictionary>(json);

                if (languageDictionary == null || languageDictionary.Translations == null)
                {
                    throw new Exception("Deserialized language dictionary is null");
                }

                return languageDictionary;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language dictionary: {ex.Message}");
                return null;
            }
        }
    }
}
