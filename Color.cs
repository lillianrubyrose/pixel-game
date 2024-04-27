using Raylib_cs;

namespace PixelGame;

public static class ColorExtensions {
	public static Color FromHsb(float hue, int saturation, int brightness) {
		double r = 0;
		double g = 0;
		double b = 0;

		var hueFraction = hue; // Assuming hue is in degrees (0-360)
		var saturationFraction = saturation / 100.0; // Assuming saturation is a percentage (0-100)
		var brightnessFraction = brightness / 100.0; // Assuming brightness is a percentage (0-100)

		if (saturationFraction == 0) {
			r = g = b = brightnessFraction;
		}
		else {
			var sectorPos = hueFraction * 6.0;
			var sectorNumber = (int)Math.Floor(sectorPos);
			var fractionalSector = sectorPos - sectorNumber;

			var p = brightnessFraction * (1.0 - saturationFraction);
			var q = brightnessFraction * (1.0 - saturationFraction * fractionalSector);
			var t = brightnessFraction * (1.0 - saturationFraction * (1 - fractionalSector));

			switch (sectorNumber) {
				case 0:
					r = brightnessFraction;
					g = t;
					b = p;
					break;
				case 1:
					r = q;
					g = brightnessFraction;
					b = p;
					break;
				case 2:
					r = p;
					g = brightnessFraction;
					b = t;
					break;
				case 3:
					r = p;
					g = q;
					b = brightnessFraction;
					break;
				case 4:
					r = t;
					g = p;
					b = brightnessFraction;
					break;
				case 5:
					r = brightnessFraction;
					g = p;
					b = q;
					break;
			}
		}

		return new Color((int)(r * 255), (int)(g * 255), (int)(b * 255), 255);
	}
}