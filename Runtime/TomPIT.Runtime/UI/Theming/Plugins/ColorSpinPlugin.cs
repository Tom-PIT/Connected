namespace TomPIT.UI.Theming.Plugins
{
	using System.ComponentModel;
	using Parser.Infrastructure.Nodes;
	using Parser.Tree;
	using Utils;

	[Description("Automatically spins all colors in a less file"), DisplayName("ColorSpin")]
    public class ColorSpinPlugin : VisitorPlugin
    {
        public double Spin { get; set; }

        public ColorSpinPlugin(double spin)
        {
            Spin = spin;
        }

        public override VisitorPluginType AppliesTo
        {
            get { return VisitorPluginType.AfterEvaluation; }
        }

        public override Node Execute(Node node, out bool visitDeeper)
        {
            visitDeeper = true;

            var color = node as Color;
            if (color == null) return node;

            var hslColor = ThemeHslColor.FromRgbColor(color);
            hslColor.Hue += Spin/360.0d;
            var newColor = hslColor.ToRgbColor();

            return newColor.ReducedFrom<Color>(color);
        }
    }
}