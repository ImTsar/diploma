using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace GW_1
{
    public class ProcessorElementUnit
    {
        public Dictionary<string, List<double>> GeneratedValues { get; private set; } = new Dictionary<string, List<double>>();
        public Dictionary<string, Func<Dictionary<string, double>, double>> OutputCalculations { get; private set; }
        public Dictionary<string, List<double>> Inputs { get; private set; }
        public Dictionary<string, List<double>> Outputs { get; private set; }
        public int CurrentTact { get; private set; }
        public int Id { get; private set; }
        public int NumberOfTacts { get; set; } = 21;

        public ProcessorElementUnit(
            int id,
            Dictionary<string, Func<Dictionary<string, double>, double>> outputCalculations,
            Dictionary<string, List<double>> inputs)
        {
            Id = id;
            OutputCalculations = outputCalculations;
            Inputs = inputs;
            Outputs = InitializeOutputs(outputCalculations);

            InitializeInputs(inputs);
        }

        private Dictionary<string, List<double>> InitializeOutputs(Dictionary<string, Func<Dictionary<string, double>, double>> outputCalculations)
        {
            var outputs = new Dictionary<string, List<double>>();
            foreach (var outputCalculation in outputCalculations)
            {
                outputs.Add(outputCalculation.Key, new List<double> { 0 });
            }
            return outputs;
        }

        private void InitializeInputs(Dictionary<string, List<double>> inputs)
        {
            foreach (var key in inputs.Keys.ToList())
            {
                inputs[key].Insert(0, 0.0);
            }
        }

        public void Calculate()
        {
            var currentValues = GetCurrentValues();
            UpdateOutputs(currentValues);
            UpdateGeneratedValuesFromInputs();
            CurrentTact++;
        }

        private Dictionary<string, double> GetCurrentValues()
        {
            return Inputs
                .Where(x => x.Value.Count > CurrentTact)
                .ToDictionary(x => x.Key, x => x.Value[CurrentTact]);
        }

        private void UpdateOutputs(Dictionary<string, double> currentValues)
        {
            foreach (var outputCalculation in OutputCalculations)
            {
                Outputs[outputCalculation.Key].Add(outputCalculation.Value(currentValues));
            }
        }

        private void UpdateGeneratedValuesFromInputs()
        {
            foreach (var inputPair in Inputs)
            {
                if (inputPair.Value.Count == NumberOfTacts)
                {
                    GeneratedValues[inputPair.Key] = new List<double>(inputPair.Value);
                }
            }
        }
    }
}
