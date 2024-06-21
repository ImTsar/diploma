using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace GW_1
{
    public class ButtonInitializer
    {
        private Main mainForm;
        private ImageLoader imageLoader;
        private Dictionary<Button, Button> pairedButtons;

        public ButtonInitializer(Main mainForm, ImageLoader imageLoader)
        {
            this.mainForm = mainForm;
            this.imageLoader = imageLoader;
        }

        public Dictionary<Button, Button> InitializeButtons(Control parent, string[] panelNames, Image plusIcon)
        {
            foreach (var panelName in panelNames)
            {
                AddButtonToPanel(parent, panelName, plusIcon);
            }

            var syncRadioButton = FindControlRecursive(parent, "syncArrowRadioButton") as System.Windows.Forms.RadioButton;

            if (syncRadioButton.Checked)
            {

                pairedButtons = new Dictionary<Button, Button>
                {
                    { GetButton("VertLeftUpButton"), GetButton("VertRightUpButton") },
                    { GetButton("VertRightUpButton"), GetButton("VertLeftUpButton") },
                    { GetButton("VertLeftDownButton"), GetButton("VertRightDownButton") },
                    { GetButton("VertRightDownButton"), GetButton("VertLeftDownButton") },
                    { GetButton("HorizLeftUpButton"), GetButton("HorizLeftDownButton") },
                    { GetButton("HorizLeftDownButton"), GetButton("HorizLeftUpButton") },
                    { GetButton("HorizRightUpButton"), GetButton("HorizRightDownButton") },
                    { GetButton("HorizRightDownButton"), GetButton("HorizRightUpButton") }
                };
            }
            else
            {
                pairedButtons = new Dictionary<Button, Button>
                {
                    { GetButton("VertLeftUpButton"), GetButton("VertLeftUpButton") },
                    { GetButton("VertRightUpButton"), GetButton("VertRightUpButton") },
                    { GetButton("VertLeftDownButton"), GetButton("VertLeftDownButton") },
                    { GetButton("VertRightDownButton"), GetButton("VertRightDownButton") },
                    { GetButton("HorizLeftUpButton"), GetButton("HorizLeftUpButton") },
                    { GetButton("HorizLeftDownButton"), GetButton("HorizLeftDownButton") },
                    { GetButton("HorizRightUpButton"), GetButton("HorizRightUpButton") },
                    { GetButton("HorizRightDownButton"), GetButton("HorizRightDownButton") }
                };
            }

            InitializeButtonProperties(pairedButtons.Keys.Concat(new[] { GetButton("HorizLeftUpButton"), GetButton("HorizRightUpButton") }), plusIcon);

            return pairedButtons;
        }

        private void AddButtonToPanel(Control parent, string panelName, Image plusIcon)
        {
            Panel panel = FindControlRecursive(parent, panelName) as Panel;
            if (panel != null)
            {
                panel.BackColor = Color.Transparent;
                var button = CreateButton(panelName.Replace("Panel", "Button"), plusIcon);
                panel.Controls.Add(button);
            }
        }

        private Button CreateButton(string name, Image backgroundImage)
        {
            return new Button
            {
                Name = name,
                BackgroundImage = backgroundImage,
                BackgroundImageLayout = ImageLayout.Zoom,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                FlatAppearance = { BorderSize = 0 }
            };
        }

        private Button GetButton(string name)
        {
            return FindControlRecursive(mainForm, name) as Button;
        }

        private void InitializeButtonProperties(IEnumerable<Button> buttons, Image plusIcon)
        {
            foreach (var button in buttons)
            {
                if (button != null)
                {
                    SetupButton(button, plusIcon);
                }
                else
                {
                    MessageBox.Show("Button not found!");
                }
            }
        }

        private void SetupButton(Button button, Image backgroundImage)
        {
            button.Height = 40;
            button.Width = 40;
            button.BackgroundImage = backgroundImage;
            button.BackgroundImageLayout = ImageLayout.Zoom;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = Color.Transparent;
            button.Click += DirectionButton_Click;
        }

        private void DirectionButton_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                ShowArrowSelectionMenu(button);
            }
        }

        private void ShowArrowSelectionMenu(Button button)
        {
            var contextMenu = new ContextMenuStrip();
            if (button.Name.StartsWith("Horiz"))
            {
                AddMenuItem(contextMenu, mainForm.GetTranslation("Direction Up"), imageLoader.TopArrowIcon, button, "top");
                AddMenuItem(contextMenu, mainForm.GetTranslation("Direction Down"), imageLoader.BottomArrowIcon, button, "bottom");
            }
            else
            {
                AddMenuItem(contextMenu, mainForm.GetTranslation("Direction Left"), imageLoader.LeftArrowIcon, button, "left");
                AddMenuItem(contextMenu, mainForm.GetTranslation("Direction Right"), imageLoader.RightArrowIcon, button, "right");
            }
            contextMenu.AutoSize = true;
            contextMenu.Show(button, new Point(button.Width / 2, button.Height / 2));
        }

        private void AddMenuItem(ContextMenuStrip contextMenu, string text, Image icon, Button button, string direction)
        {
            var menuItem = new ToolStripMenuItem(text, icon, (s, e) => mainForm.SetArrow(button, direction));
            contextMenu.Items.Add(menuItem);
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

        public bool HasAtLeastOneArrow()
        {
            return (mainForm as Main)?.PEGroupBoxControl.Controls.OfType<Panel>()
                .SelectMany(panel => panel.Controls.OfType<Button>())
                .Any(button => button.BackgroundImage != (mainForm as Main)?.PlusIcon) ?? false;
        }

        public void UpdateButtonsInControl(Control parent, Image newImage)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is Button button && control.Name != "returnButton" && control.Name != "defaultButton")
                {
                    button.BackgroundImage = newImage;
                    button.BackgroundImageLayout = ImageLayout.Zoom;
                    button.FlatAppearance.BorderSize = 0;
                    button.BackColor = Color.Transparent;
                    (mainForm as Main)?.UpdateProcessorElementDirection(button.Name, newImage);
                }
                else if (control.HasChildren)
                {
                    UpdateButtonsInControl(control, newImage);
                }
            }
        }
    }
}
