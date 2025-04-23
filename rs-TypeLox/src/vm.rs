use std::fs::read_to_string;
use std::path::Path;
use std::path::absolute;

use anyhow::bail;
use url::Url;

use crate::value::Value;

// munificent calls this VM
// This is named more like Lua
pub struct LoxState {}

impl LoxState {
    pub fn new() -> Self { LoxState {} }

    pub fn run_string(
        &mut self,
        code: &str,
        location: &Url,
    ) -> crate::Result<Value> {
        eprintln!("running at '{}'", location);
        eprintln!("code: \n{:?}", code);
        // TODO: scan parse eval etc.
        Ok(Value::default())
    }

    pub fn run_file(&mut self, path: &Path) -> crate::Result<Value> {
        let Ok(url) = Url::from_file_path(absolute(path)?) else {
            bail!("failed to convert '{}' to a file uri", path.display());
        };
        let contents = read_to_string(path)?;
        self.run_string(&contents, &url)
    }
}
