use rand::{rngs::ThreadRng, Rng};
use raylib::prelude::*;

#[derive(Debug, Clone, Copy)]
struct PixelState {
    pub enabled: bool,
}

impl PixelState {
    pub fn toggle(&mut self) {
        self.enabled = !self.enabled;
    }
}

impl Default for PixelState {
    fn default() -> Self {
        Self { enabled: false }
    }
}

struct PixelGrid {
    states: Vec<Vec<PixelState>>,
}

impl PixelGrid {
    pub fn new(columns: usize, rows: usize) -> Self {
        let mut states: Vec<Vec<PixelState>> = Vec::with_capacity(columns);
        for i in 0..columns {
            let mut rows = Vec::with_capacity(rows);
            for j in 0..rows.capacity() {
                rows.insert(j, PixelState::default());
            }
            states.insert(i, rows);
        }
        Self { states }
    }

    pub fn get_state(&self, col: i32, row: i32) -> Option<&PixelState> {
        if col.is_negative() || row.is_negative() {
            return None;
        }

        self.states
            .get(col as usize)
            .map(|rows| rows.get(row as usize))?
    }

    pub fn get_state_mut(&mut self, col: i32, row: i32) -> Option<&mut PixelState> {
        if col.is_negative() || row.is_negative() {
            return None;
        }

        self.states
            .get_mut(col as usize)
            .map(|rows| rows.get_mut(row as usize))?
    }
}

const PIXEL_SIZE: i32 = 5;
const WIDTH: i32 = 1200;
const HEIGHT: i32 = 1200;
const COLUMNS: i32 = WIDTH / PIXEL_SIZE;
const ROWS: i32 = HEIGHT / PIXEL_SIZE;

fn main() {
    let (mut handle, thread) = raylib::init()
        .size(WIDTH, HEIGHT)
        .title("Pixel Game")
        .build();

    handle.set_target_fps(240);

    let mut pixel_grid = PixelGrid::new(COLUMNS as usize, ROWS as usize);
    let mut rng = ThreadRng::default();
    while !handle.window_should_close() {
        let lmb_held = handle.is_mouse_button_down(MouseButton::MOUSE_LEFT_BUTTON);
        let rmb_held = handle.is_mouse_button_down(MouseButton::MOUSE_RIGHT_BUTTON);
        let mouse_pos = handle.get_mouse_position();
        let mut new_grid = PixelGrid::new(COLUMNS as usize, ROWS as usize);

        let mut draw = handle.begin_drawing(&thread);
        draw.clear_background(Color::BLACK);

        if lmb_held {
            let col = (mouse_pos.x / PIXEL_SIZE as f32).floor() as i32;
            let row = (mouse_pos.y / PIXEL_SIZE as f32).floor() as i32;
            if let Some(_) = pixel_grid.get_state(col, row) {
                for i in -3..3 {
                    let state = if rng.gen_bool(0.5) {
                        pixel_grid.get_state_mut(col + i, row)
                    } else {
                        pixel_grid.get_state_mut(col, row + i)
                    };

                    if let Some(state) = state {
                        if !state.enabled {
                            state.toggle();
                        }
                    }
                }

                if let Some(state) = pixel_grid.get_state_mut(col, row) {
                    if !state.enabled {
                        state.toggle();
                    }
                }
            }
        } else if rmb_held {
            let col = (mouse_pos.x / PIXEL_SIZE as f32).floor() as i32;
            let row = (mouse_pos.y / PIXEL_SIZE as f32).floor() as i32;
            if let Some(state) = pixel_grid.get_state_mut(col, row) {
                if state.enabled {
                    state.toggle();
                }
            }
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
                    draw.draw_rectangle(x, y, PIXEL_SIZE, PIXEL_SIZE, Color::WHITE);
                }
            }
        }

        // update pixels
        for col in 0..COLUMNS {
            for row in 0..ROWS {
                let Some(state) = pixel_grid.get_state_mut(col, row) else {
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
                                if let Some(state) =
                                    new_grid.get_state_mut(col + direction, row + 1)
                                {
                                    state.enabled = true;
                                }
                            } else if left_empty {
                                if let Some(state) = new_grid.get_state_mut(col - 1, row + 1) {
                                    state.enabled = true;
                                }
                            } else if right_empty {
                                if let Some(state) = new_grid.get_state_mut(col + 1, row + 1) {
                                    state.enabled = true;
                                }
                            } else {
                                if let Some(state) = new_grid.get_state_mut(col, row) {
                                    state.enabled = true;
                                }
                            }
                        } else {
                            if let Some(state) = new_grid.get_state_mut(col, row + 1) {
                                state.enabled = true;
                            }
                        }
                    } else {
                        if let Some(state) = new_grid.get_state_mut(col, row) {
                            state.enabled = true;
                        }
                    }
                }
            }
        }

        pixel_grid = new_grid;
    }
}
