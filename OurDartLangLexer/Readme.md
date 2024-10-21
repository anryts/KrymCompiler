# National Technical University of Ukraine
## Igor Sikorsky Kyiv Polytechnic Institute
**Faculty of Informatics and Computing Technology**  
**Department of Informatics and Software Engineering**

**Project Participants**:  
Nazar Kharchuk  
Andriy Hrytsenko



---

## Yalta Programming Language Specification

### 1. Introduction

Yalta is an imperative general-purpose programming language. The syntax is based on Dart.  
The name is pronounced as "Yalta," in honor of the largest resort town in Crimea.

#### 1.1 Processing

A program written in Yalta is input into a translator for translation into the target language. The result is executed in the runtime system, which accepts input data and provides the program output.

The translation process includes the following phases:
- Lexical analysis
- Syntax analysis
- Semantic analysis
- Code generation

#### 1.2 Notation

For language description, we use an Extended Backus-Naur Form (EBNF). Syntax diagrams are presented using Wirth notation.

---

### 2. Lexical Structure

#### 2.1 Alphabet

A Yalta program can contain the following characters:  
`Letter` = `a` | `b` | ... | `z`  
`Digit` = `0` | `1` | ... | `9`  
`SpecSign` = `.` | `,` | `:` | `;` | `(` | `)` | `=` | `+` | `-` | `*` | `/` | `<` | `>` | `^` | `WhiteSpace` | `EndOfLine`

#### 2.2 Special Symbols

Tokens in Yalta include special symbols, identifiers, constants, and keywords.  
Examples:  
`AddOp` = `+` | `-`  
`RelOp` = `=` | `<=` | `>=`

#### 2.3 Identifiers

Identifiers represent variables and must start with a letter, followed by letters or digits.  
Examples: `numberOfInputs`, `a123`

#### 2.4 Constants

Yalta supports three types of literals:
- Integer (`IntNumb`)
- Real (`RealNumb`)
- Boolean (`BoolConst`)

Examples: `true`, `10`, `3.14`

#### 2.5 Keywords

Keywords in Yalta include:  
`main`, `int`, `double`, `bool`, `when`, `fallback`, `while`, `read`, `print`

#### 2.6 Tokens

Examples of tokens:
- `id`: Identifiers (e.g., `tmp`, `test_2`)
- `intnum`: Integer numbers (e.g., `10`)
- `realnum`: Real numbers (e.g., `3.14`)
- `keyword`: Reserved words (e.g., `main`, `int`)

#### 2.7 Comments

Single-line comments start with `//` and end at the line break.  
Syntax:  
`Comment = '//' {Letter | Digit } ('\n' | '\r\n')`

---

### 3. Data Types and Variables

#### 3.1 Data Types

Yalta supports three scalar types:
- `int`: Integer
- `double`: Real numbers
- `bool`: Boolean

#### 3.2 Variables

Each variable must be explicitly declared with a type and initialized during the declaration.

---

### 4. Variable Declaration

Syntax:  
`Declaration = Type Identifier "=" value ";"`  
Example:  
`int x = 5;`

---

### 5. Expressions

#### 5.1 Expression Grammar

There are arithmetic and boolean expressions in Yalta.  
Examples:  
`ArithmExpr = setPoint - actualValue`  
`BoolExpr = x > z`

#### 5.2 Operators

Operators are left-associative, except for exponentiation.  
Examples:
- `+`: Addition
- `*`: Multiplication
- `^`: Exponentiation

---

### 6. Statements

#### 6.1 Assignment Statement

Syntax:  
`AssignStatement = Identifier '=' Expression`

Example:  
`double pi = 3.14;`

#### 6.2 Input Statement

Syntax:  
`ReadStatement = read '(' Identifier ')'`

Example:  
`read(userInput);`

#### 6.3 Output Statement

Syntax:  
`PrintStatement = print '(' Identifier ')'`

Example:  
`print(a);`

#### 6.4 Conditional Statement

Yalta uses `when-fallback` for conditionals.  
Syntax:  
`IfStatement = when '(' BoolExpr ')' DoBlock [ fallback DoBlock ]`

Example:
```yalta
when (a > b) {
  print(a);
} fallback {
  print(b);
}
