using PixelGame;
using Raylib_cs;
using static Raylib_cs.Raylib;

const int pixelSize = 5;
const int width = 1200;
const int height = 1200;

InitWindow(width, height, "Pixel Game");
SetTargetFPS(360);

var grid = new PixelGrid(width / pixelSize, height / pixelSize);
var currentlyDroppingKind = PixelKind.Sand;

while (!WindowShouldClose()) {
	BeginDrawing();
	ClearBackground(Color.Black);

	if (IsMouseButtonDown(MouseButton.Left)) {
		var pos = GetMousePosition();
		var col = (int)Math.Floor(pos.X / pixelSize);
		var row = (int)Math.Floor(pos.Y / pixelSize);
		var kind = currentlyDroppingKind;
		grid.UpdateState(col, row, pixelState => {
			pixelState.Kind = kind;
			pixelState.Reset();
			pixelState.Enabled = true;
		});
	}
	else if (IsMouseButtonDown(MouseButton.Right)) {
		var pos = GetMousePosition();
		var col = (int)Math.Floor(pos.X / pixelSize);
		var row = (int)Math.Floor(pos.Y / pixelSize);
		grid.UpdateState(col, row, pixelState => { pixelState.Enabled = false; });
	}

	if (IsKeyPressed(KeyboardKey.Space)) currentlyDroppingKind = currentlyDroppingKind.Next();
	if (IsKeyPressed(KeyboardKey.F))
		foreach (var (pixelState, _, _) in grid) {
			pixelState.Kind = currentlyDroppingKind;
			pixelState.Reset();
			pixelState.Enabled = true;
		}
	if (IsKeyPressed(KeyboardKey.Delete))
		foreach (var (pixelState, _, _) in grid) {
			pixelState.Enabled = false;
		}


	// draw pixels
	foreach (var (pixelState, col, row) in grid.Where(tuple => tuple.Item1.Enabled)) {
		var x = col * pixelSize;
		var y = row * pixelSize;
		DrawRectangle(x, y, pixelSize, pixelSize, pixelState.Color());
	}

	var newGrid = new PixelGrid(height / pixelSize, height / pixelSize);

	// update pixels
	foreach (var (pixelState, col, row) in grid.Where(tuple => tuple.Item1.Enabled))
		pixelState.Tick(grid, newGrid, col, row);

	grid = newGrid;

	DrawText($"TPS: {GetFPS()}", 4, 4, 20, Color.RayWhite);
	DrawText($"Currently dropping: {currentlyDroppingKind.Name()}", 4, 4 + 20, 20, Color.RayWhite);
	EndDrawing();
}

CloseWindow();