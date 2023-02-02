using System;
using System.IO;

namespace Compiler.Common.Types
{
    /// <summary>
    /// Represents the way files are handled by the VM.
    /// </summary>
    public class File
    {
        /// <summary>
        /// Path to file.
        /// </summary>
        public string Path { get; init; }

        private FileStream? file;

        /// <summary>
        /// Constructs a new file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="mode"></param>
        public File(string path, string mode)
        {
            Path = path;

            var fileMode = mode switch
            {
                "r" => FileMode.Open,
                "w" => FileMode.Create,
                "a" => FileMode.Append,
                _ => throw new NotSupportedException("Unsupported file mode!")
            }; ;

            try { file = new FileStream(path, fileMode, FileAccess.ReadWrite); }
            catch { file = null; }
        }

        ~File()
        {
            if (file is null) return;
            file.Close();
            file.Dispose();
            file = null;
        }

        /// <summary>
        /// Allows to check if the file is properly open.
        /// </summary>
        public bool IsOpen { get => file is not null; }

        /// <summary>
        /// Closes the file.
        /// </summary>
        public void Close()
        {
            if (file is null) return;
            file.Close();
            file.Dispose();
            file = null;
        }

        /// <summary>
        /// Reads the content of the whole file.
        /// </summary>
        /// <returns>A string with the whole file's content.</returns>
        public string ReadAll()
        {
            if (!IsOpen) return "";

            using var reader = new StreamReader(file!);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Writes all data to file.
        /// </summary>
        /// <param name="data">Writes the whole string onto the file.</param>
        public void WriteAll(string data)
        {
            if (!IsOpen) return;

            using var writer = new StreamWriter(file!);
            writer.Write(data);
        }
    }
}
