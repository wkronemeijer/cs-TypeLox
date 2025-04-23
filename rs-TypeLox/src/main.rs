use std::path::PathBuf;
use std::process::ExitCode;

use clap::Parser;
use clap::Subcommand;
use clap::ValueEnum;
use strum::Display;
pub use typelox::Result;
use typelox::run_file;
use typelox::run_repl;

#[derive(Debug, Default, Clone, Copy, ValueEnum, Display)]
#[clap(rename_all = "kebab-case")]
#[strum(serialize_all = "kebab-case")]
pub enum LangVersion {
    Lox,
    #[default]
    TypeLox2025,
}

/// Which specific subcommand to run
#[derive(Debug, Clone, Subcommand)]
#[deny(missing_docs)]
pub enum CliMode {
    /// Start a REPL.
    Repl,
    /// Run a file.
    Run {
        /// The location of the file to run.
        file: PathBuf,
    },
    /// Find then run a set of files as tests.
    Test {
        /// The search string for tests. Defaults to '*.test.lox'.
        #[arg(long)]
        filter: Option<String>,
        /// Where to look for tests. Defaults to '.'.
        directory: Option<PathBuf>,
    },
}

/// Set of tools to run and develop Lox.
#[derive(Debug, Clone, Parser)]
#[command(version, about)]
#[deny(missing_docs)]
pub struct Cli {
    /// Which command to run.
    #[command(subcommand)]
    mode: Option<CliMode>,

    /// File to run directly (supported for compatibility).
    file: Option<PathBuf>,
}

impl Cli {
    pub fn mode(self) -> CliMode {
        if let Some(mode) = self.mode {
            mode
        } else if let Some(file) = self.file {
            CliMode::Run { file }
        } else {
            CliMode::Repl
        }
    }
}

fn start(options: Cli) -> crate::Result {
    match options.mode() {
        // Execution
        CliMode::Run { file } => run_file(&file)?,
        CliMode::Repl => run_repl()?,

        // Tools
        CliMode::Test { .. } => {
            todo!("TODO: test subcommand");
        },
    }
    Ok(())
}

fn main() -> ExitCode {
    match start(Cli::parse()) {
        Ok(()) => ExitCode::SUCCESS,
        // TODO: Read https://docs.rs/anyhow/latest/anyhow/struct.Error.html#display-representations
        // to include causes as well
        Err(e) => {
            eprint!("\x1B[31m{}", e);
            let mut causes = e.chain();
            if let Some(first) = causes.next() {
                eprintln!("\ncaused by:");
                eprintln!("{}", first);
                for rest in causes {
                    eprintln!("caused by:");
                    eprintln!("{}", rest);
                }
            }
            eprintln!("\x1B[39m");
            ExitCode::FAILURE
        },
    }
}
