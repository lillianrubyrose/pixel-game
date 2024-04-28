using System.Collections;
using Raylib_cs;

namespace PixelGame;

public enum PixelKind {
	Sand,
	ColoredSand
}

public interface IPixelData;

public class AccelerationPixelData : IPixelData {
	public int Acceleration { get; set; } = 1;
}

public class ColoredSandPixelData : AccelerationPixelData {
	private static float _lastHue;

	private static float Hue {
		get {
			var cache = _lastHue;
			_lastHue += 0.0002f;
			if (_lastHue > 1.0) _lastHue = 0;
			return cache;
		}
	}

	public Color Color { get; } = ColorExtensions.FromHsb(Hue, 30, 85);
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
			PixelKind.Sand => new AccelerationPixelData(),
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

	public T Data<T>() {
		return (T?)_data ?? throw new InvalidOperationException("Attempt to get data on PixelKind without it present.");
	}

	public void Tick(PixelGrid oldGrid, PixelGrid newGrid, int col, int row) {
		switch (Kind) {
			case PixelKind.Sand:
				SimpleTick(oldGrid, newGrid, col, row, Data<AccelerationPixelData>());
				break;
			case PixelKind.ColoredSand:
				SimpleTick(oldGrid, newGrid, col, row, Data<ColoredSandPixelData>());
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(Kind), Kind, null);
		}
	}

	private void SimpleTick(PixelGrid oldGrid, PixelGrid newGrid, int col, int row,
		AccelerationPixelData? accelerationPixelData = null) {
		var belowState = oldGrid.GetState(col, row + 1);
		switch (belowState) {
			case { Enabled: true }:
				var leftEmpty = oldGrid.GetState(col - 1, row + 1) is { Enabled: false };
				var rightEmpty = oldGrid.GetState(col + 1, row + 1) is { Enabled: false };

				var direction = -1;
				if (Random.Shared.NextSingle() > 0.5) direction = 1;

				if (leftEmpty && rightEmpty)
					newGrid.Set(col + direction, row, this);
				else if (leftEmpty)
					newGrid.Set(col - 1, row, this);
				else if (rightEmpty)
					newGrid.Set(col + 1, row, this);
				else
					newGrid.Set(col, row, this);

				break;
			case { Enabled: false }:
				if (accelerationPixelData != null) {
					var acceleration = accelerationPixelData.Acceleration;
					var newRow = row + acceleration;
					if (newRow >= newGrid.Rows) {
						newRow = newGrid.Rows - 1;
						acceleration = 0;
					}

					int? blocker = null;
					for (var i = row + 1; i <= newRow; i++) {
						var potentialBlocker = oldGrid.GetState(col, i);
						if (potentialBlocker is not { Enabled: true }) continue;
						blocker = i;
						break;
					}

					if (blocker != null) {
						newRow = (int)(blocker - 1);
						acceleration = 0;
					}

					if (newRow < row) newRow = row + 1;

					accelerationPixelData.Acceleration = acceleration + 1;
					newGrid.Set(col, newRow, this);
				}
				else {
					newGrid.Set(col, row + 1, this);
				}

				break;
			case null:
				newGrid.Set(col, row, this);
				break;
		}
	}
}

public class PixelGrid : IEnumerable<Tuple<PixelState, int, int>> {
	private readonly int _columns;
	private readonly List<PixelState> _states;
	public readonly int Rows;

	public PixelGrid(int columns, int rows) {
		_columns = columns;
		Rows = rows;

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
		if (col >= _columns || rowIdx >= Rows) return null;

		var index = rowIdx * _columns + col;
		return _states.Count <= index ? null : _states[index];
	}

	public void Set(int col, int row, PixelState state) {
		if (col < 0 || row < 0) return;
		if (col >= _columns || row >= Rows) return;

		var index = row * _columns + col;
		_states[index] = state;
	}

	public void UpdateState(int col, int row, Action<PixelState> transform) {
		var state = GetState(col, row);
		if (state == null) return;
		transform(state);
	}
}