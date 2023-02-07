using Compiler.Common;
using Compiler.Common.Types;
using System.Diagnostics.Contracts;
using Lexer;
using Lexer.Common;

namespace Compiler
{
    /// <summary>
    /// Parses source code and generates bytecode for the
    /// virtual machine
    /// </summary>
    public class Compiler
    {
        /// <summary>
        /// Last examined token
        /// </summary>
        public Token Previous { get; set; }

        /// <summary>
        /// Token being examined
        /// </summary>
        public Token Current { get; set; }

        /// <summary>
        /// Source code scanner and tokenizer
        /// </summary>
        public Scanner Scanner { get; set; }

        /// <summary>
        /// Symbols resolver, which generates the
        /// symbol table
        /// </summary>
        public Resolver Resolver { get; set; }

        /// <summary>
        /// Compiler utilities for classes
        /// </summary>
        public ClassCompiler? ClassCompiler { get; set; }

        /// <summary>
        /// Parser fatal error
        /// </summary>
        private bool hadError;

        /// <summary>
        /// Signals a fatal error already occured
        /// </summary>
        private bool panicMode;

        /// <summary>
        /// Parsing rules
        /// </summary>
        private readonly ParseRule[] rules;

        /// <summary>
        /// Constructs a new compiler
        /// </summary>
        /// <param name="source">Source code as input</param>
        public Compiler(string source)
        {
            Previous = new(TokenType.Eof, source, 0);
            Current = new(TokenType.Eof, source, 0);
            Scanner = new(source);
            ClassCompiler = null;
            hadError = false;
            panicMode = false;

            Resolver = new Resolver(this, FunctionType.Main, null);

            Advance();

            rules = new ParseRule[]
            {
                new ParseRule(Grouping, Call, Precedence.Call),     // (
                new ParseRule(null, null, Precedence.None),         // )
                new ParseRule(null, null, Precedence.None),         // {
                new ParseRule(null, null, Precedence.None),         // }
                new ParseRule(Array, ArrayIdx, Precedence.Or),      // [
                new ParseRule(Unary, null, Precedence.None),        // ]
                new ParseRule(null, null, Precedence.None),         // ,
                new ParseRule(null, Dot, Precedence.Call),         // .
                new ParseRule(null, null, Precedence.None),         // ;
                new ParseRule(Unary, null, Precedence.Unary),       // ~
                new ParseRule(null, Binary, Precedence.Term),       // ^
                new ParseRule(null, Binary, Precedence.Term),       // +
                new ParseRule(Binary, null, Precedence.Term),       // +=
                new ParseRule(Unary, Binary, Precedence.Term),      // -
                new ParseRule(Binary, null, Precedence.Term),       // -=
                new ParseRule(null, Binary, Precedence.Factor),     // *
                new ParseRule(Binary, null, Precedence.Factor),     // *=
                new ParseRule(null, Binary, Precedence.Factor),     // /
                new ParseRule(Binary, null, Precedence.Factor),     // /=
                new ParseRule(Unary, null, Precedence.None),        // !
                new ParseRule(null, Binary, Precedence.Equality),   // !=
                new ParseRule(null, null, Precedence.None),         // =
                new ParseRule(null, Binary, Precedence.Equality),   // ==
                new ParseRule(null, Binary, Precedence.Comparison), // >
                new ParseRule(null, Binary, Precedence.Comparison), // >=
                new ParseRule(null, Binary, Precedence.Comparison), // <
                new ParseRule(null, Binary, Precedence.Comparison), // <=
                new ParseRule(null, Binary, Precedence.Factor),     // &
                new ParseRule(null, And, Precedence.And),           // &&
                new ParseRule(null, Binary, Precedence.Term),       // |
                new ParseRule(null, Or, Precedence.Or),             // ||
                new ParseRule(Variable, null, Precedence.None),     // Variables
                new ParseRule(String, null, Precedence.None),       // Strings
                new ParseRule(Number, null, Precedence.None),       // Numbers
                new ParseRule(null, null, Precedence.None),         // class
                new ParseRule(null, null, Precedence.None),         // else
                new ParseRule(Literal, null, Precedence.None),      // false,
                new ParseRule(null, null, Precedence.None),         // func
                new ParseRule(null, null, Precedence.None),         // for
                new ParseRule(null, null, Precedence.None),         // if
                new ParseRule(null, null, Precedence.None),         // print
                new ParseRule(null, null, Precedence.None),         // return
                new ParseRule(Super, null, Precedence.None),        // super
                new ParseRule(This, null, Precedence.None),         // this
                new ParseRule(Literal, null, Precedence.None),      // true
                new ParseRule(null, null, Precedence.None),         // var
                new ParseRule(null, null, Precedence.None),         // while
                new ParseRule(null, null, Precedence.None),         // error
                new ParseRule(null, null, Precedence.None),         // Eof
            };
        }

