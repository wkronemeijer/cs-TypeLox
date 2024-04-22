# TypeLox

Implement Lox using a JavaScript transpiler, 
then add type annotations on top of it like TypeScript.
Bonus points if it can generate valid `.d.ts`.

This is intended as a trail-run for a custom language
But since that one is very much in flux, 
we pick a simple(r) target to test on.

## Goals 

- Scan Lox
- Parse Lox
- Resolve Lox
- Run Lox by treewalker
- Format Lox by CST
- Run Lox by bytecode
- Run Lox by transpiler to Lua
- Run Lox by transpiler to JS

Fun note for the transpilers: 
the test suite includes 
```lox
print nil; // expect: nil
```
...which is going to be really fun when trying to transpile.
