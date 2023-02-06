using Compiler.Common;
using Compiler.Common.Types;
using Lexer;
using Lexer.Common;
using System.Diagnostics.Contracts;

namespace Compiler
{
    public class Parser
    {
        public Token Previous { get; set; }

        public Token Current { get; set; }

        public Scanner Scanner { get; set; }

        public Resolver Resolver { get; set; }

        public ClassCompiler? ClassCompiler { get; set; }

        private bool hadError;

        private bool panicMode;

        public Parser(string source)
        {
            Previous = new(TokenType.Eof, source, 0);
            Current = new(TokenType.Eof, source, 0);
            Scanner = new(source);
            ClassCompiler = null;
            hadError = false;
            panicMode = false;

            Resolver = new Resolver(this, FunctionType.Main, null);

            Advance();

            // BuildParseRules();
        }

        public Function? Compile()
        {
            return null;
        }

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

        [Pure]
        private bool Check(TokenType type)
        {
            return (Current.Type == type);
        }

        private bool Match(TokenType type)
        {
            if (!Check(type)) return false;
            Advance();
            return true;
        }

        public void Emit(byte byte_)
        {
            CurrentChunk().Write(byte_, Previous.Line);
        }

        public void Emit(Instruction instr)
        {
            CurrentChunk().Write(instr, Previous.Line);
        }

        public void Emit(Instruction instr, byte data)
        {
            Emit(instr);
            Emit(data);
        }

        public void Emit(Instruction instr1, Instruction instr2)
        {
            Emit(instr1);
            Emit(instr2);
        }

        private void EmitLoop(int loopStart)
        {
            Emit(Instruction.Branch);

            var offset = CurrentChunk().Count - loopStart + 2;
            if (offset >= ushort.MaxValue)
                Error("Loop body contains too many instructions.");

            Emit((byte)((offset >> 8) & 0xff));
            Emit((byte)(offset & 0xff));
        }

        private int EmitJump(Instruction instr)
        {
            Emit(instr);
            Emit(0xff);
            Emit(0xff);
            return CurrentChunk().Count - 2;
        }

        private void EmitReturn()
        {
            if (Resolver.Type == FunctionType.Initializer)
                Emit(Instruction.GetLoc, (byte)0);
            else
                Emit(Instruction.Nop);

            Emit(Instruction.Ret);
        }

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

        private void EmitConstant(Value value)
        {
            Emit(Instruction.Const, MakeConstant(value));
        }

        private void PatchJump(int offset)
        {
            var jump = CurrentChunk().Count - offset - 2;

            if (jump > ushort.MaxValue)
                Error("Jump is too long.");

            CurrentChunk().SetCode(offset, (byte)((jump >> 8) & 0xff));
            CurrentChunk().SetCode(offset + 1, (byte)(jump & 0xff));
        }

        private Function EndCompiler()
        {
            EmitReturn();
            return Resolver.Function;
        }

        private void Array(bool canAssign)
        {
            int count = 0;

            if (!Check(TokenType.CloseSquare))
            {
                do
                {
                    if (Check(TokenType.CloseSquare)) break;
                    // ParsePrecedence(Precedence.Or);

                    if (count == byte.MaxValue + 1)
                        Error("List literals do not allow more than 255 items.");

                    count++;
                }
                while (Match(TokenType.Comma));
            }

            // Consume(TokenType.Square, "Expected ']' after list literal.");

            Emit(Instruction.ArrBuild);
            Emit((byte)count);
        }

        public void Error(string message)
        {
            
        }

        public void ErrorAtCurrent(string message)
        {

        }

        private Chunk CurrentChunk()
        {
            return Resolver.Function.Chunk;
        }
    }
}
