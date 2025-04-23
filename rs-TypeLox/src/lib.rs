// #![warn(missing_docs)]

pub mod chunk;
pub mod value;
pub mod vm;

use std::io::BufRead;
use std::io::Write as _;
use std::io::stdin;
use std::io::stdout;
use std::path::Path;

use url::Url;

use crate::chunk::Chunk;
use crate::chunk::OpCode;
use crate::vm::LoxState;

pub type Result<T = (), E = anyhow::Error> = std::result::Result<T, E>;

pub fn run_file(file: &Path) -> crate::Result {
    eprintln!("running file '{}'", file.display());

    let mut chunk = Chunk::new();

    chunk.append(OpCode::RETURN);

    eprintln!("{}", chunk.disassemble(file));

    Ok(())
}

pub fn run_repl() -> crate::Result {
    let mut vm = LoxState::new();
    let mut line_no = 1;
    loop {
        // (P)rompt (???)
        let mut stdout = stdout().lock();
        write!(stdout, "> ").expect("failed to write to stdout");
        stdout.flush().expect("failed to flush stdout");

        // (R)ead
        let Some(Ok(line)) = stdin().lock().lines().next() else {
            break;
        };
        let line = line.trim_ascii();
        if line == ".exit" || line.starts_with('\x04') {
            break;
        }

        // (E)val
        let location = format!("eval:///line/{}", line_no)
            .parse::<Url>()
            .expect("failed to parse line uri");
        line_no += 1;

        let value = vm.run_string(line, &location)?;

        // (P)rint
        println!("{:?}", value);

        // (L)oop
    }
    Ok(())
}