        /// <summary>
        /// Compiles the code into bytecode for the
        /// Virtual Machine
        /// </summary>
        /// <returns>
        /// An executable function type or null if
        /// any error occured during code generation
        /// </returns>
        public Function? Compile()
        {
            while (!Match(TokenType.Eof))
                Declaration();

            var function = EndCompiler();

            if (hadError)
                return null;

            return function;
        }

        /// <summary>
        /// Advances the parser
        /// </summary>
        private void Advance()
        {
            Previous = Current;

            while (true)
            {
                Current = Scanner.ScanToken();
                if (Current.Type != TokenType.Error) break;

                ErrorAtCurrent(Current.Text);
            }
        }

        /// <summary>
        /// Consumes a token
        /// </summary>
        /// <param name="type">Type of the token to consume</param>
        /// <param name="message">Error message to print if unexpected token</param>
        private void Consume(TokenType type, string message)
        {
            if (Current.Type == type)
            {
                Advance();
                return;
            }

            ErrorAtCurrent(message);
        }

        /// <summary>
        /// Checks the type of the token
        /// </summary>
        /// <param name="type">Type of token</param>
        /// <returns>True if it matches, false otherwise</returns>
        [Pure]
        private bool Check(TokenType type)
        {
            return (Current.Type == type);
        }

        /// <summary>
        /// Checks the type of the token and advances parser
        /// </summary>
        /// <param name="type">Type of token</param>
        /// <returns>True if matches, false otherwise</returns>
        private bool Match(TokenType type)
        {
            if (!Check(type)) return false;
            Advance();
            return true;
        }

        /// <summary>
        /// Emits a new byte
        /// </summary>
        /// <param name="byte_">Byte to write</param>
        public void Emit(byte byte_)
        {
            CurrentChunk().Write(byte_, Previous.Line);
        }

        /// <summary>
        /// Emits a new instruction
        /// </summary>
        /// <param name="instr">Instruction to write</param>
        public void Emit(Instruction instr)
        {
            CurrentChunk().Write(instr, Previous.Line);
        }

        /// <summary>
        /// Emits an instruction with some data
        /// </summary>
        /// <param name="instr">Instruction to write</param>
        /// <param name="data">Data to write</param>
        public void Emit(Instruction instr, byte data)
        {
            Emit(instr);
            Emit(data);
        }

        /// <summary>
        /// Emits two instructions
        /// </summary>
        /// <param name="instr1">First instruction to write</param>
        /// <param name="instr2">Second instruction to write</param>
        public void Emit(Instruction instr1, Instruction instr2)
        {
            Emit(instr1);
            Emit(instr2);
        }

        /// <summary>
        /// Emits a loop
        /// </summary>
        /// <param name="loopStart">Start offset of the loop</param>
        private void EmitLoop(int loopStart)
        {
            Emit(Instruction.Branch);

            var offset = CurrentChunk().Count - loopStart + 2;
            if (offset >= ushort.MaxValue)
                Error("Loop body contains too many instructions.");

            Emit((byte)((offset >> 8) & 0xff));
            Emit((byte)(offset & 0xff));
        }

        /// <summary>
        /// Emits a jump
        /// </summary>
        /// <param name="instr">Jump instruction</param>
        /// <returns>Offset of the last padding byte</returns>
        private int EmitJump(Instruction instr)
        {
            Emit(instr);
            Emit(0xff);
            Emit(0xff);
            return CurrentChunk().Count - 2;
        }

        /// <summary>
        /// Emits a return
        /// </summary>
        private void EmitReturn()
        {
            if (Resolver.Type == FunctionType.Initializer)
                Emit(Instruction.GetLoc, (byte)0);
            else
                Emit(Instruction.Nop);

            Emit(Instruction.Ret);
        }

