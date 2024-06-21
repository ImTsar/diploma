using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GW_1
{
    public class SystolicSystemUnit
    {
        public List<ProcessorElementUnit> ProcessorElements { get; set; }
        private readonly Dictionary<int, string> peTexts = new Dictionary<int, string>();
        public List<DataFlowConnector> Connectors { get; set; }

        public SystolicSystemUnit(List<ProcessorElementUnit> processorElements, List<DataFlowConnector> connectors)
        {
            ProcessorElements = processorElements;
            Connectors = connectors;
        }

        public void DoTact(Control control, string topologyName)
        {
            ConnectPEs(ProcessorElements);
            TableLayoutPanel tableLayoutPanel = FindTableLayoutPanel(control, topologyName);
            UpdateOperationPanel(ProcessorElements, tableLayoutPanel, GetPEPanelName(topologyName));

            Panel panel = FindControlRecursive(control, "panel4") as Panel;
            var generatedValues = GenerateValues();

            UpdateRichTextBox(control, "richTextBox5", generatedValues, "Вхідні потоки даних");
            UpdateRichTextBox(control, "richTextBox6", generatedValues, "Обчислені вихідні потоки даних");

            RichTextBox richTextBox7 = FindControlRecursive(control, "richTextBox7") as RichTextBox;
            if (richTextBox7 != null)
            {
                UpdateRichTextBox7(richTextBox7, ProcessorElements, panel);
            }
        }

        private TableLayoutPanel FindTableLayoutPanel(Control control, string topologyName)
        {
            string layoutPanelName = topologyName == "square" ? "tableLayoutPanel3" : "PEsTableLayoutPanel";
            return FindControlRecursive(control, layoutPanelName) as TableLayoutPanel;
        }

        private string GetPEPanelName(string topologyName)
        {
            return topologyName == "square" ? "operatPanelPE" : "operationsPanelPE";
        }

        private List<Dictionary<string, List<double>>> GenerateValues()
        {
            var generatedValues = new List<Dictionary<string, List<double>>>();

            foreach (var processorElement in ProcessorElements)
            {
                processorElement.Calculate();
                generatedValues.Add(processorElement.GeneratedValues);
            }

            return generatedValues;
        }

        private void UpdateRichTextBox(Control control, string richTextBoxName, List<Dictionary<string, List<double>>> generatedValues, string dataType)
        {
            RichTextBox richTextBox = FindControlRecursive(control, richTextBoxName) as RichTextBox;
            if (richTextBox != null)
            {
                richTextBox.Clear();
                richTextBox.WordWrap = false;
                richTextBox.ScrollBars = RichTextBoxScrollBars.Both;

                for (int i = 0; i < ProcessorElements.Count; i++)
                {
                    var processorElement = ProcessorElements[i];
                    var data = dataType == "Вхідні потоки даних" ? processorElement.GeneratedValues : processorElement.Outputs;
                    AppendRichTextBoxData(richTextBox, processorElement, data);
                }
            }
        }

        private void AppendRichTextBoxData(RichTextBox richTextBox, ProcessorElementUnit processorElement, Dictionary<string, List<double>> data)
        {
            richTextBox.AppendText($"ПЕ{processorElement.Id}:\n");

            if (data.Count > 0)
            {
                foreach (var kvp in data)
                {
                    richTextBox.AppendText($"  Назва: {kvp.Key};\n  Значення: {string.Join(", ", kvp.Value)}\n");
                }
                richTextBox.AppendText("\n");
            }
            else
            {
                richTextBox.AppendText("  Назва: -;\n  Значення: -\n\n");
            }
        }

        public void UpdateOperationPanel(List<ProcessorElementUnit> processorElements, TableLayoutPanel tableLayoutPanel, string PEPanelName)
        {
            for (int row = 0; row < tableLayoutPanel.RowCount; row++)
            {
                for (int col = 0; col < tableLayoutPanel.ColumnCount; col++)
                {
                    Panel generalPanel = tableLayoutPanel.GetControlFromPosition(col, row) as Panel;
                    if (generalPanel != null)
                    {
                        UpdateGeneralPanel(generalPanel, row, col, processorElements, tableLayoutPanel.ColumnCount, PEPanelName);
                    }
                }
            }
        }

        private void UpdateGeneralPanel(Panel generalPanel, int row, int col, List<ProcessorElementUnit> processorElements, int columnCount, string PEPanelName)
        {
            foreach (Control child in generalPanel.Controls)
            {
                if (child is Panel processorPanel)
                {
                    foreach (Control nestedChild in processorPanel.Controls)
                    {
                        if (nestedChild is Panel operationsPanel && operationsPanel.Name.Contains(PEPanelName))
                        {
                            UpdateOperationsPanel(operationsPanel, row, col, processorElements, columnCount);
                        }
                    }
                }
            }
        }

        private void UpdateOperationsPanel(Panel operationsPanel, int row, int col, List<ProcessorElementUnit> processorElements, int columnCount)
        {
            operationsPanel.Controls.Clear();
            operationsPanel.AutoScroll = true;

            int index = (columnCount - 1) * row + col;
            if (col == (columnCount - 1))
                index -= 1;

            if (index < processorElements.Count || row == (columnCount - 1))
            {
                if (row == (columnCount - 1))
                    index -= (columnCount - 1);

                var processorElement = processorElements[index];
                TextBox textBox = CreateTextBox();

                StringBuilder text = GenerateProcessorElementText(processorElement);
                textBox.Text = text.ToString().TrimEnd('\r', '\n');
                peTexts[processorElement.Id] = $"Такт {processorElement.CurrentTact}\n{text}.";
                operationsPanel.Controls.Add(textBox);
            }
        }

        private TextBox CreateTextBox()
        {
            return new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 12, FontStyle.Bold),
                ReadOnly = true,
                WordWrap = false,
                ScrollBars = ScrollBars.Vertical,
                TextAlign = HorizontalAlignment.Left,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
        }

        private StringBuilder GenerateProcessorElementText(ProcessorElementUnit processorElement)
        {
            StringBuilder text = new StringBuilder();

            foreach (var input in processorElement.Inputs.OrderBy(kv => kv.Key))
            {
                int currentTact = processorElement.CurrentTact == 0 ? processorElement.CurrentTact : processorElement.CurrentTact - 1;
                text.AppendLine($"{input.Key} = {input.Value.ElementAt(currentTact)};");
            }

            foreach (var output in processorElement.Outputs.OrderBy(kv => kv.Key))
            {
                text.AppendLine($"{output.Key} = {output.Value.Last()};");
            }

            return text;
        }

        public void UpdateRichTextBox7(RichTextBox richTextBox7, List<ProcessorElementUnit> processorElements, Panel panel4)
        {
            if (richTextBox7 != null)
            {
                richTextBox7.Font = new Font("Arial", 16, FontStyle.Bold);

                var formulas = GetOutputFormulasFromGroupBox3(panel4);
                var sortedKeys = peTexts.Keys.OrderBy(key => key).ToList();

                foreach (var key in sortedKeys)
                {
                    AppendRichTextBox7Text(richTextBox7, processorElements[key], formulas);
                }
            }
        }

        private void AppendRichTextBox7Text(RichTextBox richTextBox7, ProcessorElementUnit processorElement, Dictionary<string, List<string>> formulas)
        {
            StringBuilder peText = new StringBuilder();
            peText.AppendLine($"ПЕ{processorElement.Id}, Такт {processorElement.CurrentTact}");

            foreach (var input in processorElement.Inputs.OrderBy(i => i.Key))
            {
                peText.AppendLine($"{input.Key} = {input.Value.ElementAt(processorElement.CurrentTact == 0 ? processorElement.CurrentTact : processorElement.CurrentTact - 1)};");
            }

            foreach (var output in processorElement.Outputs.OrderBy(o => o.Key))
            {
                AppendOutputText(peText, processorElement, output, formulas);
            }

            richTextBox7.AppendText(peText.ToString().TrimEnd('\r', '\n') + "\n\n");
        }

        private void AppendOutputText(StringBuilder peText, ProcessorElementUnit processorElement, KeyValuePair<string, List<double>> output, Dictionary<string, List<string>> formulas)
        {
            if (formulas.TryGetValue(output.Key, out List<string> formulaList))
            {
                foreach (var formula in formulaList)
                {
                    string substitutedFormula = formula.Substring(formula.IndexOf('=') + 1).Trim().Replace(";", "");

                    if (substitutedFormula.Length > 1)
                    {
                        foreach (var input in processorElement.Inputs)
                        {
                            string inputValue = input.Value.ElementAt(processorElement.CurrentTact == 0 ? processorElement.CurrentTact : processorElement.CurrentTact - 1).ToString();
                            substitutedFormula = substitutedFormula.Replace(input.Key, inputValue);
                        }

                        peText.AppendLine($"{output.Key} = {formula.Substring(formula.IndexOf('=') + 1).Trim().Replace(";", "")} = {substitutedFormula} = {output.Value.Last()};");
                    }
                    else
                    {
                        peText.AppendLine($"{output.Key} = {formula.Substring(formula.IndexOf('=') + 1).Trim().Replace(";", "")} = {output.Value.Last()};");
                    }
                }
            }
            else
            {
                peText.AppendLine($"{output.Key} = {output.Value.Last()};");
            }
        }

        private Dictionary<string, List<string>> GetOutputFormulasFromGroupBox3(Panel panel4)
        {
            var formulas = new Dictionary<string, List<string>>();

            foreach (Control control in panel4.Controls)
            {
                if (control is RichTextBox richTextBox && control.Enabled)
                {
                    ExtractFormulasFromRichTextBox(richTextBox, formulas);
                }
            }

            return formulas;
        }

        private void ExtractFormulasFromRichTextBox(RichTextBox richTextBox, Dictionary<string, List<string>> formulas)
        {
            string[] lines = richTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                if (line.Contains("="))
                {
                    string outputName = line.Split('=')[0].Trim();
                    if (outputName.EndsWith("'"))
                    {
                        if (!formulas.ContainsKey(outputName))
                        {
                            formulas[outputName] = new List<string>();
                        }
                        formulas[outputName].Add(line);
                    }
                }
            }
        }

        public Control FindControlRecursive(Control root, string name)
        {
            if (root.Name == name)
            {
                return root;
            }

            foreach (Control child in root.Controls)
            {
                Control found = FindControlRecursive(child, name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        public void ConnectPEs(List<ProcessorElementUnit> processorElements)
        {
            foreach (var processorElement in processorElements)
            {
                ConnectPE(processorElement);
            }
        }

        public void ConnectPE(ProcessorElementUnit currentElement)
        {
            foreach (var connector in Connectors)
            {
                string threadName = connector.DataFlowName;
                string baseName = threadName.TrimEnd('\'');

                if (connector.FlowDirection == ConnectorDirection.ToRightPE && (currentElement.Id + 1) % 3 != 0)
                {
                    ConnectToRightPE(currentElement, baseName, threadName);
                }
                else if (connector.FlowDirection == ConnectorDirection.FromRightPE && (currentElement.Id + 1) % 3 != 0)
                {
                    ConnectFromRightPE(currentElement, baseName, threadName);
                }
                else if (connector.FlowDirection == ConnectorDirection.ToBottomPE)
                {
                    ConnectToBottomPE(currentElement, baseName, threadName);
                }
                else if (connector.FlowDirection == ConnectorDirection.FromBottomPE)
                {
                    ConnectFromBottomPE(currentElement, baseName, threadName);
                }
            }
        }

        private void ConnectToRightPE(ProcessorElementUnit currentElement, string baseName, string threadName)
        {
            var nextElement = ProcessorElements.FirstOrDefault(pe => pe.Id == currentElement.Id + 1);
            if (nextElement != null)
            {
                var output = currentElement.Outputs[threadName];
                if (nextElement.Inputs.ContainsKey(baseName))
                {
                    nextElement.Inputs[baseName] = output;
                }
                else
                {
                    nextElement.Inputs.Add(baseName, output);
                }
            }
        }

        private void ConnectFromRightPE(ProcessorElementUnit currentElement, string baseName, string threadName)
        {
            var nextElement = ProcessorElements.FirstOrDefault(pe => pe.Id == currentElement.Id + 1);
            if (nextElement != null)
            {
                baseName = threadName.EndsWith("'") ? threadName : threadName + "'";
                var output = nextElement.Outputs[baseName];
                if (currentElement.Inputs.ContainsKey(threadName))
                {
                    currentElement.Inputs[threadName] = output;
                }
                else
                {
                    currentElement.Inputs.Add(threadName, output);
                }
            }
        }

        private void ConnectToBottomPE(ProcessorElementUnit currentElement, string baseName, string threadName)
        {
            var nextElement = ProcessorElements.FirstOrDefault(pe => pe.Id == currentElement.Id + 3);
            if (nextElement != null)
            {
                var output = currentElement.Outputs[threadName];
                if (nextElement.Inputs.ContainsKey(baseName))
                {
                    nextElement.Inputs[baseName] = output;
                }
                else
                {
                    nextElement.Inputs.Add(baseName, output);
                }
            }
        }

        private void ConnectFromBottomPE(ProcessorElementUnit currentElement, string baseName, string threadName)
        {
            var nextElement = ProcessorElements.FirstOrDefault(pe => pe.Id == currentElement.Id + 3);
            if (nextElement != null)
            {
                baseName = threadName.EndsWith("'") ? threadName : threadName + "'";
                var output = nextElement.Outputs[baseName];
                if (currentElement.Inputs.ContainsKey(threadName))
                {
                    currentElement.Inputs[threadName] = output;
                }
                else
                {
                    currentElement.Inputs.Add(threadName, output);
                }
            }
        }
    }
}
