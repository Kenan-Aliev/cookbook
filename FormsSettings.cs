using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;


namespace Lab1_RKP
{
    class FormsSettings
    {
        public string Text { get; set; }

        public Color BackColor { get; set; }
        public Point Location { get; set; }
          


        public FormsSettings(string Text)
        {
            this.Text = Text;
            BackColor = Color.Beige;
            Location = new Point(200, 200);
        }
    }
}
