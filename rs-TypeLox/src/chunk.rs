////////////
// OpCode //
////////////

use std::fmt::Write;
use std::path::Path;

#[repr(u8)]
#[allow(non_camel_case_types)]
pub enum OpCode {
    RETURN = 0,
}

impl From<OpCode> for u8 {
    #[inline]
    fn from(value: OpCode) -> Self { value as u8 }
}

pub struct Chunk {
    #[allow(unused)]
    bytes: Vec<u8>,
}

impl Chunk {
    pub fn new() -> Self { Chunk { bytes: Vec::new() } }

    pub fn bytes(&self) -> &[u8] { &self.bytes }
    pub fn size(&self) -> usize { self.bytes.len() }

    pub fn append(&mut self, byte: impl Into<u8>) {
        self.bytes.push(byte.into())
    }

    pub fn disassemble(&self, name: &Path) -> String {
        let mut result = String::new();

        writeln!(result, "=== {} ===", name.display()).unwrap();
        // TODO: Disassebmel instructions
        result
    }
}
