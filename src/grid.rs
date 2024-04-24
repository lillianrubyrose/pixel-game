use raylib::color::Color;

#[derive(Debug, Clone, Copy)]
pub enum PixelKind {
    Sand,
}

impl PixelKind {
    pub const fn color(&self) -> Color {
        Color {
            r: 194,
            g: 178,
            b: 128,
            a: 255,
        }
    }

    pub const fn name(&self) -> &'static str {
        match self {
            PixelKind::Sand => "Sand",
        }
    }
}

#[derive(Debug, Clone, Copy)]
pub struct PixelState {
    pub enabled: bool,
    pub kind: PixelKind,
    pub velocity: i32,
}

impl PixelState {
    pub fn reset(&mut self) {
        self.enabled = false;
        self.velocity = 1;
        self.kind = PixelKind::Sand;
    }
}

impl Default for PixelState {
    fn default() -> Self {
        Self {
            enabled: false,
            kind: PixelKind::Sand,
            velocity: 1,
        }
    }
}

pub struct PixelGrid {
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

    pub fn update_state<F: Fn(&mut PixelState)>(&mut self, col: i32, row: i32, transform: F) {
        if let Some(state) = self.get_state_mut(col, row) {
            transform(state);
        }
    }
}
