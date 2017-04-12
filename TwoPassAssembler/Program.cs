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
        static List<Symbols> symTable1 = new List<Symbols>();
        static List<Symbols> symTable2 = new List<Symbols>();
        static List<string> totalObjectCodeList = new List<string>();
        static void Main(string[] args)
        {
            ResetFiles();

            CreateInstructions();

            CreateDirectives();

            LoadFirstAssembly();

            LoadSecondAssembly();

            Linker();

            Loader("100");

            Console.ReadLine();
        }

        static void Linker()
        {
            string objectFile1 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\objectCode.txt");
            string objectFile2 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\objectCode2.txt");
            string addingObjectCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\addingObjectCode.txt");

            string line = "";

            int totalSize = 0;

            int firstProgSize = 0;

            string startingAddress = "";

            using (var reader = new StreamReader(objectFile1))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    string[] firstLine = line.Split(' ');

                    if (firstLine.Length > 1)
                    {
                        startingAddress = firstLine[1];

                        string size = firstLine[2];

                        totalSize = Convert.ToInt32(size, 16);

                        firstProgSize = totalSize;
                    }
                    else
                    {
                        totalObjectCodeList.Add(firstLine[0]);
                    }
                }
            }

            using (var reader = new StreamReader(objectFile2))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    string[] firstLine = line.Split(' ');

                    if (firstLine.Length > 1)
                    {
                        string size = firstLine[2];

                        totalSize += Convert.ToInt32(size, 16);
                    }
                    else
                    {
                        string[] lines = firstLine[0].Split('r');

                        if (lines.Length == 1)
                        {
                            totalObjectCodeList.Add(firstLine[0]);
                        }
                        else
                        {
                            int address = Convert.ToInt32(lines[0], 16) + firstProgSize;

                            string addressHexa = string.Format("{0:x4}", address);

                            totalObjectCodeList.Add(addressHexa + "r");
                        }

                    }
                }
            }

            using (StreamWriter sw = File.AppendText(addingObjectCode))
            {
                string header = "H " + startingAddress + " " + string.Format("{0:x4}", totalSize);

                sw.WriteLine(header);

                foreach (var s in totalObjectCodeList)
                {
                    sw.WriteLine(s);
                }
            }
            
        }

        private static void Loader(string memoryAddress)
        {
            string addingObjectCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\addingObjectCode.txt");
            string Final = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\Final.txt");

            totalObjectCodeList = new List<string>();

            string line = "";

            string size = "";

            int memoryAdd = Convert.ToInt32(memoryAddress, 16);

            using (var reader = new StreamReader(addingObjectCode))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    string[] firstLine = line.Split(' ');

                    if (firstLine.Length > 1)
                    {
                        size = firstLine[2];
                    }
                    else
                    {
                        string[] lines = firstLine[0].Split('r');

                        if (lines.Length == 1)
                        {
                            totalObjectCodeList.Add(firstLine[0]);
                        }
                        else
                        {
                            int address = Convert.ToInt32(lines[0], 16) + memoryAdd;

                            string addressHexa = string.Format("{0:x4}", address);

                            totalObjectCodeList.Add(addressHexa);
                        }

                    }
                }
            }

            using (StreamWriter sw = File.AppendText(Final))
            {
                string header = "H " + string.Format("{0:x4}", memoryAdd) + " " + size;

                sw.WriteLine(header);

                foreach (var s in totalObjectCodeList)
                {
                    sw.WriteLine(s);
                }
            }


        }

        static void LoadFirstAssembly()
        {
            string assemblyCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\assemblyCode.txt");
            string intermediateCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\intermediateCode.txt");
            string objectCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\objectCode.txt");
            string SYMTAB = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\SYMTAB.txt");


            Pass_1(assemblyCode, symTable1, intermediateCode, SYMTAB);

            Pass_2(assemblyCode, symTable1, objectCode);
        }

        static void LoadSecondAssembly()
        {
            string assemblyCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\assemblyCode2.txt");
            string intermediateCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\intermediateCode2.txt");
            string objectCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\objectCode2.txt");
            string SYMTAB = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\SYMTAB2.txt");


            Pass_1(assemblyCode, symTable2, intermediateCode, SYMTAB);

            Pass_2(assemblyCode, symTable2, objectCode);
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

        static void Pass_1(string assemblyCode, List<Symbols> symTable, string intermediateFilePath, string symFilePath)
        {
            using (var reader = new StreamReader(assemblyCode))
            {
                string line;
                string interLine = "";
                locationCounter = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    string[] lineWords = line.Split(' ');
                    string word_1, word_2, word_3;
                    string label, opCode, directive, value, operand;

                    int count = lineWords.Length;

                    if (count == 1)
                    {
                        opCode = lineWords[0];

                        bool isInstruction = instructions.Any(a => a.opName.ToLower().Equals(opCode.ToLower()));

                        if (isInstruction)
                        {
                            //Write something to intermediate file 
                            interLine = string.Format("{0:x3}", locationCounter) + " " + line;
                            SaveToIntermediateFile(interLine, intermediateFilePath);
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

                        bool isInstruction = instructions.Any(a => a.opName.ToLower().Equals(word_1.ToLower()));
                        bool isDirective = directives.Any(a => a.ToLower().Equals(word_1.ToLower()));

                        if (isInstruction)
                        {
                            opCode = word_1;
                            operand = word_2;
                            //Write something to intermediate file interLine =
                            interLine = string.Format("{0:x3}", locationCounter) + " " + line;
                            SaveToIntermediateFile(interLine, intermediateFilePath);
                            locationCounter++;
                        }
                        else if (isDirective)
                        {
                            directive = word_1;

                            if (directive.ToLower() == "EXTERN".ToLower())
                            {
                                operand = word_2;

                                bool isLabelExist = symTable.Any(a => a.symName.ToLower().Equals(operand.ToLower()));

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
                                    SaveToIntermediateFile(interLine, intermediateFilePath);
                                    locationCounter++;
                                }
                            }
                            else if (directive.ToLower() == "ORG".ToLower())
                            {
                                value = word_2;
                                startingAddress = Convert.ToInt32(value, 16);                  
                                //Write something to intermediate file interLine =
                                interLine = string.Format("{0:x3}", locationCounter) + " " + line;
                                SaveToIntermediateFile(interLine, intermediateFilePath);
                                locationCounter = startingAddress;
                            }
                            else if (directive.ToLower() == "TITLE".ToLower())
                            {
                                value = word_2;

                                programName = value;

                                //Write something to intermediate file interLine =
                                interLine = string.Format("{0:x3}", locationCounter) + " " + line;
                                SaveToIntermediateFile(interLine, intermediateFilePath);

                                locationCounter++;
                            }
                            else if (directive.ToLower() == "DEFINE".ToLower())
                            {
                                string[] splitWord_2 = word_2.Split('=');
                                if (splitWord_2.Length == 2)
                                {
                                    operand = splitWord_2[0];
                                    value = splitWord_2[1];
                                }
                                //Write something to intermediate file interLine =
                                interLine = string.Format("{0:x3}", locationCounter) + " " + line;
                                SaveToIntermediateFile(interLine, intermediateFilePath);
                                locationCounter++;
                            }
                            else if (directive.ToLower() == "PUBLIC".ToLower())
                            {
                                //Write something to intermediate file interLine =
                                interLine = string.Format("{0:x3}", locationCounter) + " " + line;
                                SaveToIntermediateFile(interLine, intermediateFilePath);
                                locationCounter++;
                            }

                        }
                        else //label
                        {
                            label = word_1;

                            bool isLabelExist = symTable.Any(a => a.symName.ToLower().Equals(label.ToLower()));

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
                            SaveToIntermediateFile(interLine, intermediateFilePath);
                            locationCounter++;
                            
                        }
                    }
                    else if (count == 3)
                    {
                        label = lineWords[0];

                        bool isLabelExist = symTable.Any(a => a.symName.ToLower().Equals(label.ToLower()));

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
                            SaveToIntermediateFile(interLine, intermediateFilePath);
                        }

                        word_2 = lineWords[1];
                        word_3 = lineWords[2];

                        bool isInstruction = instructions.Any(a => a.opName.ToLower().Equals(word_2.ToLower()));
                        bool isDirective = directives.Any(a => a.ToLower().Equals(word_2.ToLower()));

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
                            if (directive.ToLower() == "DUP".ToLower())
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

                SaveToSymbleTable(symTable, symFilePath);
            }
        }

        static void Pass_2(string assemblyCode, List<Symbols> symTable, string objectFilePath)
        {
            string output = "";

            output = "H " + (string.IsNullOrEmpty(programName) ? "" : programName + " ") + string.Format("{0:x4}", startingAddress) + " " + string.Format("{0:x4}", programSize);

            SaveToObjectFile(output, objectFilePath);

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

                        bool isInstruction = instructions.Any(a => a.opName.ToLower().Equals(opCode.ToLower()));

                        if (isInstruction)
                        {
                            int code = GetInstructionCode(opCode);
                            output = string.Format("{0:x}", code) + string.Format("{0:x3}", 0);
                            string binary = Convert.ToString(Convert.ToInt32(output, 16), 2).PadLeft(16, '0');
                            //SaveToObjectFile(binary + " | " + output, objectFilePath);
                            SaveToObjectFile(output, objectFilePath);
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

                        bool isInstruction = instructions.Any(a => a.opName.ToLower().Equals(word_1.ToLower()));
                        bool isDirective = directives.Any(a => a.ToLower().Equals(word_1.ToLower()));

                        if (isInstruction)
                        {
                            opCode = word_1;
                            operand = word_2;

                            int code = GetInstructionCode(opCode);
                            int address = GetSymbolAddress(operand, symTable);

                            output = string.Format("{0:x}", code) + string.Format("{0:x3}", address);
                            string binary = Convert.ToString(Convert.ToInt32(output, 16), 2).PadLeft(16, '0');
                            //SaveToObjectFile(binary + " | " + output, objectFilePath);
                            SaveToObjectFile(output + "r", objectFilePath);
                        }
                        else if (isDirective)
                        {
                            directive = word_1;

                            if (directive.ToLower() == "EXTERN".ToLower())
                            {
                                operand = word_2;

                                int address = GetSymbolAddress(operand, symTable);
                                output = string.Format("{0:x4}", address);
                                string binary = Convert.ToString(Convert.ToInt32(output, 16), 2).PadLeft(16, '0');
                                //SaveToObjectFile(binary + " | " + output, objectFilePath);
                                SaveToObjectFile(output + "r", objectFilePath);
                            }
                            else if (directive.ToLower() == "ORG".ToLower())
                            {
                                
                            }
                            else if (directive.ToLower() == "TITLE".ToLower())
                            {
                                
                            }
                            else if (directive.ToLower() == "DEFINE".ToLower())
                            {
                                
                            }
                            else if (directive.ToLower() == "PUBLIC".ToLower())
                            {
                                operand = word_2;

                                int address = GetSymbolAddress(operand, symTable);
                                output = string.Format("{0:x4}", address);
                                string binary = Convert.ToString(Convert.ToInt32(output, 16), 2).PadLeft(16, '0');
                                //SaveToObjectFile(binary + " | " + output, objectFilePath);
                                SaveToObjectFile(output + "r", objectFilePath);
                            }
                        }
                        else //label
                        {
                            label = word_1;

                            value = word_2;

                            output = string.Format("{0:x4}", int.Parse(value));
                            string binary = Convert.ToString(Convert.ToInt32(output, 16), 2).PadLeft(16, '0');
                            //SaveToObjectFile(binary + " | " + output, objectFilePath);
                            SaveToObjectFile(output, objectFilePath);

                        }
                    }
                    else if (count == 3)
                    {
                        label = lineWords[0];

                        word_2 = lineWords[1];
                        word_3 = lineWords[2];

                        bool isInstruction = instructions.Any(a => a.opName.ToLower().Equals(word_2.ToLower()));
                        bool isDirective = directives.Any(a => a.ToLower().Equals(word_2.ToLower()));

                        if (isInstruction)
                        {
                            opCode = word_2;
                            operand = word_3;

                            int code = GetInstructionCode(opCode);
                            int address = GetSymbolAddress(operand, symTable);

                            output = string.Format("{0:x}", code) + string.Format("{0:x3}", address);
                            string binary = Convert.ToString(Convert.ToInt32(output, 16), 2).PadLeft(16, '0');
                            //SaveToObjectFile(binary + " | " + output, objectFilePath);
                            SaveToObjectFile(output + "r", objectFilePath);
                        }
                        else if (isDirective)
                        {
                            directive = word_2;
                            value = word_3;
                            if (directive.ToLower() == "DEC".ToLower())
                            {
                                output = string.Format("{0:x4}", int.Parse(value));
                                string binary = Convert.ToString(Convert.ToInt32(output, 16), 2).PadLeft(16, '0');
                                //SaveToObjectFile(binary + " | " + output, objectFilePath);
                                SaveToObjectFile(output, objectFilePath);
                            }
                            else if (directive.ToLower() == "HEX".ToLower())
                            {
                                output = string.Format("{0:0000}", int.Parse(value));
                                string binary = Convert.ToString(Convert.ToInt32(output, 16), 2).PadLeft(16, '0');
                                //SaveToObjectFile(binary + " | " + output, objectFilePath);
                                SaveToObjectFile(output, objectFilePath);
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

            //output = "E " + string.Format("{0:x4}", startingAddress);
           // SaveToObjectFile(output);
        }

        private static int GetSymbolAddress(string operand, List<Symbols> symTable)
        {
            var res = symTable.SingleOrDefault(a => a.symName.ToLower() == operand.ToLower());

            int address = res == null? 0 : res.symAddress;

            return address;
        }

        private static int GetInstructionCode(string opCode)
        {
            int code = instructions.SingleOrDefault(a => a.opName.ToLower() == opCode.ToLower()).opCode;
            return code;
        }

        static void SaveToSymbleTable(List<Symbols> symTable, string SYMTAB)
        {
            using (StreamWriter sw = File.AppendText(SYMTAB))
            {
                foreach (var s in symTable)
                {
                    sw.WriteLine(s.symName + " | " + String.Format("{0:x3}", s.symAddress) + " | " + (s.isRelative ? "r" : ""));
                }
            }

        }

        static void SaveToIntermediateFile(string inter, string intermediateCode)
        {
            //string intermediateCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"E:\Learning\Diploma\System-Programming\projects\TwoPassAssembler\Assembler\TwoPassAssembler\Files\intermediateCode.txt");
            
            using (StreamWriter sw = File.AppendText(intermediateCode))
            {
                sw.WriteLine(inter);
            }
        }

        static void SaveToObjectFile(string line, string objectCode)
        {
            using (StreamWriter sw = File.AppendText(objectCode))
            {
                sw.WriteLine(line);
            }
        }

        static void ResetFiles()
        {
           string intermediateCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\intermediateCode.txt");

            using (StreamWriter sw = new StreamWriter(intermediateCode))
            {
                sw.Write("");
            }

            intermediateCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\intermediateCode2.txt");

            using (StreamWriter sw = new StreamWriter(intermediateCode))
            {
                sw.Write("");
            }

            string SYMTAB = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\SYMTAB.txt");

            using (StreamWriter sw = new StreamWriter(SYMTAB))
            {
                sw.Write("");
            }

            SYMTAB = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\SYMTAB2.txt");

            using (StreamWriter sw = new StreamWriter(SYMTAB))
            {
                sw.Write("");
            }

            string objectCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\objectCode.txt");

            using (StreamWriter sw = new StreamWriter(objectCode))
            {
                sw.Write("");
            }

            objectCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\objectCode2.txt");

            using (StreamWriter sw = new StreamWriter(objectCode))
            {
                sw.Write("");
            }

            string addingObjectCode = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\addingObjectCode.txt");

            using (StreamWriter sw = new StreamWriter(addingObjectCode))
            {
                sw.Write("");
            }

            string final = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Files\Final.txt");

            using (StreamWriter sw = new StreamWriter(final))
            {
                sw.Write("");
            }
        }


    }

}
