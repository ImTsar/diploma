namespace GW_1
{
    public static class ProcessorElementManager
    {
        public static List<DataFlowConnector> GetElementInterconnections(Control parent)
        {
            List<DataFlowConnector> connectors = new List<DataFlowConnector>();
            ProcessChildControls(parent, connectors);
            return connectors;
        }

        private static void ProcessChildControls(Control parent, List<DataFlowConnector> connectors)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is GroupBox groupBox && groupBox.Name.StartsWith("PEGroupBox"))
                {
                    TextBox vertLeftUpTextBox = groupBox.Controls.Find("VertLeftUpTextBox", true).FirstOrDefault() as TextBox;
                    TextBox vertRightUpTextBox = groupBox.Controls.Find("VertRightUpTextBox", true).FirstOrDefault() as TextBox;

                    TextBox vertLeftDownTextBox = groupBox.Controls.Find("VertLeftDownTextBox", true).FirstOrDefault() as TextBox;
                    TextBox vertRightDownTextBox = groupBox.Controls.Find("VertRightDownTextBox", true).FirstOrDefault() as TextBox;

                    TextBox horizLeftUpTextBox = groupBox.Controls.Find("HorizLeftUpTextBox", true).FirstOrDefault() as TextBox;
                    TextBox horizLeftDownTextBox = groupBox.Controls.Find("HorizLeftDownTextBox", true).FirstOrDefault() as TextBox;

                    TextBox horizRightUpTextBox = groupBox.Controls.Find("HorizRightUpTextBox", true).FirstOrDefault() as TextBox;
                    TextBox horizRightDownTextBox = groupBox.Controls.Find("HorizRightDownTextBox", true).FirstOrDefault() as TextBox;

                    if (vertLeftUpTextBox != null && vertRightUpTextBox != null)
                    {
                        if (vertLeftUpTextBox.Text == "a" && vertRightUpTextBox.Text == "a'")
                        {
                            connectors.Add(new DataFlowConnector("a'", ConnectorDirection.ToRightPE));
                        }
                        else if (vertLeftUpTextBox.Text == "a'" && vertRightUpTextBox.Text == "a")
                        {
                            connectors.Add(new DataFlowConnector("a", ConnectorDirection.FromRightPE));
                        }
                    }

                    if (vertLeftDownTextBox != null && vertRightDownTextBox != null)
                    {
                        if (vertLeftDownTextBox.Text == "c" && vertRightDownTextBox.Text == "c'")
                        {
                            connectors.Add(new DataFlowConnector("c'", ConnectorDirection.ToRightPE));
                        }
                        else if (vertLeftDownTextBox.Text == "c'" && vertRightDownTextBox.Text == "c")
                        {
                            connectors.Add(new DataFlowConnector("c", ConnectorDirection.FromRightPE));
                        }
                    }

                    if (horizLeftUpTextBox != null && horizLeftDownTextBox != null)
                    {
                        if (horizLeftUpTextBox.Text == "b" && horizLeftDownTextBox.Text == "b'")
                        {
                            connectors.Add(new DataFlowConnector("b'", ConnectorDirection.ToBottomPE));
                        }
                        else if (horizLeftUpTextBox.Text == "b'" && horizLeftDownTextBox.Text == "b")
                        {
                            connectors.Add(new DataFlowConnector("b", ConnectorDirection.FromBottomPE));
                        }
                    }

                    if (horizRightUpTextBox != null && horizRightDownTextBox != null)
                    {
                        if (horizRightUpTextBox.Text == "d" && horizRightDownTextBox.Text == "d'")
                        {
                            connectors.Add(new DataFlowConnector("d'", ConnectorDirection.ToBottomPE));
                        }
                        else if (horizRightUpTextBox.Text == "d'" && horizRightDownTextBox.Text == "d")
                        {
                            connectors.Add(new DataFlowConnector("d", ConnectorDirection.FromBottomPE));
                        }
                    }
                }

                ProcessChildControls(child, connectors);
            }
        }
    }
}
