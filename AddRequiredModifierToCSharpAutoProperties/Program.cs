//
//
//

using System.Text;

namespace AddRequiredModifierToCSharpAutoProperties
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Get information about the root directory.
            if (args == null || args.Length == 0 || String.IsNullOrWhiteSpace(args[0]))
            {
                Console.WriteLine("Please specify the fully-qualified pathname of the solution or project-root folder " +
                    "as the first and only argument.");
                return;
            }

            var pathname = args[0];

            DirectoryInfo rootDir;
            try
            {
                rootDir = new DirectoryInfo(pathname);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get information about directory '{pathname}'.  {ex.Message}");
                return;
            }

            if (rootDir.Exists is false)
            {
                Console.WriteLine($"Directory '{pathname}' doesn't exist.");
                return;
            }


            // Get all .cs files in the root directory and its subdirectories (recursive).
            FileInfo[] csharpFiles;
            try
            {
                csharpFiles = rootDir.EnumerateFiles("*.cs", SearchOption.AllDirectories).ToArray();

                if (csharpFiles.Length == 0)
                {
                    Console.WriteLine("No C# source code files were found.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get all C# files from the specified directory and sub-directories.  {ex.Message}");
                return;
            }

            
            // Process each file.
            foreach (var file in csharpFiles)
            {
                ProcessCsharpFile(file);
            }
        }

        // Adds the required modifier to all appropriate auto-properties found in the specified C# file.
        // Only considers those auto-properties that are declared on one line only, and that are correctly formatted.
        // Throws one of a number of exceptions if this fails.
        static void ProcessCsharpFile(FileInfo file)
        {
            var lines = File.ReadAllLines(file.FullName);

            var linesWithSetOrInit = lines.Where(LineContainsPropSetOrInit).ToArray();

            if (linesWithSetOrInit.Length == 0)
                return;

            var modified = false;

            String line;
            String[] tokens;
            Int32 length;
            Int32 publicOrInternalIdx;
            for (var i = 0; i < lines.Length; ++i)
            {
                tokens = TokenizeLine(lines[i]);

                if (IsViableProperty(tokens) is false)
                    continue;

                line = lines[i];

                publicOrInternalIdx = line.IndexOf("public ", StringComparison.Ordinal);
                length = "public ".Length;
                if (publicOrInternalIdx < 0)
                {
                    publicOrInternalIdx = line.IndexOf("internal ", StringComparison.Ordinal);
                    if (publicOrInternalIdx < 0)
                        continue;
                    length = "internal ".Length;
                }

                lines[i] = line.Insert(publicOrInternalIdx + length, "required ");
                modified = true;
            }

            if (modified is true)
            {
                File.WriteAllLines(file.FullName, lines, Encoding.UTF8);
            }
        }

        static Boolean LineContainsPropSetOrInit(String line)
        {
            var sc = StringComparison.Ordinal;
            return line.Contains(" set; ", sc) || line.Contains(" init; ", sc);
        }

        static String[] TokenizeLine(String line)
        {
            return line.Split(' ', StringSplitOptions.TrimEntries);
        }

        static Boolean IsViableProperty(String[] tokens)
        {
            var disallowedKeywords = new String[]
            {
                "protected",
                "private",
                "required",     // i.e. the property is already required
                "static"
            };

            // must have at least five tokens
            if (tokens == null || tokens.Length < 5)
                return false;

            // no token can be a disallowed keyword
            if (tokens.Any(token => disallowedKeywords.Any(kw => kw == token)))
                return false;

            // property must be marked as public or internal
            if (Array.IndexOf(tokens, "public") < 0 && Array.IndexOf(tokens, "internal") < 0)
                return false;

            // get the { of the auto-property
            var lbraceIdx = Array.IndexOf(tokens, "{");
            if (lbraceIdx < 0)
                return false;

            // get the } of the auto-property
            var rbraceIdx = Array.IndexOf(tokens, "}");
            if (rbraceIdx < 0)
                return false;

            // check order!
            if (rbraceIdx <= lbraceIdx)
                return false;

            // get the index of the "set;" or the "init;" token
            var setOrInitIdx = Array.IndexOf(tokens, "set;");
            if (setOrInitIdx < 0)
            {
                setOrInitIdx = Array.IndexOf(tokens, "init;");
                if (setOrInitIdx < 0)
                    return false;
            }

            // correct order -> we'll accept this tokenized line
            if (lbraceIdx < setOrInitIdx && setOrInitIdx < rbraceIdx)
                return true;

            return false;
        }
    }
}