        /// <summary>
        /// Makes a new constant in memory (value initialization)
        /// </summary>
        /// <param name="value">Value of the constant</param>
        /// <returns>Offset of the new constant</returns>
        private byte MakeConstant(Value value)
        {
            var constant = CurrentChunk().AddConstant(value);
            if (constant > byte.MaxValue)
            {
                Error($"Chunks support at most {byte.MaxValue} constants.");
                return 0;
            }

            return (byte)constant;
        }

        /// <summary>
        /// Emits a new constant
        /// </summary>
        /// <param name="value">Value of the constant to emit</param>
        private void EmitConstant(Value value)
        {
            Emit(Instruction.Const, MakeConstant(value));
        }

        /// <summary>
        /// Patches a jump instruction
        /// </summary>
        /// <param name="offset">Offset of the jump</param>
        private void PatchJump(int offset)
        {
            var jump = CurrentChunk().Count - offset - 2;

            if (jump > ushort.MaxValue)
                Error("Jump is too long.");

            CurrentChunk().SetCode(offset, (byte)((jump >> 8) & 0xff));
            CurrentChunk().SetCode(offset + 1, (byte)(jump & 0xff));
        }

        /// <summary>
        /// Ends the compiler returning its compiled
        /// function
        /// </summary>
        /// <returns></returns>
        private Function EndCompiler()
        {
            EmitReturn();
            return Resolver.Function;
        }

        /// <summary>
        /// Parses array operations
        /// </summary>
        private void Array(bool canAssign)
        {
            int count = 0;

            if (!Check(TokenType.CloseSquare))
            {
                do
                {
                    if (Check(TokenType.CloseSquare)) break;
                    ParsePrecedence(Precedence.Or);

                    if (count == byte.MaxValue + 1)
                        Error("List literals do not allow more than 255 items.");

                    count++;
                }
                while (Match(TokenType.Comma));
            }

            Consume(TokenType.CloseSquare, "Expected ']' after list literal.");

            Emit(Instruction.ArrBuild);
            Emit((byte)count);
        }

        /// <summary>
        /// Parses array indexing
        /// </summary>
        private void ArrayIdx(bool canAssign)
        {
            ParsePrecedence(Precedence.Or);
            Consume(TokenType.CloseSquare, "Expected ']' after array index expression.");

            if (canAssign && Match(TokenType.Equal))
            {
                Expression();
                Emit(Instruction.ArrStore);
            }
            else
                Emit(Instruction.ArrIdx);
        }

        /// <summary>
        /// Parses a binary expression
        /// </summary>
        private void Binary(bool canAssign)
        {
            var operatorType = Previous.Type;
            var rule = GetRule(operatorType);
            ParsePrecedence(rule.Precedence + 1);

            switch (operatorType)
            {
                case TokenType.ExclEqual:       Emit(Instruction.Eq, Instruction.Not); break;
                case TokenType.EqualEqual:      Emit(Instruction.Eq); break;
                case TokenType.Greater:         Emit(Instruction.Gt); break;
                case TokenType.GreaterEqual:    Emit(Instruction.Lt, Instruction.Not); break;
                case TokenType.Less:            Emit(Instruction.Lt); break;
                case TokenType.LessEqual:       Emit(Instruction.Gt, Instruction.Not); break;
                case TokenType.Plus:            Emit(Instruction.Add); break;
                case TokenType.Minus:           Emit(Instruction.Sub); break;
                case TokenType.Star:            Emit(Instruction.Mul); break;
                case TokenType.Slash:           Emit(Instruction.Div); break;
                case TokenType.Pipe:            Emit(Instruction.BwOr); break;
                case TokenType.Amp:             Emit(Instruction.BwAnd); break;
                case TokenType.Caret:           Emit(Instruction.BwXor); break;
                default:                        return;
            }
        }

        /// <summary>
        /// Parses a call expression
        /// </summary>
        private void Call(bool canAssign)
        {
            var argCount = ArgsList();
            Emit(Instruction.Call, argCount);
        }

