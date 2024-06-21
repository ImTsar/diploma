using GroupBox = System.Windows.Forms.GroupBox;
using System.Windows.Forms.DataVisualization.Charting;

namespace GW_1
{
    public partial class Main : Form
    {
        private Dictionary<Button, Button> pairedButtons;
        private string lastClickedButtonName;
        private ImageLoader imageLoader;
        private ButtonInitializer buttonInitializer;
        private TableLayoutUpdater tableLayoutUpdater;
        private Image plusIcon;
        private Button processorArrowButton;
        private Image currentProcessorArrow;
        private Panel componentPanel;
        private ProcessorElementUI defaultProcessorElement = new ProcessorElementUI();
        private List<ProcessorElementUnit> processorElements;
        private SystolicSystemUnit systolicSystemUnit;
        private LanguageDictionary languageDictionary;
        private string currentLanguage = "en";

        public Main()
        {
            InitializeComponent();

            imageLoader = new ImageLoader();
            buttonInitializer = new ButtonInitializer(this, imageLoader);
            pairedButtons = buttonInitializer.InitializeButtons(PEDesignerTabPage, new[]
            {
                "VertLeftUpPanel", "VertLeftDownPanel", "VertRightUpPanel", "VertRightDownPanel",
                "HorizLeftUpPanel", "HorizRightUpPanel", "HorizLeftDownPanel", "HorizRightDownPanel"
            }, plusIcon);

            tableLayoutUpdater = new TableLayoutUpdater(this, defaultProcessorElement, pairedButtons, buttonInitializer, imageLoader, UpdateTextBox);
            
            LoadIcons();
            InitializeDirectionButtons();
            InitializeProcessorElement();
            FillChart(chart1);

            languageDictionary = LanguageDictionary.LoadFromFile("translations.json");
            ChangeLanguage(this); 
        }
        public void ChangeLanguage(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is Label label)
                {
                    label.Text = GetTranslation(label.Text);
                }
                else if (control.HasChildren)
                {
                    ChangeLanguage(control);
                }
            }
        }

        private void eNToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            ChangeLanguage("ua");
        }

        private void uAToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            ChangeLanguage("en");
        }

        private void ChangeLanguage(string languageCode)
        {
            currentLanguage = languageCode;
            var translations = languageDictionary.Translations[currentLanguage];
            var languageManager = new LanguageManager(translations);
            languageManager.ChangeLanguage(this);
            UpdateControlText(this, translations);
        }

        private void UpdateControlText(Control control, Dictionary<string, string> translations)
        {
            foreach (Control child in control.Controls)
            {
                if (child is Button button && translations.ContainsKey(button.Text))
                {
                    button.Text = translations[button.Text];
                }
                else if (child is Label label && translations.ContainsKey(label.Text))
                {
                    label.Text = translations[label.Text];
                }
                else if (child is MenuStrip menuStrip)
                {
                    foreach (ToolStripMenuItem item in menuStrip.Items)
                    {
                        if (translations.ContainsKey(item.Text))
                        {
                            item.Text = translations[item.Text];
                        }
                        UpdateToolStripItems(item, translations);
                    }
                }
                else if (child.Controls.Count > 0)
                {
                    UpdateControlText(child, translations);
                }
            }
        }

        public string GetTranslation(string key)
        {
            if (languageDictionary.Translations.ContainsKey(currentLanguage) &&
                languageDictionary.Translations[currentLanguage].TryGetValue(key, out string translation))
            {
                return translation;
            }
            return key;
        }

        private void UpdateToolStripItems(ToolStripItem item, Dictionary<string, string> translations)
        {
            if (item is ToolStripMenuItem menuItem)
            {
                foreach (ToolStripMenuItem subItem in menuItem.DropDownItems)
                {
                    if (translations.ContainsKey(subItem.Text))
                    {
                        subItem.Text = translations[subItem.Text];
                    }
                    UpdateToolStripItems(subItem, translations);
                }
            }
        }

        private void LoadIcons()
        {
            try
            {
                plusIcon = imageLoader.PlusIcon;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading images: {ex.Message}");
            }
        }

        public Image PlusIcon => plusIcon;

        public GroupBox PEGroupBoxControl => PEGroupBox;

        private void InitializeDirectionButtons()
        {
            pairedButtons = buttonInitializer.InitializeButtons(PEDesignerTabPage, new[]
            {
                "VertLeftUpPanel", "VertLeftDownPanel", "VertRightUpPanel", "VertRightDownPanel",
                "HorizLeftUpPanel", "HorizRightUpPanel", "HorizLeftDownPanel", "HorizRightDownPanel"
            }, plusIcon);
        }

        private void InitializeProcessorElement()
        {
            RoundedPanel processorElement = FindControlRecursive(this, "ProcessorElementPanel") as RoundedPanel;
            if (processorElement == null)
            {
                MessageBox.Show("ProcessorElement not found");
                return;
            }

            processorElement.CornerRadius = 50;

            componentPanel = new RoundedPanel
            {
                Size = new Size(150, 100),
                Location = new Point((processorElement.Width - 150) / 2, (processorElement.Height - 100) / 2),
                BackColor = Color.Beige,
                CornerRadius = 20,
                Visible = false
            };

            Label delayLabel = new Label
            {
                Text = "Затримка\nтактів:",
                Location = new Point(10, 10),
                Font = new Font("Arial", 10, FontStyle.Bold),
                AutoSize = true
            };

            TextBox delayTextBox = new TextBox
            {
                Location = new Point(15, 55),
                Width = 50,
                Text = "0",
                TextAlign = System.Windows.Forms.HorizontalAlignment.Center
            };

            delayTextBox.TextChanged += DelayTextBox_TextChanged;

            processorArrowButton = new Button
            {
                BackgroundImage = imageLoader.TopArrowIcon,
                Size = new Size(40, 40),
                Location = new Point(100, 30),
                BackgroundImageLayout = ImageLayout.Zoom,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                FlatAppearance = { BorderSize = 0 }
            };

            processorArrowButton.Click += ProcessorArrowButton_Click;

            componentPanel.Controls.Add(delayLabel);
            componentPanel.Controls.Add(delayTextBox);
            componentPanel.Controls.Add(processorArrowButton);

            processorElement.Controls.Add(componentPanel);

            if (buttonInitializer.HasAtLeastOneArrow())
            {
                componentPanel.Visible = true;
            }
        }

        private void FillChart(Chart chart)
        {
            chart.ChartAreas[0].AxisX.Title = "Розмір рухомого вікна";
            chart.ChartAreas[0].AxisX.TitleFont = new System.Drawing.Font("Arial", 10, FontStyle.Bold);
            chart.ChartAreas[0].AxisY.Title = "Кількість тактів";
            chart.ChartAreas[0].AxisY.TitleFont = new System.Drawing.Font("Arial", 10, FontStyle.Bold);

            chart.ChartAreas[0].AxisY.Minimum = 0;

            chart.ChartAreas[0].AxisX.Minimum = 3;
            chart.ChartAreas[0].AxisX.Maximum = 17;

            chart.ChartAreas[0].AxisX.MajorGrid.LineWidth = 2;
            chart.ChartAreas[0].AxisY.MajorGrid.LineWidth = 2;

            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Для послідовного виконання на одному ПЕ.",
                Color = System.Drawing.Color.Blue,
                ChartType = SeriesChartType.Line,
                BorderWidth = 3, 
                BorderDashStyle = ChartDashStyle.Dot 
            };

            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Для виконання на СС з декількома ПЕ.",
                Color = System.Drawing.Color.Green,
                ChartType = SeriesChartType.Line,
                BorderWidth = 3 
            };

            chart.Series.Clear();
            chart.Series.Add(series1);
            chart.Series.Add(series2);

            int n = 5; 

            for (int m = 1; m <= 8; m++)
            {
                int sizeOfMovingWindow = 2 * m + 1;
                int systolicSystemTacts = 2 * n + 4 * m - 1;
                int sequentialSystemTacts = (2 * m + 1) * n;

                series1.Points.AddXY(sizeOfMovingWindow, sequentialSystemTacts);
                series2.Points.AddXY(sizeOfMovingWindow, systolicSystemTacts);
            }

            chart.Legends.Clear();
            Legend legend = new Legend
            {
                Docking = Docking.Right,
                Font = new System.Drawing.Font("Arial", 10)
            };
            chart.Legends.Add(legend);
        }

        private void DelayTextBox_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse((sender as TextBox)?.Text, out int delay))
            {
                defaultProcessorElement.DelayTakts = delay;
            }
        }

        private void ProcessorArrowButton_Click(object sender, EventArgs e)
        {
            tableLayoutUpdater.HandleProcessorArrowButtonClick(lastClickedButtonName, processorArrowButton, ref currentProcessorArrow);
        }

        private void defaultPEButton_Click(object sender, EventArgs e)
        {
            componentPanel.Visible = false;
            defaultProcessorElement = new ProcessorElementUI();
            buttonInitializer.UpdateButtonsInControl(PEGroupBoxControl, plusIcon);
            HideAllTextBoxes();
            AnalyzeAndDisable();
        }

        private void HideAllTextBoxes()
        {
            foreach (Control control in PEGroupBoxControl.Controls)
            {
                if (control is TextBox textBox)
                {
                    textBox.Text = "";
                }
            }
        }

        private void stepBackButton_Click(object sender, EventArgs e)
        {
            componentPanel.Enabled = false;
            tableLayoutUpdater.HandleReturnButtonClick(lastClickedButtonName, plusIcon);
            ClearAndHideTextBoxes(lastClickedButtonName);
            AnalyzeAndDisable();
        }

        public void ClearAndHideTextBoxes(string buttonName)
        {
            string textBoxName = buttonName.Replace("Button", "TextBox");
            var textBox = FindControlRecursive(PEGroupBoxControl, textBoxName) as TextBox;

            if (textBox != null)
            {
                textBox.Text = string.Empty;
            }

            if (syncArrowRadioButton.Checked && pairedButtons.TryGetValue(FindControlRecursive(this, buttonName) as Button, out var pairedButton))
            {
                string pairedTextBoxName = pairedButton.Name.Replace("Button", "TextBox");
                var pairedTextBox = FindControlRecursive(PEGroupBoxControl, pairedTextBoxName) as TextBox;

                if (pairedTextBox != null)
                {
                    pairedTextBox.Text = string.Empty;
                    pairedTextBox.Visible = false;
                }
            }
        }

        public List<string> GetVisibleLabelsWithoutApostrophe(GroupBox PEGroupBox)
        {
            List<string> threadsNames = new List<string>();
            ProcessInputThreads(PEGroupBox, threadsNames);
            return threadsNames;
        }

        private void ProcessInputThreads(Control parent, List<string> threadsNames)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is TextBox textBox && textBox.Visible && IsEnglishAlphabetLetter(textBox.Text) && !textBox.Text.Contains("'"))
                {
                    threadsNames.Add(textBox.Text);
                }

                ProcessInputThreads(child, threadsNames);
            }
        }

        private bool IsEnglishAlphabetLetter(string text)
        {
            return text.Length == 1 && char.IsLetter(text[0]) && (text[0] >= 'A' && text[0] <= 'Z' || text[0] >= 'a' && text[0] <= 'z');
        }

        public List<string> GetVisibleLabelsTextBoxes(GroupBox PEGroupBox)
        {
            List<string> threadsNames = new List<string>();
            ProcessOutputThreads(PEGroupBox, threadsNames);
            return threadsNames;
        }

        private void ProcessOutputThreads(Control parent, List<string> threadsNames)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is TextBox textBox && textBox.Visible && textBox.Text.Contains("'"))
                {
                    threadsNames.Add(textBox.Text);
                }

                ProcessInputThreads(child, threadsNames);
            }
        }

        public Dictionary<string, string> GetOutputFormulas(Control groupBox)
        {
            Dictionary<string, string> outputFormulas = new Dictionary<string, string>();
            ProcessControls(groupBox, outputFormulas);
            return outputFormulas;
        }

        private void ProcessControls(Control parent, Dictionary<string, string> outputFormulas)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is RichTextBox textBox && textBox.Enabled)
                {
                    string text = textBox.Text;
                    int equalsIndex = text.IndexOf('=');
                    int semicolonIndex = text.IndexOf(';', equalsIndex);

                    if (equalsIndex != -1 && semicolonIndex != -1 && semicolonIndex > equalsIndex)
                    {
                        string key = text.Substring(0, equalsIndex).Trim();
                        string value = text.Substring(equalsIndex + 1, semicolonIndex - equalsIndex - 1).Trim();

                        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                        {
                            outputFormulas[key] = value;
                        }
                    }
                }

                ProcessControls(child, outputFormulas);
            }
        }

        private void linearButton_Click(object sender, EventArgs e)
        {
            GroupBox PEGroupBox = FindControlRecursive(this, "PEGroupBox") as GroupBox;
            GroupBox groupBox3 = FindControlRecursive(this, "groupBox3") as GroupBox;
            Panel panel4 = FindControlRecursive(this, "panel4") as Panel;

            if (!ValidateRichTextBoxValues(panel4))
            {
                return;
            }

            tableLayoutUpdater.UpdateTableLayoutPanel(PEsTableLayoutPanel, true, defaultProcessorElement);

            var connections = ProcessorElementManager.GetElementInterconnections(this);

            var builder = new ProcessorElementBuilder();

            List<string> inputsThreadsInfo = GetVisibleLabelsWithoutApostrophe(PEGroupBox);
            List<string> outputsThreadsInfo = GetVisibleLabelsTextBoxes(PEGroupBox);

            Dictionary<string, string> outputFormulas = GetOutputFormulas(groupBox3);

            var creationInfo1 = new ProcessorElementCreationInfo
            {
                InputsThreadsInfo = inputsThreadsInfo,
                OutputFormulas = outputFormulas
            };

            processorElements = new List<ProcessorElementUnit>();

            double countOfPE = Convert.ToInt32(numericUpDown1.Value);

            for (int i = 0; i < countOfPE; i++)
            {
                processorElements.Add(builder.CreateProcessorElement(creationInfo1));
            }

            systolicSystemUnit = new SystolicSystemUnit(processorElements, connections);

            x.SelectedIndex = 1;

            FillMatrixFlowLayoutPanels(PEsTableLayoutPanel, processorElements);
        }

        private void FillMatrixFlowLayoutPanels(TableLayoutPanel tableLayoutPanel, List<ProcessorElementUnit> processorElements)
        {
            int index = 0;
            for (int row = 0; row < tableLayoutPanel.RowCount; row++)
            {
                for (int col = 0; col < tableLayoutPanel.ColumnCount; col++)
                {
                    var control = tableLayoutPanel.GetControlFromPosition(col, row);
                    if (control is Panel panel)
                    {
                        foreach (Control child in panel.Controls)
                        {
                            if (child is FlowLayoutPanel flowLayoutPanel)
                            {
                                flowLayoutPanel.Controls.Clear();
                                flowLayoutPanel.WrapContents = false;

                                flowLayoutPanel.AutoScroll = true;

                                if (flowLayoutPanel.Tag != null && flowLayoutPanel.Tag.ToString() != "")
                                {
                                    string tag = flowLayoutPanel.Tag.ToString();

                                    var processorElement = processorElements.ElementAtOrDefault(index);
                                    if (processorElement != null)
                                    {
                                        if (processorElement.Inputs.ContainsKey(tag))
                                        {
                                            var inputValues = processorElement.Inputs[tag];
                                            if (inputValues != null && inputValues.Any())
                                            {
                                                IEnumerable<double> values;
                                                if (flowLayoutPanel.Name.Contains("RightTopArrow") ||
                                                        (flowLayoutPanel.Tag != null && flowLayoutPanel.Tag.ToString() == "RightTopArrowPathPE2" && RightTopArrowPathPE2.Tag.ToString() == "a") ||
                                                        flowLayoutPanel.Name.Contains("RightBottomArrow") ||
                                                        (flowLayoutPanel.Tag != null && flowLayoutPanel.Tag.ToString() == "RightBottomArrowPathPE2" && RightBottomArrowPathPE2.Tag.ToString() == "c"))
                                                {
                                                    values = inputValues;
                                                }
                                                else
                                                {
                                                    values = inputValues.AsEnumerable().Reverse();
                                                }

                                                foreach (var value in values)
                                                {
                                                    var label = new Label
                                                    {
                                                        Text = value.ToString(),
                                                        Font = new Font("Arial", 12, FontStyle.Bold),
                                                        Size = new Size(62, 29),
                                                        BackColor = Color.DarkSalmon,
                                                        Margin = new Padding(5),
                                                        Anchor = AnchorStyles.None,
                                                        BorderStyle = BorderStyle.FixedSingle,
                                                        TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                                                    };
                                                    flowLayoutPanel.Controls.Add(label);
                                                }
                                            }
                                        }

                                        if (processorElement.Outputs.ContainsKey(tag))
                                        {
                                            var outputValues = processorElement.Outputs[tag];
                                            if (outputValues != null && outputValues.Any())
                                            {
                                                var values = outputValues;
                                                foreach (var value in values)
                                                {
                                                    var label = new Label
                                                    {
                                                        Text = value.ToString(),
                                                        Font = new Font("Arial", 12, FontStyle.Bold),
                                                        Size = new Size(62, 29),
                                                        BackColor = Color.DarkSalmon,
                                                        Margin = new Padding(5),
                                                        Anchor = AnchorStyles.None,
                                                        BorderStyle = BorderStyle.FixedSingle,
                                                        TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                                                    };
                                                    flowLayoutPanel.Controls.Add(label);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        index++;
                    }

                    if (col == tableLayoutPanel.ColumnCount - 2)
                    {
                        index--;
                    }
                }
            }
        }
        private void squareButton_Click(object sender, EventArgs e)
        {
            GroupBox PEGroupBox = FindControlRecursive(this, "PEGroupBox") as GroupBox;
            GroupBox groupBox3 = FindControlRecursive(this, "groupBox3") as GroupBox;
            Panel panel4 = FindControlRecursive(this, "panel4") as Panel;

            if (!ValidateRichTextBoxValues(panel4))
            {
                return;
            }

            tableLayoutUpdater.UpdateTableLayoutPanel(tableLayoutPanel3, false, defaultProcessorElement);

            var connections = ProcessorElementManager.GetElementInterconnections(this);

            var builder = new ProcessorElementBuilder();

            List<string> inputsThreadsInfo = GetVisibleLabelsWithoutApostrophe(PEGroupBox);
            List<string> outputsThreadsInfo = GetVisibleLabelsTextBoxes(PEGroupBox);

            Dictionary<string, string> outputFormulas = GetOutputFormulas(groupBox3);

            var creationInfo1 = new ProcessorElementCreationInfo
            {
                InputsThreadsInfo = inputsThreadsInfo,
                OutputFormulas = outputFormulas
            };

            var processorElements = new List<ProcessorElementUnit>();

            double countOfPE = Math.Pow(Convert.ToInt32(numericUpDown1.Value), 2);

            for (int i = 0; i < countOfPE; i++)
            {
                processorElements.Add(builder.CreateProcessorElement(creationInfo1));
            }

            systolicSystemUnit = new SystolicSystemUnit(processorElements, connections);

            x.SelectedIndex = 2;
            systolicSystemUnit.DoTact(this, "square");
        }

        private bool ValidateRichTextBoxValues(Panel panel4)
        {
            HashSet<string> validValues = new HashSet<string>();
            foreach (Control control in PEGroupBox.Controls)
            {
                if (control is TextBox textBox && !string.IsNullOrEmpty(textBox.Text))
                {
                    validValues.Add(textBox.Text);
                }
            }

            foreach (Control control in panel4.Controls)
            {
                if (control is RichTextBox richTextBox && richTextBox.Enabled)
                {
                    string[] lines = richTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    foreach (string line in lines)
                    {
                        if (line.Contains("="))
                        {
                            string value = line.Substring(line.IndexOf('=') + 1).Replace(" ", "").Trim();

                            value = value.Replace(";", "").Trim();

                            string[] elements = value.Split(new[] { '+', '*' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string element in elements)
                            {
                                string invalidType = (element.Contains("/") || element.Contains("-") || element.Contains("&")) ? "operation" : "value";
                                if (!validValues.Contains(element) && !double.TryParse(element, out _))
                                {
                                    MessageBox.Show($"Invalid {invalidType} '{element}' in formula '{line}'", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return false; 
                                }
                            }
                        }
                    }
                }
            }
            return true; 
        }

        private void designerMenuButton_Click(object sender, EventArgs e)
        {
            x.SelectedIndex = 0;
        }

        private void chartMenuButton_Click(object sender, EventArgs e)
        {
            x.SelectedIndex = 3;
        }

        private void infoMenuButton_Click(object sender, EventArgs e)
        {
            x.SelectedIndex = 4;
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

        private void ShowComponentPanel(bool visible)
        {
            componentPanel.Visible = visible;
            componentPanel.Enabled = visible;
        }

        public void SetLastClickedButtonName(string name)
        {
            lastClickedButtonName = name;
        }

        public void SetArrow(Button button, string direction)
        {
            var arrowIcon = imageLoader.GetArrowIcon(direction);

            UpdateButtonImage(button, arrowIcon);
            UpdateProcessorElementDirection(button.Name, arrowIcon);

            if (syncArrowRadioButton.Checked && pairedButtons.TryGetValue(button, out var pairedButton))
            {
                UpdateButtonImage(pairedButton, arrowIcon);
                UpdateProcessorElementDirection(pairedButton.Name, arrowIcon);
            }

            ShowComponentPanel(true);
            processorArrowButton.BackgroundImage = imageLoader.GetOppositeArrowIcon(arrowIcon);
            currentProcessorArrow = processorArrowButton.BackgroundImage;
            lastClickedButtonName = button.Name;

            defaultProcessorElement.ProcessData();

            UpdateTextBox(button.Name, defaultProcessorElement);
            AnalyzeAndDisable();
        }

        private void AnalyzeAndDisable()
        {
            GroupBox PEGroupBox = FindControlRecursive(this, "PEGroupBox") as GroupBox;
            Dictionary<string, TextBox> textBoxMap = new Dictionary<string, TextBox>();

            foreach (Control control in PEGroupBox.Controls)
            {
                if (control is TextBox textBox && textBox.Text.Contains("'"))
                {
                    textBoxMap[textBox.Text] = textBox;
                }
            }

            foreach (Control groupControl in panel4.Controls)
            {
                if (groupControl is RichTextBox groupTextBox)
                {
                    bool foundMatchingTextBox = false;

                    foreach (var valueWithApostrophe in textBoxMap.Keys)
                    {
                        if (groupTextBox.Text.Contains(valueWithApostrophe))
                        {
                            groupTextBox.Enabled = true;
                            foundMatchingTextBox = true;
                            break;
                        }
                    }

                    if (!foundMatchingTextBox)
                    {
                        groupTextBox.Enabled = false;
                    }
                }
            }
        }

        private void UpdateTextBox(string buttonName, ProcessorElementUI element)
        {
            UpdateSingleTextBox(buttonName, element);

            if (syncArrowRadioButton.Checked && pairedButtons.TryGetValue(FindControlRecursive(this, buttonName) as Button, out var pairedButton))
            {
                UpdateSingleTextBox(pairedButton.Name, element);
            }
        }

        private void UpdateSingleTextBox(string buttonName, ProcessorElementUI element)
        {
            string textBoxName = buttonName.Replace("Button", "TextBox");
            var textBox = FindControlRecursive(PEGroupBoxControl, textBoxName) as TextBox;

            if (textBox != null)
            {
                switch (buttonName)
                {
                    case "VertLeftUpButton":
                        textBox.Text = element.LeftTopArrowPath.GetDescription();
                        break;
                    case "VertLeftDownButton":
                        textBox.Text = element.LeftBottomArrowPath.GetDescription();
                        break;
                    case "VertRightUpButton":
                        textBox.Text = element.RightTopArrowPath.GetDescription();
                        break;
                    case "VertRightDownButton":
                        textBox.Text = element.RightBottomArrowPath.GetDescription();
                        break;
                    case "HorizLeftUpButton":
                        textBox.Text = element.TopLeftArrowPath.GetDescription();
                        break;
                    case "HorizLeftDownButton":
                        textBox.Text = element.BottomLeftArrowPath.GetDescription();
                        break;
                    case "HorizRightUpButton":
                        textBox.Text = element.TopRightArrowPath.GetDescription();
                        break;
                    case "HorizRightDownButton":
                        textBox.Text = element.BottomRightArrowPath.GetDescription();
                        break;
                }
                textBox.Visible = true;
            }
        }

        public void UpdateButtonImage(Button button, Image image)
        {
            button.BackgroundImage = image;
            button.BackgroundImageLayout = ImageLayout.Zoom;
        }

        public void UpdateProcessorElementDirection(string buttonName, Image newImage)
        {
            ProcessorElementUI.Direction direction = imageLoader.GetDirectionFromImage(newImage);

            switch (buttonName)
            {
                case "VertLeftUpButton":
                    defaultProcessorElement.LeftTopArrow = direction;
                    break;
                case "VertLeftDownButton":
                    defaultProcessorElement.LeftBottomArrow = direction;
                    break;
                case "VertRightUpButton":
                    defaultProcessorElement.RightTopArrow = direction;
                    break;
                case "VertRightDownButton":
                    defaultProcessorElement.RightBottomArrow = direction;
                    break;
                case "HorizLeftUpButton":
                    defaultProcessorElement.TopLeftArrow = direction;
                    break;
                case "HorizLeftDownButton":
                    defaultProcessorElement.BottomLeftArrow = direction;
                    break;
                case "HorizRightUpButton":
                    defaultProcessorElement.TopRightArrow = direction;
                    break;
                case "HorizRightDownButton":
                    defaultProcessorElement.BottomRightArrow = direction;
                    break;
            }
        }

        int numberOfTakts = 14;
        int currentTakt = -1;
        private void stepForwardButton_Click_1(object sender, EventArgs e)
        {
            if (currentTakt < numberOfTakts)
            {
                button8.Enabled = true;

                systolicSystemUnit.DoTact(this, "linear");
                button8.Text = $"ТАКТ: {currentTakt}";
                currentTakt++;
                if (currentTakt > 0)
                    UpdateLabels();
            }
        }

        public void UpdateLabels()
        {
            foreach (Control control in PEsTableLayoutPanel.Controls)
            {
                if (control is Panel panel)
                {
                    foreach (Control child in panel.Controls)
                    {
                        if (child is FlowLayoutPanel flowLayoutPanel)
                        {
                            if (flowLayoutPanel.Tag != null && !flowLayoutPanel.Tag.ToString().Contains("'"))
                            {
                                if ((flowLayoutPanel.Tag.ToString() == "c" && flowLayoutPanel.Name == "RightBottomArrowPathPE2") || (flowLayoutPanel.Tag.ToString() == "a" && flowLayoutPanel.Name == "RightTopArrowPathPE2"))
                                    flowLayoutPanel.Controls.RemoveAt(0);
                                else if (flowLayoutPanel.Controls.Count > 0)
                                {
                                    flowLayoutPanel.Controls.RemoveAt(flowLayoutPanel.Controls.Count - 1);
                                }
                            }
                            else if (flowLayoutPanel.Tag != null && flowLayoutPanel.Tag.ToString().Contains("'"))
                            {
                                bool check = false;
                                if ((flowLayoutPanel.Tag.ToString() == "c'" && flowLayoutPanel.Name == "RightBottomArrowPathPE2") || (flowLayoutPanel.Tag.ToString() == "a'" && flowLayoutPanel.Name == "RightTopArrowPathPE2"))
                                    check = true;

                                var tag = flowLayoutPanel.Tag.ToString();
                                char lastChar = flowLayoutPanel.Name.Last(); 
                                if (char.IsDigit(lastChar))
                                {
                                    int index = int.Parse(lastChar.ToString()); 
                                    if (index >= 0 && index < processorElements.Count)
                                    {
                                        var processorElement = processorElements[index];
                                        if (processorElement.Outputs.ContainsKey(tag))
                                        {
                                            var outputValues = processorElement.Outputs[tag];
                                            if (outputValues != null && outputValues.Any())
                                            {
                                                var value = outputValues.ElementAt(outputValues.Count - 3);
                                                var label = new Label
                                                {
                                                    Text = value.ToString(),
                                                    Font = new Font("Arial", 12, FontStyle.Bold),
                                                    Size = new Size(62, 29),
                                                    BackColor = Color.DarkSalmon,
                                                    Margin = new Padding(5),
                                                    Anchor = AnchorStyles.None,
                                                    BorderStyle = BorderStyle.FixedSingle,
                                                    TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                                                };

                                                if (check)
                                                {
                                                    flowLayoutPanel.Controls.Add(label); 
                                                    flowLayoutPanel.Controls.SetChildIndex(label, 0); 
                                                }
                                                else
                                                {
                                                    flowLayoutPanel.Controls.Add(label);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if (currentTakt <= numberOfTakts && currentTakt > 0)
            {
                stepForwardButton.Enabled = true;
                button8.Text = $"ТАКТ: {currentTakt}";
            }
            else
            {
                button3.Enabled = false;
            }
        }

        private void logoutButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        int curTakt = 0;
        private void button1_Click(object sender, EventArgs e)
        {
            if (currentTakt < numberOfTakts)
            {
                button4.Enabled = true;

                systolicSystemUnit.DoTact(this, "square");
                button4.Text = $"ТАКТ: {curTakt}";
                curTakt++;
            }
        }

        private CancellationTokenSource _cancellationTokenSource;

        private async void button10_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            await Task.Run(() => ExecuteTactsAsync(token));
        }

        private async Task ExecuteTactsAsync(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                if (currentTakt < numberOfTakts)
                {
                    Invoke(new Action(() =>
                    {
                        button8.Enabled = true;
                        button8.Text = $"TACT: {currentTakt}";
                        systolicSystemUnit.DoTact(this, "linear");
                        if (currentTakt > 0)
                            UpdateLabels();
                        currentTakt++;
                    }));

                    await Task.Delay(1000); 
                }
                else
                {
                    break;
                }
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}