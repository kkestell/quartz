# Zircon

Zircon is a stack-based virtual machine written in C#, loosely based on the design presented in [Crafting Interpreters](https://craftinginterpreters.com/) by Robert Nystrom. It features support for functions, arrays, and a complete object-oriented system with classes and methods.

## Bytecode

### Overview

Zircon Bytecode is a binary format used for the execution of programs in the Zircon virtual machine. All multi-byte values in the Zircon Bytecode file are stored in little-endian byte order.

### File Structure

A Zircon Bytecode file consists of a header, a constants table, a functions section, and a classes section.

#### Header

* **Magic Number**: `ZRCN` (4 bytes)
* **Version**: 1 byte

#### Constants Table

* **Number of Constants**: 4 bytes (unsigned int)
* **Constants**: A sequence of entries, each with a type specifier followed by the constant value.
    * **Number**: 1-byte type specifier (`0x01`) + 8 bytes for a double-precision floating-point value.
    * **Boolean**: 1-byte type specifier (`0x02`) + 1 byte for the boolean value (0 for false, non-zero for true).
    * **String**: 1-byte type specifier (`0x03`) + 2 bytes (unsigned short) for the string length in bytes + N bytes for the UTF-8 encoded string.

#### Functions Section

* **Number of Functions**: 4 bytes (unsigned int)
* **Functions**: A sequence of function definitions, each consisting of:
    * **Number of Arguments**: 4 bytes (int)
    * **Number of Instructions**: 4 bytes (unsigned int)
    * **Instructions**: A sequence of instruction bytes.

#### Classes Section

* **Number of Classes**: 4 bytes (unsigned int)
* **Classes**: A sequence of class definitions, each consisting of:
    * **Name Index**: 2 bytes (ushort) - Index into the constants table for the class name.
    * **Constructor Index**: 4 bytes (int) - Index into the functions table for the constructor.
    * **Number of Methods**: 2 bytes (ushort)
    * **Methods**: A sequence of method definitions, each consisting of:
        * **Method Name Index**: 2 bytes (ushort) - Index into the constants table.
        * **Function Index**: 4 bytes (int) - Index into the functions table.

### Instructions

| Opcode               | Hex Value | Operand(s)                                   | Description                                                                    |
| -------------------- | --------- | -------------------------------------------- | ------------------------------------------------------------------------------ |
| `PushConst`          | `0x01`    | 2-byte constant index                        | Pushes a specified constant onto the stack.                                    |
| `Pop`                | `0x02`    | None                                         | Pops the top value from the stack.                                             |
| `Dup`                | `0x03`    | None                                         | Duplicates the top value on the stack.                                         |
| `Swap`               | `0x04`    | None                                         | Swaps the top two values on the stack.                                         |
| `Add`                | `0x10`    | None                                         | Adds the top two values on the stack, pushing the result.                      |
| `Subtract`           | `0x11`    | None                                         | Subtracts the top stack value from the second, pushing the result.             |
| `Multiply`           | `0x12`    | None                                         | Multiplies the top two stack values, pushing the result.                       |
| `Divide`             | `0x13`    | None                                         | Divides the second top stack value by the top, pushing the result.             |
| `Modulo`             | `0x14`    | None                                         | Calculates the modulus of the second top value by the top, pushing the result. |
| `Negate`             | `0x15`    | None                                         | Negates the top value on the stack, pushing the result.                        |
| `And`                | `0x20`    | None                                         | Performs a logical AND on the top two stack values, pushing the result.        |
| `Or`                 | `0x21`    | None                                         | Performs a logical OR on the top two stack values, pushing the result.         |
| `Not`                | `0x22`    | None                                         | Performs a logical NOT on the top stack value, pushing the result.             |
| `Equal`              | `0x30`    | None                                         | Checks if the top two stack values are equal, pushing the boolean result.      |
| `GreaterThan`        | `0x31`    | None                                         | Compares if the second stack value is > the top, pushing the result.         |
| `LessThan`           | `0x32`    | None                                         | Compares if the second stack value is < the top, pushing the result.         |
| `Jump`               | `0x40`    | 2-byte target address                        | Unconditionally jumps to the specified instruction address.                    |
| `JumpIfTrue`         | `0x41`    | 2-byte target address                        | Jumps to the address if the top stack value is true, popping the value.        |
| `JumpIfFalse`        | `0x42`    | 2-byte target address                        | Jumps to the address if the top stack value is false, popping the value.       |
| `NewArray`           | `0x50`    | 2-byte item count                            | Creates an array from the top N items on the stack.                            |
| `ArrayGet`           | `0x51`    | None                                         | Gets an element from an array at a specified index.                            |
| `ArraySet`           | `0x52`    | None                                         | Sets an element in an array at a specified index.                              |
| `ArrayLength`        | `0x53`    | None                                         | Pushes the length of an array onto the stack.                                  |
| `Print`              | `0x60`    | None                                         | Prints the top value of the stack and pops it.                                 |
| `GetLocal`           | `0x70`    | 2-byte variable index                        | Pushes the value of a local variable onto the stack.                           |
| `SetLocal`           | `0x71`    | 2-byte variable index                        | Sets a local variable to the top stack value, popping it.                      |
| `GetGlobal`          | `0x72`    | 2-byte variable index                        | Pushes the value of a global variable onto the stack.                          |
| `SetGlobal`          | `0x73`    | 2-byte variable index                        | Sets a global variable to the top stack value, popping it.                     |
| `Call`               | `0x80`    | 2-byte function index                        | Calls a function, setting up a new call frame.                                 |
| `Return`             | `0x81`    | None                                         | Returns from the current function.                                             |
| `NewInstance`        | `0x90`    | 2-byte class index                           | Creates a new instance of a class and calls its constructor.                   |
| `GetField`           | `0x91`    | 2-byte field name index                      | Gets a field from an instance and pushes it onto the stack.                    |
| `SetField`           | `0x92`    | 2-byte field name index                      | Sets a field on an instance to the top stack value.                            |
| `CallMethod`         | `0x93`    | 2-byte name index + 1-byte arg count       | Calls a method on an instance.                                                 |
| `Halt`               | `0xFF`    | None                                         | Halts VM execution.                                                            |

## Source

The source code for Zircon is available on [GitHub](https://github.com/kkestell/zircon).