        /// <summary>
        /// Parses a Dot expression
        /// </summary>
        private void Dot(bool canAssign)
        {
            Consume(TokenType.Identifier, "Expected property name after '.'.");
            var name = IdentifierConstant(Previous.Text);

            if (canAssign && Match(TokenType.Equal))
            {
                Expression();
                Emit(Instruction.SetProp, (byte)name);
            }
            else if (Match(TokenType.OpenParen))
            {
                var count = ArgsList();
                Emit(Instruction.Invoke, (byte)name);
                Emit(count);
            }
            else
                Emit(Instruction.GetProp, (byte)name);
        }

        /// <summary>
        /// Parses a literal value or expression
        /// </summary>
        private void Literal(bool canAssign)
        {
            switch (Previous.Type)
            {
                case TokenType.False:       Emit(Instruction.False); break;
                case TokenType.True:        Emit(Instruction.True); break;
                case TokenType.Null:        Emit(Instruction.Nop); break;
                default:                    return;
            }
        }

        /// <summary>
        /// Parses a grouping expression
        /// </summary>
        private void Grouping(bool canAssign)
        {
            Expression();
            Consume(TokenType.CloseParen, "Expected ')' after expression.");
        }

        /// <summary>
        /// Parses a numerical expression
        /// </summary>
        private void Number(bool canAssign)
        {
            var value = double.Parse(Previous.Text);
            EmitConstant(value);
        }

        /// <summary>
        /// Parses an Or expression (boolean)
        /// </summary>
        private void Or(bool canAssign)
        {
            var elseJump = EmitJump(Instruction.JmpZ);
            var endJump = EmitJump(Instruction.Jmp);

            PatchJump(elseJump);
            Emit(Instruction.Pop);

            ParsePrecedence(Precedence.Or);
            PatchJump(endJump);
        }
      
        /// <summary>
        /// Parses a string expression
        /// </summary>
        private void String(bool canAssign)
        {
            var str = Previous.Text;
            str = str.Remove(0);
            str = str.Remove(str.Length - 1);
            EmitConstant(str);
        }

        /// <summary>
        /// Declares a named variable
        /// </summary>
        /// <param name="name">Name of the variable</param>
        /// <param name="canAssign">Can be assigned</param>
        private void NamedVariable(string name, bool canAssign)
        {
            Instruction getOp;
            Instruction setOp;

            var arg = Resolver.ResolveLocal(name);
            if (arg != -1)
            {
                getOp = Instruction.GetLoc;
                setOp = Instruction.SetLoc;
            } else if ((arg = Resolver.ResolveUpvalue(name)) != -1)
            {
                getOp = Instruction.GetUpv;
                setOp = Instruction.SetUpv;
            }
            else
            {
                arg = IdentifierConstant(name);
                getOp = Instruction.GetGlob;
                setOp = Instruction.SetGlob;
            }

            if (canAssign && Match(TokenType.Equal))
            {
                Expression();
                Emit(setOp, (byte)arg);
            }
            else
                Emit(getOp, (byte)arg);
        }

        /// <summary>
        /// Creates a new variable parsing the source code
        /// </summary>
        private void Variable(bool canAssign)
        {
            NamedVariable(Previous.Text, canAssign);
        }

        /// <summary>
        /// Parses a super (superclass) expression
        /// </summary>
        private void Super(bool canAssign)
        {
            if (ClassCompiler is null)
                Error("'super' cannot be invoked outside of a class instance.");
            else if (!ClassCompiler.HasSuperclass)
                Error("'super' cannot be invoked in classes without parent classes.");

            Consume(TokenType.Dot, "Expected '.' after 'super'.");
            Consume(TokenType.Identifier, "Expected superclass method name.");

            var name = IdentifierConstant(Previous.Text);

            NamedVariable("this", false);
            NamedVariable("super", false);
            Emit(Instruction.GetSup, (byte)name);
        }

        /// <summary>
        /// Parses this expression
        /// </summary>
        private void This(bool canAssign)
        {
            if (ClassCompiler is null)
            {
                Error("'this' cannot be referenced outside of a class body.");
                return;
            }

            Variable(false);
        }

        /// <summary>
        /// Parses And expressions
        /// </summary>
        private void And(bool canAssign)
        {
            var endJump = EmitJump(Instruction.JmpZ);

            Emit(Instruction.Pop);
            ParsePrecedence(Precedence.And);

            PatchJump(endJump);
        }

