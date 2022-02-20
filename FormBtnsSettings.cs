using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;


namespace Lab1_RKP
{
    class FormBtnsSettings
    {

        public string Text { get; set; }

        public Color BackColor { get; set; }

        public Color ForeColor { get; set; }

        public Size Size { get; set; }

        public Point Location { get; set; }


        public FormBtnsSettings(string Text,Color BackColor, Color ForeColor, Size Size,Point Location)
        {
            this.Text = Text;
            this.BackColor = BackColor;
            this.ForeColor = ForeColor;
            this.Size = Size;
            this.Location = Location;
        }


    }
}
