#[derive(Debug, Default, Clone)]
pub enum Value {
    #[default]
    Nil,
    Boolean(bool),
    Number(f64),
}