        /// <summary>
        /// Parses unary expressions
        /// </summary>
        private void Unary(bool canAssign)
        {
            var operatorType = Previous.Type;

            ParsePrecedence(Precedence.Unary);

            switch (operatorType)
            {
                case TokenType.Excl:        Emit(Instruction.Not); break;
                case TokenType.Minus:       Emit(Instruction.Neg); break;
                case TokenType.Tilde:       Emit(Instruction.BwNot); break;
                default:                    return;
            }
        }

        /// <summary>
        /// Calculates the parsing precedence
        /// </summary>
        private void ParsePrecedence(Precedence precedence)
        {
            Advance();
            var prefix = GetRule(Previous.Type).Prefix;
            if (prefix is null)
            {
                Error("Expected expression.");
                return;
            }

            var canAssign = precedence <= Precedence.Assignment;
            prefix(canAssign);

            while (precedence <= GetRule(Previous.Type).Precedence)
            {
                Advance();
                var infix = GetRule(Previous.Type).Infix;
                infix!(canAssign);
            }

            if (canAssign && Match(TokenType.Equal))
            {
                Error("Invalid assignment target.");
                Expression();
            }
        }

        /// <summary>
        /// Creates a new constant used as identifier
        /// </summary>
        private int IdentifierConstant(string name)
        {
            return MakeConstant(name);
        }

        /// <summary>
        /// Parses a variable
        /// </summary>
        private byte ParseVariable(string errorMessage)
        {
            Consume(TokenType.Identifier, errorMessage);

            Resolver.DeclareVariable(Previous.Text);
            if (Resolver.IsLocal) return 0;

            return (byte)IdentifierConstant(Previous.Text);
        }

        /// <summary>
        /// Defines a new variable
        /// </summary>
        private void DefineVariable(byte global)
        {
            if (Resolver.IsLocal)
            {
                Resolver.MarkInitialized();
                return;
            }

            Emit(Instruction.DefGlob, global);
        }

        /// <summary>
        /// Defines a new argument list
        /// </summary>
        private byte ArgsList()
        {
            byte argCount = 0;
            if (!Check(TokenType.CloseParen))
            {
                do
                {
                    Expression();
                    if (argCount == byte.MaxValue)
                        Error("Functions cannot have more than 255 parameters.");
                    argCount++;
                } while (Match(TokenType.Comma));
            }

            Consume(TokenType.CloseParen, "Expected ')' after arguments.");
            return argCount;
        }

        /// <summary>
        /// Expression parsing
        /// </summary>
        private void Expression()
        {
            ParsePrecedence(Precedence.Assignment);
        }

        /// <summary>
        /// Parses a block of statements
        /// </summary>
        private void Block()
        {
            while (!Check(TokenType.CloseCurly) && !Check(TokenType.Eof))
                Declaration();

            Consume(TokenType.CloseCurly, "Expected '}' after block.");
        }

        /// <summary>
        /// Parses a function
        /// </summary>
        private void Function(FunctionType type)
        {
            Resolver = new Resolver(this, type, Resolver);
            Resolver.BeginScope();

            Consume(TokenType.OpenParen, "Expected '(' after function name.");

            if (!Check(TokenType.CloseParen))
            {
                do
                {
                    Resolver.Function.Arity++;
                    if (Resolver.Function.Arity > byte.MaxValue)
                        ErrorAtCurrent("Functions cannot have more than 255 parameters.");

                    var constant = ParseVariable("Expected parameter name.");
                    DefineVariable(constant);
                } while (Match(TokenType.Comma));
            }

            Consume(TokenType.CloseParen, "Expected ')' after parameters.");
            Consume(TokenType.OpenCurly, "Expected '{' before function body.");
            Block();

            var function = EndCompiler();
            var newCompiler = Resolver;
            Resolver = newCompiler.Enclosing!;

            Emit(Instruction.Closure, MakeConstant(function));

            foreach (var upvalue in newCompiler.Upvalues)
            {
                Emit(upvalue.IsLocal ? (byte)0x1 : (byte)0x0);
                Emit(upvalue.Index);
            }
        }

