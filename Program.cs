using PixelGame;
using Raylib_cs;
using static Raylib_cs.Raylib;

const int pixelSize = 10;
const int width = 1200;
const int height = 1200;

InitWindow(width, height, "Pixel Game");
SetTargetFPS(60);

var grid = new PixelGrid(width / pixelSize, height / pixelSize);
var currentlyDroppingKind = PixelKind.Sand;
var dropRadius = 0;

while (!WindowShouldClose()) {
	BeginDrawing();
	ClearBackground(Color.Black);

	if (IsMouseButtonDown(MouseButton.Left)) {
		var pos = GetMousePosition();
		var col = (int)Math.Floor(pos.X / pixelSize);
		var row = (int)Math.Floor(pos.Y / pixelSize);
		var kind = currentlyDroppingKind;
		grid.UpdateState(col, row, pixelState => {
			if (pixelState.Enabled) return;
			pixelState.Kind = kind;
			pixelState.Reset();
			pixelState.Enabled = true;
		});

		if (dropRadius > 0) {
			for (var i = col - dropRadius; i <= col + dropRadius; i++) {
				for (var j = row - dropRadius; j <= row + dropRadius; j++) {
					grid.UpdateState(i, j, pixelState => {
						if (pixelState.Enabled) return;
						pixelState.Kind = kind;
						pixelState.Reset();
						pixelState.Enabled = true;
					});
				}
			}
		}
	}
	else if (IsMouseButtonDown(MouseButton.Right)) {
		var pos = GetMousePosition();
		var col = (int)Math.Floor(pos.X / pixelSize);
		var row = (int)Math.Floor(pos.Y / pixelSize);
		grid.UpdateState(col, row, pixelState => { pixelState.Enabled = false; });
		
		if (dropRadius > 0) {
			for (var i = col - dropRadius; i <= col + dropRadius; i++) {
				for (var j = row - dropRadius; j <= row + dropRadius; j++) {
					grid.UpdateState(i, j, pixelState => {
						pixelState.Enabled = false;
					});
				}
			}
		}
	}

	if (IsKeyPressed(KeyboardKey.Space)) currentlyDroppingKind = currentlyDroppingKind.Next();
	if (IsKeyPressed(KeyboardKey.F))
		foreach (var (pixelState, _, _) in grid) {
			pixelState.Kind = currentlyDroppingKind;
			pixelState.Reset();
			pixelState.Enabled = true;
		}

	if (IsKeyPressed(KeyboardKey.Delete))
		foreach (var (pixelState, _, _) in grid)
			pixelState.Enabled = false;
	dropRadius = (int)(dropRadius + GetMouseWheelMove());
	if (dropRadius < 0) dropRadius = 0;

	// draw pixels
	foreach (var (pixelState, col, row) in grid.Where(tuple => tuple.Item1.Enabled)) {
		var x = col * pixelSize;
		var y = row * pixelSize;
		DrawRectangle(x, y, pixelSize, pixelSize, pixelState.Color());
	}

	var newGrid = new PixelGrid(grid.Columns, grid.Rows);

	// update pixels
	foreach (var (pixelState, col, row) in grid.Where(tuple => tuple.Item1.Enabled))
		pixelState.Tick(grid, newGrid, col, row);

	grid = newGrid;

	if (dropRadius > 0) {
		var pos = GetMousePosition();
		var startCol = (int)Math.Floor(pos.X / pixelSize);
		var startRow = (int)Math.Floor(pos.Y / pixelSize);

		var topCol = startCol - dropRadius;
		var topRow = startRow - dropRadius;
		var x1 = topCol * pixelSize;
		var y1 = topRow * pixelSize;
		
		var bottomCol = startCol + dropRadius;
		var bottomRow = startRow + dropRadius;
		var x2 = bottomCol * pixelSize;
		var y2 = bottomRow * pixelSize;
		DrawRectangleLines(x1, y1, x2 - x1, y2 - y1, Color.Blue);
	}

	DrawText($"TPS: {GetFPS()}", 4, 4, 20, Color.RayWhite);
	DrawText($"Currently dropping: {currentlyDroppingKind.Name()}", 4, 4 + 20, 20, Color.RayWhite);
	DrawText($"Drop Radius: {dropRadius}", 4, 4 + 20 * 2, 20, Color.RayWhite);
	EndDrawing();
}

CloseWindow();