using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GW_1
{
    internal class LanguageManager
    {
        private Dictionary<string, string> translations;

        public LanguageManager(Dictionary<string, string> translations)
        {
            this.translations = translations;
        }

        public void ChangeLanguage(Control control)
        {
            foreach (Control childControl in control.Controls)
            {
                if (translations.ContainsKey(childControl.Text))
                {
                    childControl.Text = translations[childControl.Text];
                }

                if (childControl.Controls.Count > 0)
                {
                    ChangeLanguage(childControl);
                }
            }
        }
    }
}