        /// <summary>
        /// Parses a method
        /// </summary>
        private void Method()
        {
            Consume(TokenType.Identifier, "Expected method name.");
            var constant = IdentifierConstant(Previous.Text);
            var type = Previous.Text.Equals("init") ? FunctionType.Initializer : FunctionType.Method;
            Function(type);
            Emit(Instruction.Method, (byte)constant);
        }

        /// <summary>
        /// Parses a class declaration
        /// </summary>
        private void ClassDeclaration()
        {
            Consume(TokenType.Identifier, "Expected class name.");

            var className = Previous.Text;
            var nameConst = IdentifierConstant(className);
            Resolver.DeclareVariable(className);

            Emit(Instruction.Class, (byte)nameConst);
            DefineVariable((byte)nameConst);

            ClassCompiler = new ClassCompiler(ClassCompiler);

            if (Match(TokenType.Less))
            {
                Consume(TokenType.Identifier, "Expected superclass name.");
                Variable(false);

                if (className.Equals(Previous.Text))
                    Error("Classes cannot inherit from themselves.");

                Resolver.BeginScope();
                Resolver.AddLocal("super");
                DefineVariable(0);

                NamedVariable(className, false);
                Emit(Instruction.Inherit);
                ClassCompiler.HasSuperclass = true;
            }

            NamedVariable(className, false);
            Consume(TokenType.OpenCurly, "Expected '{' before class body.");

            while (!Check(TokenType.CloseCurly) && !Check(TokenType.Eof))
                Method();

            Consume(TokenType.CloseCurly, "Expected '}' after class body.");
            Emit(Instruction.Pop);

            if (ClassCompiler.HasSuperclass)
                Resolver.EndScope();

            ClassCompiler = ClassCompiler.Enclosing;
        }

        /// <summary>
        /// Parses a function declaration
        /// </summary>
        private void FuncDeclaration()
        {
            var global = ParseVariable("Expected function name.");
            Resolver.MarkInitialized();
            Function(FunctionType.Function);
            DefineVariable(global);
        }

        /// <summary>
        /// Parses a variable declaration
        /// </summary>
        private void VarDeclaration()
        {
            var global = ParseVariable("Expected variable name.");

            if (Match(TokenType.Equal))
                Expression();
            else
                Emit(Instruction.Nop);

            Consume(TokenType.Semicolon, "Expected ';' after variable declaration.");

            DefineVariable(global);
        }

        /// <summary>
        /// Parses an expression statement
        /// </summary>
        private void ExpressionStatement()
        {
            Expression();
            Emit(Instruction.Pop);
            Consume(TokenType.Semicolon, "Expected ';' after expression.");
        }

        /// <summary>
        /// Parses a for statement
        /// </summary>
        private void ForStatement()
        {
            Resolver.BeginScope();

            Consume(TokenType.OpenParen, "Expected '(' after 'for'.");
            if (Match(TokenType.Var))
                VarDeclaration();
            else if (Match(TokenType.Semicolon))
                ((Action)(() => { }))();
            else
                ExpressionStatement();

            var loopStart = CurrentChunk().Count;
            var exitJump = -1;

            if (!Match(TokenType.Semicolon))
            {
                Expression();
                Consume(TokenType.Semicolon, "Expected ';' after loop condition.");
                exitJump = EmitJump(Instruction.JmpZ);
                Emit(Instruction.Pop);
            }

            if (!Match(TokenType.CloseParen))
            {
                var bodyJump = EmitJump(Instruction.Jmp);
                var incStart = CurrentChunk().Count;

                Expression();
                Emit(Instruction.Pop);
                Consume(TokenType.CloseParen, "Expected ')' after for clauses.");

                EmitLoop(loopStart);
                loopStart = incStart;
                PatchJump(bodyJump);
            }

            Statement();

            EmitLoop(loopStart);

            if (exitJump != -1)
            {
                PatchJump(exitJump);
                Emit(Instruction.Pop);
            }

            Resolver.EndScope();
        }

        /// <summary>
        /// Parses an if statement
        /// </summary>
        private void IfStatement()
        {
            Consume(TokenType.OpenParen, "Expected '(' after 'if'.");
            Expression();
            Consume(TokenType.CloseParen, "Expected ')' after condition.");

            var then = EmitJump(Instruction.JmpZ);
            Emit(Instruction.Pop);
            Statement();
            var elseJump = EmitJump(Instruction.Jmp);

            PatchJump(then);
            Emit(Instruction.Pop);
            if (Match(TokenType.Else)) Statement();
            PatchJump(elseJump);
        }

