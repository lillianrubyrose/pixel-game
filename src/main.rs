mod grid;

use grid::{PixelGrid, PixelKind, PixelState};
use rand::{rngs::ThreadRng, Rng};
use raylib::prelude::*;

const PIXEL_SIZE: i32 = 5;
const WIDTH: i32 = 1200;
const HEIGHT: i32 = 1200;
const COLUMNS: i32 = WIDTH / PIXEL_SIZE;
const ROWS: i32 = HEIGHT / PIXEL_SIZE;

fn main() {
    let (mut handle, thread) = init().size(WIDTH, HEIGHT).title("Pixel Game").build();

    handle.set_target_fps(60);

    let mut pixel_grid = PixelGrid::new(COLUMNS as usize, ROWS as usize);
    let mut rng = ThreadRng::default();
    let selected_pixel_kind = PixelKind::Sand;
    while !handle.window_should_close() {
        let lmb_held = handle.is_mouse_button_down(MouseButton::MOUSE_LEFT_BUTTON);
        let rmb_held = handle.is_mouse_button_down(MouseButton::MOUSE_RIGHT_BUTTON);
        let mouse_pos = handle.get_mouse_position();
        let mut new_grid = PixelGrid::new(COLUMNS as usize, ROWS as usize);

        let mut draw = handle.begin_drawing(&thread);
        draw.clear_background(Color::BLACK);
        draw.draw_text(
            &format!("Currently dropping: {}", selected_pixel_kind.name()),
            4,
            4,
            20,
            Color::RAYWHITE,
        );

        if lmb_held {
            let col = (mouse_pos.x / PIXEL_SIZE as f32).floor() as i32;
            let row = (mouse_pos.y / PIXEL_SIZE as f32).floor() as i32;
            if pixel_grid.get_state(col, row).is_some() {
                let transform = |state: &mut PixelState| {
                    state.reset();
                    state.enabled = true;
                    state.kind = selected_pixel_kind;
                };

                for i in -3..3 {
                    if rng.gen_bool(0.5) {
                        pixel_grid.update_state(col + i, row, transform);
                    } else {
                        pixel_grid.update_state(col, row + i, transform);
                    }
                }

                pixel_grid.update_state(col, row, transform);
            }
        } else if rmb_held {
            let col = (mouse_pos.x / PIXEL_SIZE as f32).floor() as i32;
            let row = (mouse_pos.y / PIXEL_SIZE as f32).floor() as i32;
            pixel_grid.update_state(col, row, |state| state.enabled = false);
        }

        // draw pixels
        for col in 0..COLUMNS {
            for row in 0..ROWS {
                let Some(state) = pixel_grid.get_state_mut(col, row) else {
                    unreachable!()
                };

                if state.enabled {
                    let x = col * PIXEL_SIZE;
                    let y = row * PIXEL_SIZE;
                    draw.draw_rectangle(x, y, PIXEL_SIZE, PIXEL_SIZE, state.kind.color());
                }
            }
        }

        // update pixels
        for col in 0..COLUMNS {
            for row in 0..ROWS {
                let Some(state) = pixel_grid.get_state(col, row) else {
                    unreachable!()
                };

                if state.enabled {
                    if let Some(below_state) = pixel_grid.get_state(col, row + 1) {
                        if below_state.enabled {
                            let left_empty = pixel_grid
                                .get_state(col - 1, row + 1)
                                .map_or(false, |state| !state.enabled);
                            let right_empty = pixel_grid
                                .get_state(col + 1, row + 1)
                                .map_or(false, |state| !state.enabled);

                            let mut direction = -1;
                            if rng.gen_bool(0.5) {
                                direction = 1;
                            }

                            if left_empty && right_empty {
                                new_grid.update_state(col + direction, row + 1, |state| {
                                    state.enabled = true
                                });
                            } else if left_empty {
                                new_grid
                                    .update_state(col - 1, row + 1, |state| state.enabled = true);
                            } else if right_empty {
                                new_grid
                                    .update_state(col + 1, row + 1, |state| state.enabled = true);
                            } else {
                                new_grid.update_state(col, row, |state| state.enabled = true);
                            }
                        } else {
                            let mut velocity = state.velocity;
                            let mut new_row = row + velocity;
                            if new_row >= ROWS {
                                new_row = ROWS - 1;
                                velocity = 0;
                            }

                            let mut blocker = None;
                            for i in row+1..=new_row {
                                if let Some(pixel) = pixel_grid.get_state(col, i) {
                                    if pixel.enabled {
                                        blocker = Some(i);
                                        break;
                                    }
                                }
                            }

                            if let Some(blocker) = blocker {
                                new_row = blocker - 1;
                                velocity = 0;
                            }
                            if new_row < row {
                                new_row = row+1;
                            }

                            new_grid.update_state(col, new_row, |state| {
                                state.enabled = true;
                                state.velocity = velocity + 1;
                            });
                        }
                    } else {
                        new_grid.update_state(col, row, |state| {
                            state.enabled = true;
                        });
                    }
                }
            }
        }

        pixel_grid = new_grid;
    }
}
