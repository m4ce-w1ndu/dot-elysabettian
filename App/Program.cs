using VirtualMachine;

namespace App
{
    internal class Program
    {
        static void Repl(ExecutionEngine ee)
        {
            string? line;
            while (true)
            {
                Console.Write("> ");
                line = Console.ReadLine();

                if (line is null) continue;

                ee.Interpret(line);
            }
        }

        static void Main(string[] args)
        {
            var ee = new ExecutionEngine();
            Repl(ee);
        }
    }
}