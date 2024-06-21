using System;
using System.Collections.Generic;
using System.Linq;

namespace GW_1
{
    public class ProcessorElementBuilder
    {
        private Random _random = new Random();
        public int Indexer { get; private set; } = 0;

        public ProcessorElementUnit CreateProcessorElement(ProcessorElementCreationInfo info)
        {
            var inputs = new Dictionary<string, List<double>>();
            if (info.InputsThreadsInfo != null && info.InputsThreadsInfo.Any())
            {
                info.InputsThreadsInfo.ForEach(x =>
                {
                    var generatedData = GenerateData();
                    inputs.Add(x, generatedData);
                });
            }

            var outputs = new Dictionary<string, Func<Dictionary<string, double>, double>>();
            foreach (var item in info.OutputFormulas)
            {
                outputs.Add(item.Key, FormulaTransformator.Transform(item.Value));
            }

            return new ProcessorElementUnit(Indexer++, outputs, inputs);
        }

        private List<double> GenerateData(int count = 20)
        {
            var randomNumbers = new List<double>(count);
            int min = 0;
            int max = 10;

            for (int i = 0; i < count; i++)
            {
                var randomNumber = _random.Next(min, max);
                randomNumbers.Add(randomNumber);
            }

            return randomNumbers;
        }
    }

    public class ProcessorElementCreationInfo
    {
        public List<string> InputsThreadsInfo { get; set; }
        public Dictionary<string, string> OutputFormulas { get; set; }
    }
}
