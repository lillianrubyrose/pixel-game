using PixelGame;
using Raylib_cs;
using static Raylib_cs.Raylib;

const int pixelSize = 5;
const int width = 1200;
const int height = 1200;

InitWindow(width, height, "Pixel Game");
SetTargetFPS(60);

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
	else if (IsKeyPressed(KeyboardKey.Space)) {
		currentlyDroppingKind = currentlyDroppingKind.Next();
	}

	// draw pixels
	foreach (var (pixelState, col, row) in grid.Where(tuple => tuple.Item1.Enabled)) {
		var x = col * pixelSize;
		var y = row * pixelSize;
		DrawRectangle(x, y, pixelSize, pixelSize, pixelState.Color());
	}

	var newGrid = new PixelGrid(height / pixelSize, height / pixelSize);

	// update pixels
	foreach (var (pixelState, col, row) in grid.Where(tuple => tuple.Item1.Enabled)) {
		var belowState = grid.GetState(col, row + 1);
		switch (belowState) {
			case { Enabled: true }:
				var leftEmpty = grid.GetState(col - 1, row + 1)?.Enabled != true;
				var rightEmpty = grid.GetState(col + 1, row + 1)?.Enabled != true;

				var direction = -1;
				if (Random.Shared.NextSingle() > 0.5) direction = 1;

				if (leftEmpty && rightEmpty)
					newGrid.Set(col + direction, row, pixelState);
				else if (leftEmpty)
					newGrid.Set(col - 1, row, pixelState);
				else if (rightEmpty)
					newGrid.Set(col + 1, row, pixelState);
				else
					newGrid.Set(col, row, pixelState);

				break;
			case { Enabled: false }:
				newGrid.Set(col, row + 1, pixelState);
				break;
			case null:
				newGrid.Set(col, row, pixelState);
				break;
		}
	}

	grid = newGrid;

	DrawText($"TPS: {GetFPS()}", 4, 4, 20, Color.RayWhite);
	DrawText($"Currently dropping: {currentlyDroppingKind.Name()}", 4, 4 + 20, 20, Color.RayWhite);
	EndDrawing();
}

CloseWindow();