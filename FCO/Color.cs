namespace SUFcoTool
{
    public class Color
    {
        public int ColorStart { get; set; }
        public int ColorEnd { get; set; }
        public int ColorMarker { get; set; }

        //Could be replaced with a Vector4
        public byte ColorAlpha { get; set; }
        public byte ColorRed { get; set; }
        public byte ColorGreen { get; set; }
        public byte ColorBlue { get; set; }


        public static implicit operator System.Numerics.Vector4(Color d) => new System.Numerics.Vector4(d.ColorRed / 255.0f, d.ColorGreen / 255.0f, d.ColorBlue / 255.0f, d.ColorAlpha / 255.0f);
    }

}
