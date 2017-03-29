using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TwoPassAssembler
{
    class Program
    {
        static string programName;
        static int programSize;
        static int locationCounter = 0;
        static int startingAddress = 0;
        static Instructions[] instructions;
        static string[] directives;
        static List<Symbols> symTable = new List<Symbols>();
        static void Main(string[] args)
        {
            ResetFiles();

            CreateInstructions();
            CreateDirectives();

            Pass_1();

            Pass_2();

            Console.ReadLine();
        }

        static void CreateInstructions()
        {
            instructions = new Instructions[13];
            instructions[0] = new Instructions() { opCode = 1, opName = "LOAD" };
            instructions[1] = new Instructions() { opCode = 2, opName = "STORE" };
            instructions[2] = new Instructions() { opCode = 3, opName = "ADD" };
            instructions[3] = new Instructions() { opCode = 4, opName = "SUBT" };
            instructions[4] = new Instructions() { opCode = 5, opName = "INPUT" };
            instructions[5] = new Instructions() { opCode = 6, opName = "OUTPUT" };
            instructions[6] = new Instructions() { opCode = 7, opName = "HALT" };
            instructions[7] = new Instructions() { opCode = 8, opName = "SKIPCOND" };
            instructions[8] = new Instructions() { opCode = 9, opName = "JUMP" };
            instructions[9] = new Instructions() { opCode = 10, opName = "CLEAR" };
            instructions[10] = new Instructions() { opCode = 11, opName = "ADDI" };
            instructions[11] = new Instructions() { opCode = 12, opName = "JUMPI" };
            instructions[12] = new Instructions() { opCode = 13, opName = "SUBTI" };
        }

        static void CreateDirectives()
        {
            directives = new string[8];
            directives[0] = "HEX";
            directives[1] = "DEC";
            directives[2] = "PUBLIC";
            directives[3] = "EXTERN";
            directives[4] = "ORG";
            directives[5] = "DEFINE";
            directives[6] = "TITLE";
            directives[7] = "DUP";
        }

        static void Pass_1()
        {
            //string assemblyCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"E:\Learning\Diploma\System-Programming\projects\TwoPassAssembler\Assembler\TwoPassAssembler\Files\assemblyCode.txt");
            string assemblyCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\assemblyCode.txt");

            
            using (var reader = new StreamReader(assemblyCode))
            {
                string line;
                string interLine = "";

                while ((line = reader.ReadLine()) != null)
                {
                    string[] lineWords = line.Split(' ');
                    string word_1, word_2, word_3;
                    string label, opCode, directive, value, operand;

                    int count = lineWords.Length;

                    if (count == 1)
                    {
                        opCode = lineWords[0];

                        bool isInstruction = instructions.Any(a => a.opName.Equals(opCode));

                        if (isInstruction)
                        {
                            //Write something to intermediate file 
                            interLine = string.Format("{0:x3}", locationCounter) + " " + line;
                            SaveToIntermediateFile(interLine);
                            locationCounter++;
                        }
                        else
                        {
                            Console.WriteLine("Invalid Instruction.");
                        }
                    }
                    else if (count == 2)
                    {
                        word_1 = lineWords[0];
                        word_2 = lineWords[1];

                        bool isInstruction = instructions.Any(a => a.opName.Equals(word_1));
                        bool isDirective = directives.Any(a => a.Equals(word_1));

                        if (isInstruction)
                        {
                            opCode = word_1;
                            operand = word_2;
                            //Write something to intermediate file interLine =
                            interLine = string.Format("{0:x3}", locationCounter) + " " + line;
                            SaveToIntermediateFile(interLine);
                            locationCounter++;
                        }
                        else if (isDirective)
                        {
                            directive = word_1;

                            if (directive == "EXTERN")
                            {
                                operand = word_2;

                                bool isLabelExist = symTable.Any(a => a.symName.Equals(operand));

                                if (isLabelExist)
                                {
                                    Console.WriteLine("Duplicate symbol.");
                                }
                                else
                                {
                                    symTable.Add(new Symbols()
                                    {
                                        symAddress = 0,
                                        symName = operand,
                                        isRelative = true
                                    });

                                    interLine = string.Format("{0:x3}", locationCounter) + " " + line;
                                    SaveToIntermediateFile(interLine);
                                    locationCounter++;
                                }
                            }
                            else if (directive == "ORG")
                            {
                                value = word_2;
                                startingAddress = Convert.ToInt32(value, 16);
                                locationCounter = startingAddress;
                                //Write something to intermediate file interLine =
                                interLine = string.Format("{0:x3}", locationCounter) + " " + line;
                                SaveToIntermediateFile(interLine);
                                //locationCounter++;
                            }
                            else if (directive == "TITLE")
                            {
                                value = word_2;

                                programName = value;

                                //Write something to intermediate file interLine =
                                interLine = string.Format("{0:x3}", locationCounter) + " " + line;
                                SaveToIntermediateFile(interLine);

                                locationCounter++;
                            }
                            else if (directive == "DEFINE")
                            {
                                string[] splitWord_2 = word_2.Split('=');
                                if (splitWord_2.Length == 2)
                                {
                                    operand = splitWord_2[0];
                                    value = splitWord_2[1];
                                }
                                //Write something to intermediate file interLine =
                                interLine = string.Format("{0:x3}", locationCounter) + " " + line;
                                SaveToIntermediateFile(interLine);
                                locationCounter++;
                            }
                            else if (directive == "PUBLIC")
                            {
                                //Write something to intermediate file interLine =
                                interLine = string.Format("{0:x3}", locationCounter) + " " + line;
                                SaveToIntermediateFile(interLine);
                                locationCounter++;
                            }

                        }
                        else //label
                        {
                            label = word_1;

                            bool isLabelExist = symTable.Any(a => a.symName.Equals(label));

                            if (isLabelExist)
                            {
                                Console.WriteLine("Duplicate symbol.");
                            }
                            else
                            {
                                symTable.Add(new Symbols()
                                {
                                    symAddress = locationCounter,
                                    symName = label,
                                    isRelative = true
                                });
                            }
                            //Write something to intermediate file interLine =
                            interLine = string.Format("{0:x3}", locationCounter) + " " + line;
                            SaveToIntermediateFile(interLine);
                            locationCounter++;
                            
                        }
                    }
                    else if (count == 3)
                    {
                        label = lineWords[0];

                        bool isLabelExist = symTable.Any(a => a.symName.Equals(label));

                        if (isLabelExist)
                        {
                            Console.WriteLine("Duplicate symbol.");
                        }
                        else
                        {
                            symTable.Add(new Symbols()
                            {
                                symAddress = locationCounter,
                                symName = label,
                                isRelative = true
                            });

                            //Write something to intermediate file interLine =
                            interLine = string.Format("{0:x3}", locationCounter) + " " + line;
                            SaveToIntermediateFile(interLine);
                        }

                        word_2 = lineWords[1];
                        word_3 = lineWords[2];

                        bool isInstruction = instructions.Any(a => a.opName.Equals(word_2));
                        bool isDirective = directives.Any(a => a.Equals(word_2));

                        if (isInstruction)
                        {
                            opCode = word_2;
                            operand = word_3;
                            locationCounter++;
                        }
                        else if (isDirective)
                        {
                            directive = word_2;
                            value = word_3;
                            if (directive == "DUP")
                            {
                                int i = Convert.ToInt32(value, 16);
                                //Write something to intermediate file interLine =
                                locationCounter += i;
                            }
                            else
                            {
                                //Write something to intermediate file interLine =
                                locationCounter++;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid Assembly statement.");
                        }

                    }
                    else
                    {
                        Console.WriteLine("Invalid Assembly statement.");
                    }

                }

                programSize = locationCounter;

                SaveToSymbleTable();
            }
        }

        static void Pass_2()
        {
            //string assemblyCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"E:\Learning\Diploma\System-Programming\projects\TwoPassAssembler\Assembler\TwoPassAssembler\Files\assemblyCode.txt");
            string assemblyCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\assemblyCode.txt");

            string output = "";

            output = "H " + (string.IsNullOrEmpty(programName) ? "" : programName + " ") + string.Format("{0:x4}", startingAddress) + " " + string.Format("{0:x4}", programSize);

            SaveToObjectFile(output);

            using (var reader = new StreamReader(assemblyCode))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    string[] lineWords = line.Split(' ');
                    string word_1, word_2, word_3;
                    string label, opCode, directive, value, operand;

                    int count = lineWords.Length;

                    if (count == 1)
                    {
                        opCode = lineWords[0];

                        bool isInstruction = instructions.Any(a => a.opName.Equals(opCode));

                        if (isInstruction)
                        {
                            int code = GetInstructionCode(opCode);
                            output = string.Format("{0:x}", code) + string.Format("{0:x3}", 0);
                            SaveToObjectFile(output);
                        }
                        else
                        {
                            Console.WriteLine("Invalid Instruction.");
                        }
                    }
                    else if (count == 2)
                    {
                        word_1 = lineWords[0];
                        word_2 = lineWords[1];

                        bool isInstruction = instructions.Any(a => a.opName.Equals(word_1));
                        bool isDirective = directives.Any(a => a.Equals(word_1));

                        if (isInstruction)
                        {
                            opCode = word_1;
                            operand = word_2;

                            int code = GetInstructionCode(opCode);
                            int address = GetSymbolAddress(operand);

                            output = string.Format("{0:x}", code) + string.Format("{0:x3}", address);
                            SaveToObjectFile(output);
                        }
                        else if (isDirective)
                        {
                            directive = word_1;

                            if (directive == "EXTERN")
                            {
                                operand = word_2;

                                int address = GetSymbolAddress(operand);
                                output = string.Format("{0:x4}", address);
                                SaveToObjectFile(output);
                            }
                            else if (directive == "ORG")
                            {
                                
                            }
                            else if (directive == "TITLE")
                            {
                                
                            }
                            else if (directive == "DEFINE")
                            {
                                
                            }
                            else if (directive == "PUBLIC")
                            {
                                operand = word_2;

                                int address = GetSymbolAddress(operand);
                                output = string.Format("{0:x4}", address);
                                SaveToObjectFile(output);
                            }
                        }
                        else //label
                        {
                            label = word_1;

                            value = word_2;

                            output = string.Format("{0:x4}", int.Parse(value));
                            SaveToObjectFile(output);

                        }
                    }
                    else if (count == 3)
                    {
                        label = lineWords[0];

                        word_2 = lineWords[1];
                        word_3 = lineWords[2];

                        bool isInstruction = instructions.Any(a => a.opName.Equals(word_2));
                        bool isDirective = directives.Any(a => a.Equals(word_2));

                        if (isInstruction)
                        {
                            opCode = word_2;
                            operand = word_3;

                            int code = GetInstructionCode(opCode);
                            int address = GetSymbolAddress(operand);

                            output = string.Format("{0:x}", code) + string.Format("{0:x3}", address);
                            SaveToObjectFile(output);
                        }
                        else if (isDirective)
                        {
                            directive = word_2;
                            value = word_3;
                            if (directive == "DEC")
                            {
                                output = string.Format("{0:x4}", int.Parse(value));
                                SaveToObjectFile(output);
                            }
                            else if (directive == "HEX")
                            {
                                output = string.Format("{0:0000}", int.Parse(value));
                                SaveToObjectFile(output);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid Assembly statement.");
                        }

                    }
                    else
                    {
                        Console.WriteLine("Invalid Assembly statement.");
                    }
                }
            }

            output = "E " + string.Format("{0:x4}", startingAddress);
            SaveToObjectFile(output);
        }

        private static int GetSymbolAddress(string operand)
        {
            var res = symTable.SingleOrDefault(a => a.symName == operand);

            int address = res == null? 0 : res.symAddress;

            return address;
        }

        private static int GetInstructionCode(string opCode)
        {
            int code = instructions.SingleOrDefault(a => a.opName == opCode).opCode;
            return code;
        }

        static void SaveToSymbleTable()
        {
            //string SYMTAB = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"E:\Learning\Diploma\System-Programming\projects\TwoPassAssembler\Assembler\TwoPassAssembler\Files\SYMTAB.txt");
            string SYMTAB = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\SYMTAB.txt");



            using (StreamWriter sw = File.AppendText(SYMTAB))
            {
                foreach (var s in symTable)
                {
                    sw.WriteLine(s.symName + " | " + String.Format("{0:x3}", s.symAddress) + " | " + (s.isRelative ? "r" : ""));
                }
            }

        }

        static void SaveToIntermediateFile(string inter)
        {
            //string intermediateCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"E:\Learning\Diploma\System-Programming\projects\TwoPassAssembler\Assembler\TwoPassAssembler\Files\intermediateCode.txt");
            string intermediateCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\intermediateCode.txt");

            
            using (StreamWriter sw = File.AppendText(intermediateCode))
            {
                sw.WriteLine(inter);
            }
        }

        static void SaveToObjectFile(string line)
        {
            //string objectCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"E:\Learning\Diploma\System-Programming\projects\TwoPassAssembler\Assembler\TwoPassAssembler\Files\objectCode.txt");
            string objectCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\objectCode.txt");


            using (StreamWriter sw = File.AppendText(objectCode))
            {
                sw.WriteLine(line);
            }
        }

        static void ResetFiles()
        {
            //string intermediateCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"E:\Learning\Diploma\System-Programming\projects\TwoPassAssembler\Assembler\TwoPassAssembler\Files\intermediateCode.txt");
            string intermediateCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\intermediateCode.txt");


            using (StreamWriter sw = new StreamWriter(intermediateCode))
            {
                sw.Write("");
            }

            //string SYMTAB = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"E:\Learning\Diploma\System-Programming\projects\TwoPassAssembler\Assembler\TwoPassAssembler\Files\SYMTAB.txt");
            string SYMTAB = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\SYMTAB.txt");

            using (StreamWriter sw = new StreamWriter(SYMTAB))
            {
                foreach (var s in symTable)
                {
                    sw.Write("");
                }
            }

            //string objectCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"E:\Learning\Diploma\System-Programming\projects\TwoPassAssembler\Assembler\TwoPassAssembler\Files\objectCode.txt");
            string objectCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\objectCode.txt");

            using (StreamWriter sw = new StreamWriter(objectCode))
            {
                sw.Write("");
            }
        }


    }

}