        /// <summary>
        /// Parses a generic declaration
        /// </summary>
        private void Declaration()
        {
            if (Match(TokenType.Class))
                ClassDeclaration();
            else if (Match(TokenType.Func))
                FuncDeclaration();
            else if (Match(TokenType.Var))
                VarDeclaration();
            else
                Statement();

            if (panicMode) Sync();
        }

        /// <summary>
        /// Parses a generic statement
        /// </summary>
        private void Statement()
        {
            if (Match(TokenType.Print))
                PrintStatement();
            else if (Match(TokenType.For))
                ForStatement();
            else if (Match(TokenType.If))
                IfStatement();
            else if (Match(TokenType.Return))
                ReturnStatement();
            else if (Match(TokenType.While))
                WhileStatement();
            else if (Match(TokenType.OpenCurly))
            {
                Resolver.BeginScope();
                Block();
                Resolver.EndScope();
            }
            else
                ExpressionStatement();
        }

        /// <summary>
        /// Parses a print statement
        /// </summary>
        private void PrintStatement()
        {
            Expression();
            Consume(TokenType.Semicolon, "Expected ';' after value.");
            Emit(Instruction.Pnt);
        }

        /// <summary>
        /// Parses a return statement
        /// </summary>
        private void ReturnStatement()
        {
            if (Resolver.Type == FunctionType.Main)
                Error("Cannot return from main script.");

            if (Match(TokenType.Semicolon))
                EmitReturn();
            else
            {
                if (Resolver.Type == FunctionType.Initializer)
                    Error("Cannot return value from object initializer.");

                Expression();
                Consume(TokenType.Semicolon, "Expected ';' after return value.");
                Emit(Instruction.Ret);
            }
        }

        /// <summary>
        /// Parses a while statement
        /// </summary>
        private void WhileStatement()
        {
            var loopStart = CurrentChunk().Count;

            Consume(TokenType.OpenParen, "Expected '(' after 'while'.");
            Expression();
            Consume(TokenType.CloseParen, "Expected ')' after condition.");

            var exit = EmitJump(Instruction.JmpZ);

            Emit(Instruction.Pop);
            Statement();

            EmitLoop(loopStart);

            PatchJump(exit);
            Emit(Instruction.Pop);
        }

        /// <summary>
        /// Syncs the status of the Compiler after an error
        /// occured
        /// </summary>
        private void Sync()
        {
            panicMode = false;

            while (Current.Type != TokenType.Eof)
            {
                if (Previous.Type == TokenType.Semicolon) return;

                switch (Current.Type)
                {
                    case TokenType.Class:
                    case TokenType.Func:
                    case TokenType.If:
                    case TokenType.While:
                    case TokenType.Print:
                    case TokenType.Return:
                        return;
                    default: break;
                }

                Advance();
            }
        }

        /// <summary>
        /// Returns the parsing rule corresponding with the
        /// specific token
        /// </summary>
        private ParseRule GetRule(TokenType type)
        {
            return rules[(int)type];
        }

        /// <summary>
        /// Signals an error at a specific token
        /// </summary>
        private void ErrorAt(Token token, string message)
        {
            if (panicMode) return;

            panicMode = true;

            Console.Error.Write($"[line {token.Line} Error");
            if (token.Type == TokenType.Eof)
                Console.Error.Write(" at end");
            else if (token.Type == TokenType.Error)
                ((Action)(() => { }))();
            else
                Console.Error.Write($" at '{token.Text}'");

            Console.Error.Write($": {message}\n");
            hadError = true;
        }

        /// <summary>
        /// Signals an error parsing the last token
        /// </summary>
        public void Error(string message)
        {
            ErrorAt(Previous, message);
        }

        /// <summary>
        /// Signals an error parsing the current token
        /// </summary>
        public void ErrorAtCurrent(string message)
        {
            ErrorAt(Current, message);
        }

        /// <summary>
        /// Returns a reference to the current chunk
        /// of active memory
        private Chunk CurrentChunk()
        {
            return Resolver.Function.Chunk;
        }
    }
}
