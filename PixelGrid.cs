using System.Collections;
using Raylib_cs;

namespace PixelGame;

public enum PixelKind {
	Sand,
	ColoredSand
}

public interface IPixelData;

public struct ColoredSandPixelData() : IPixelData {
	private static float _lastHue;

	private static float Hue {
		get {
			var cache = _lastHue;
			_lastHue += 0.0003f;
			if (_lastHue > 1.0) _lastHue = 0;
			return cache;
		}
	}

	public Color Color { get; } = ColorExtensions.FromHsb(Hue, 70, 100);
}

public static class Extensions {
	private static readonly PixelKind[] PixelKinds = Enum.GetValues<PixelKind>();

	public static Color Color(this PixelState pixelState) {
		return pixelState.Kind switch {
			PixelKind.Sand => new Color(194, 178, 128, 255),
			PixelKind.ColoredSand => pixelState.Data<ColoredSandPixelData>().Color,
			_ => throw new ArgumentOutOfRangeException(nameof(pixelState), pixelState, null)
		};
	}

	public static PixelKind Next(this PixelKind pixelKind) {
		return PixelKinds[((int)pixelKind + 1) % PixelKinds.Length];
	}

	public static string Name(this PixelKind pixelKind) {
		return pixelKind switch {
			PixelKind.Sand => "Sand",
			PixelKind.ColoredSand => "Colored Sand",
			_ => throw new ArgumentOutOfRangeException(nameof(pixelKind), pixelKind, null)
		};
	}

	public static IPixelData? DefaultData(this PixelKind pixelKind) {
		return pixelKind switch {
			PixelKind.Sand => null,
			PixelKind.ColoredSand => new ColoredSandPixelData(),
			_ => throw new ArgumentOutOfRangeException(nameof(pixelKind), pixelKind, null)
		};
	}
}

public record PixelState {
	private IPixelData? _data;
	public bool Enabled { get; set; }
	public PixelKind Kind { get; set; }

	public void Reset() {
		Enabled = false;
		_data = Kind.DefaultData();
	}

	public T? Data<T>() {
		return (T?)_data;
	}
}

public class PixelGrid : IEnumerable<Tuple<PixelState, int, int>> {
	public readonly List<List<PixelState>> States;

	public PixelGrid(int columns, int rows) {
		States = new List<List<PixelState>>(columns);
		for (var i = 0; i < columns; i++) {
			var rowStates = new List<PixelState>(rows);
			for (var j = 0; j < rows; j++)
				rowStates.Add(new PixelState { Enabled = false, Kind = PixelKind.Sand });
			States.Add(rowStates);
		}
	}

	public IEnumerator<Tuple<PixelState, int, int>> GetEnumerator() {
		for (var i = 0; i < States.Count; i++) {
			var row = States[i];
			for (var j = 0; j < row.Count; j++) yield return Tuple.Create(row[j], i, j);
		}
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}

	public PixelState? GetState(int col, int rowIdx) {
		if (col < 0 || rowIdx < 0) return null;

		if (States.Count <= col) return null;
		var row = States[col];
		return row.Count <= rowIdx ? null : row[rowIdx];
	}

	public void UpdateState(int col, int row, Action<PixelState> transform) {
		var state = GetState(col, row);
		if (state == null) return;
		transform(state);
	}
}