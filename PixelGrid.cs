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
	private readonly int _columns;
	private readonly List<PixelState> _states;

	public PixelGrid(int columns, int rows) {
		_columns = columns;

		_states = new List<PixelState>(columns * rows);
		for (var i = 0; i < columns * rows; i++) _states.Add(new PixelState { Enabled = false, Kind = PixelKind.Sand });
	}

	public IEnumerator<Tuple<PixelState, int, int>> GetEnumerator() {
		for (var i = 0; i < _states.Count; i++) {
			var column = i % _columns;
			var row = i / _columns;
			yield return Tuple.Create(_states[i], column, row);
		}
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}

	public PixelState? GetState(int col, int rowIdx) {
		if (col < 0 || rowIdx < 0) return null;

		var index = rowIdx * _columns + col;
		return _states.Count <= index ? null : _states[index];
	}

	public void Set(int col, int row, PixelState state) {
		var index = row * _columns + col;
		if (_states.Count <= index || int.IsNegative(index)) return;

 		_states[index] = state;
	}

	public void UpdateState(int col, int row, Action<PixelState> transform) {
		var state = GetState(col, row);
		if (state == null) return;
		transform(state);
	}
}