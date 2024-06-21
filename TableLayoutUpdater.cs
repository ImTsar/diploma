using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace GW_1
{
    public class TableLayoutUpdater
    {
        private readonly Main mainForm;
        private ProcessorElementUI defaultProcessorElement;
        private readonly Dictionary<Button, Button> pairedButtons;
        private readonly ButtonInitializer buttonInitializer;
        private readonly ImageLoader imageLoader;
        private readonly Action<string, ProcessorElementUI> updateTextBoxAction;

        public TableLayoutUpdater(Main mainForm, ProcessorElementUI defaultProcessorElement,
                                  Dictionary<Button, Button> pairedButtons,
                                  ButtonInitializer buttonInitializer, ImageLoader imageLoader,
                                  Action<string, ProcessorElementUI> updateTextBoxAction)
        {
            this.mainForm = mainForm;
            this.defaultProcessorElement = defaultProcessorElement;
            this.pairedButtons = pairedButtons;
            this.buttonInitializer = buttonInitializer;
            this.imageLoader = imageLoader;
            this.updateTextBoxAction = updateTextBoxAction;
        }


        public void UpdateTableLayoutPanel(TableLayoutPanel tableLayoutPanel, bool isDefault, ProcessorElementUI processorElement)
        {
            defaultProcessorElement = processorElement;

            for (int row = 0; row < tableLayoutPanel.RowCount; row++)
            {
                for (int col = 0; col < tableLayoutPanel.ColumnCount; col++)
                {
                    Control element = tableLayoutPanel.GetControlFromPosition(col, row);
                    string index = isDefault ? (row * tableLayoutPanel.ColumnCount + col).ToString() : $"{row}{col}";

                    if (index == (tableLayoutPanel.ColumnCount - 1).ToString())
                        index = (tableLayoutPanel.ColumnCount - 2).ToString();

                    if (element is Panel panelElement)
                    {
                        UpdatePanelElements(panelElement, index, isDefault, processorElement);
                    }
                }
            }
        }

        private void UpdatePanelElements(Panel panelElement, string index, bool isDefault, ProcessorElementUI processorElement)
        {
            foreach (Control panel in panelElement.Controls)
            {
                if (panel is Panel && panel.Name.EndsWith($"ButtonPE{index}"))
                {
                    UpdateArrowPanel(panel, index, isDefault);
                }
                else if (panel is FlowLayoutPanel flowLayoutPanel && panel.Name.Contains($"PE{index}"))
                {
                    UpdateFlowLayoutPanel(flowLayoutPanel, panel.Name, index, processorElement);
                }
                else if (panel is TextBox textBox && panel.Name.Contains($"PE{index}"))
                {
                    UpdateTextBox(textBox, panel.Name, index, processorElement);
                }
            }
        }

        private void UpdateArrowPanel(Control panel, string index, bool isDefault)
        {
            string panelName = panel.Name;
            ProcessorElementUI.Direction direction = GetDirection(panelName, index);
            Image arrowIcon = GetArrowIcon(direction);

            panel.BackColor = Color.Transparent;
            panel.Controls.Clear();

            if (arrowIcon != null)
            {
                Button arrowButton = new Button
                {
                    Size = isDefault ? new Size(40, 40) : new Size(33, 33),
                    BackgroundImage = arrowIcon,
                    BackgroundImageLayout = ImageLayout.Zoom,
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderSize = 0, MouseDownBackColor = Color.Transparent, MouseOverBackColor = Color.Transparent },
                    BackColor = Color.Transparent,
                };
                panel.Controls.Add(arrowButton);
            }
        }

        private void UpdateFlowLayoutPanel(FlowLayoutPanel flowLayoutPanel, string panelName, string index, ProcessorElementUI processorElement)
        {
            string panelNameWithoutIndex = panelName.Replace($"PE{index}", "").Replace("FlowLayoutPanel", "");
            var property = typeof(ProcessorElementUI).GetProperty(panelNameWithoutIndex);

            if (property != null)
            {
                var enumValue = property.GetValue(processorElement);
                if (enumValue != null)
                {
                    var description = GetEnumDescription((Enum)enumValue);
                    flowLayoutPanel.Tag = description;
                }
            }
        }

        private void UpdateTextBox(TextBox textBox, string panelName, string index, ProcessorElementUI processorElement)
        {
            string panelNameWithoutIndex = panelName.Replace($"PE{index}", "");
            var property = typeof(ProcessorElementUI).GetProperty(panelNameWithoutIndex);

            if (property != null)
            {
                var enumValue = property.GetValue(processorElement);
                if (enumValue != null)
                {
                    var description = GetEnumDescription((Enum)enumValue);
                    textBox.Text = description;
                }
            }
        }
        private string GetEnumDescription(Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            DescriptionAttribute attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }

        private ProcessorElementUI.Direction GetDirection(string panelName, string index)
        {
            if (panelName == $"VertLeftUpButtonPE{index}")
                return defaultProcessorElement.LeftTopArrow;
            if (panelName == $"VertLeftDownButtonPE{index}")
                return defaultProcessorElement.LeftBottomArrow;
            if (panelName == $"VertRightUpButtonPE{index}")
                return defaultProcessorElement.RightTopArrow;
            if (panelName == $"VertRightDownButtonPE{index}")
                return defaultProcessorElement.RightBottomArrow;
            if (panelName == $"HorizLeftUpButtonPE{index}")
                return defaultProcessorElement.TopLeftArrow;
            if (panelName == $"HorizRightUpButtonPE{index}")
                return defaultProcessorElement.TopRightArrow;
            if (panelName == $"HorizLeftDownButtonPE{index}")
                return defaultProcessorElement.BottomLeftArrow;
            if (panelName == $"HorizRightDownButtonPE{index}")
                return defaultProcessorElement.BottomRightArrow;

            return ProcessorElementUI.Direction.None;
        }



        private Image GetArrowIcon(ProcessorElementUI.Direction direction)
        {
            return direction switch
            {
                ProcessorElementUI.Direction.Left => imageLoader.LeftArrowIcon,
                ProcessorElementUI.Direction.Right => imageLoader.RightArrowIcon,
                ProcessorElementUI.Direction.Up => imageLoader.TopArrowIcon,
                ProcessorElementUI.Direction.Down => imageLoader.BottomArrowIcon,
                _ => null,
            };
        }

        public void HandleProcessorArrowButtonClick(string lastClickedButtonName, Button processorArrowButton, ref Image currentProcessorArrow)
        {
            if (!string.IsNullOrEmpty(lastClickedButtonName))
            {
                var syncRadioButton = buttonInitializer.FindControlRecursive(mainForm, "syncArrowRadioButton") as RadioButton;
                var lastClickedButton = pairedButtons.Keys.FirstOrDefault(b => b.Name == lastClickedButtonName);

                (mainForm as Main)?.UpdateProcessorElementDirection(lastClickedButtonName, processorArrowButton.BackgroundImage);

                if (lastClickedButton != null && pairedButtons.ContainsKey(lastClickedButton))
                {
                    lastClickedButton.BackgroundImage = processorArrowButton.BackgroundImage;

                    if (syncRadioButton.Checked)
                    {
                        pairedButtons[lastClickedButton].BackgroundImage = processorArrowButton.BackgroundImage;
                        (mainForm as Main)?.UpdateProcessorElementDirection(pairedButtons[lastClickedButton].Name, processorArrowButton.BackgroundImage);
                    }
                }

                Image newImage = GetNextArrowIcon(processorArrowButton.BackgroundImage);
                processorArrowButton.BackgroundImage = newImage;
                currentProcessorArrow = newImage;

                defaultProcessorElement.ProcessData();
                updateTextBoxAction?.Invoke(lastClickedButtonName, defaultProcessorElement);

                if (syncRadioButton.Checked && pairedButtons.TryGetValue(lastClickedButton, out var pairedButton))
                {
                    updateTextBoxAction?.Invoke(pairedButton.Name, defaultProcessorElement);
                }
            }
        }

        public void HandleReturnButtonClick(string lastClickedButtonName, Image plusIcon)
        {
            var button = (mainForm as Main)?.FindControlRecursive(mainForm, lastClickedButtonName) as Button;
            (mainForm as Main)?.UpdateButtonImage(button, plusIcon);
            (mainForm as Main)?.UpdateProcessorElementDirection(button.Name, plusIcon);

            var syncRadioButton = buttonInitializer.FindControlRecursive(mainForm, "syncArrowRadioButton") as RadioButton;

            if (syncRadioButton.Checked && pairedButtons.ContainsKey(button))
            {
                (mainForm as Main)?.UpdateButtonImage(pairedButtons[button], plusIcon);
                (mainForm as Main)?.UpdateProcessorElementDirection(pairedButtons[button].Name, plusIcon);
            }
        }

        private Image GetNextArrowIcon(Image currentImage)
        {
            return currentImage switch
            {
                var image when image == imageLoader.LeftArrowIcon => imageLoader.RightArrowIcon,
                var image when image == imageLoader.RightArrowIcon => imageLoader.LeftArrowIcon,
                var image when image == imageLoader.TopArrowIcon => imageLoader.BottomArrowIcon,
                var image when image == imageLoader.BottomArrowIcon => imageLoader.TopArrowIcon,
                _ => null,
            };
        }
    }
}
