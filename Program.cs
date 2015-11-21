using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;



namespace Metrika1
{
    class Program
    {
        public const string emptyString = "";
        public const char doubleQuotes = '"';
        public const char backSlash = '\\';
        public const char openCurvedBracket = '{';
        public const char closeCurvedBracket = '}';
        public static string[] arrayOfTypes = { "int","char","wchar_t","bool","float","double","string" };
        public const int numberOfStandartTypes = 7;


        public const String multiLineCommentRegEx = @"\/\*[\s\S]*?\*\/";
        public const String singleLineCommentRegEx = @"\/\/[^\n\r]*";
        public const String compilierInstructions = @"#.*";
        
       // public static String charRegEx = @"\'.\'"; 
 
        public const String identifierRegEx = @"\b([a-zA-Z_][a-zA-Z0-9_]*)\b";
        public const String pointerRegEx = @"\s*[\*|\&]*\s*";
        public const String functionParamsRegEx = @"\(.*\)";
        public const String signatureFunctionRegEx = identifierRegEx + pointerRegEx + identifierRegEx +  functionParamsRegEx + @"\s*({)";

        public struct variableOfGlobalNames
        {
            public string nameOfVariable;
            public string typeOfVariable;
        }
        public struct structForGlobalArrayAndCount
        {
            public variableOfGlobalNames[] arrayOfGlobalVars;
            public int countOfGlobalVars;
        }

        public const int MAX_COUNT_GLOBAL_VARS = 30;    
        static void Main(string[] args)
        {  
            string codeString;
            structForGlobalArrayAndCount structArrayOfGlobalVars = new structForGlobalArrayAndCount();

            variableOfGlobalNames[] arrayOfGlobalVars = new variableOfGlobalNames[MAX_COUNT_GLOBAL_VARS];
           


            codeString = readCodeFromFile();
            codeString = deleteStringLiterals(codeString);
            codeString = deleteComments(codeString, multiLineCommentRegEx);
            codeString = deleteComments(codeString, singleLineCommentRegEx);
            codeString = deleteCompilerInstructions(codeString, compilierInstructions);
            structArrayOfGlobalVars = getCountAndArrayOfGlobalVars(codeString, arrayOfGlobalVars, signatureFunctionRegEx);
            Console.WriteLine(getCountOfFuncFieldView(codeString, structArrayOfGlobalVars));
            Console.WriteLine(getCountAccessFuncToGlobalVars(codeString, structArrayOfGlobalVars));
            Console.WriteLine((float)getCountAccessFuncToGlobalVars(codeString, structArrayOfGlobalVars) / (float)getCountOfFuncFieldView(codeString, structArrayOfGlobalVars));    
            Console.ReadLine();
        }

        public static String deleteStringLiterals(String codeString)  //delete all string literals
        {
            StringBuilder additionalStringForCode = new StringBuilder(emptyString, codeString.Length);

            int startDoubleQuotes = 0, endDoubleQuotes = 0;
            int counterForAddStr = 0;
            int i = 0;
            Boolean checkDeletingLiterals = true;

            for (i = 0; i < codeString.Length; i++ )
            {
                if (codeString[i] == doubleQuotes)
                {
                    if (codeString[i - 1] != backSlash)
                    {
                        if (startDoubleQuotes == 0)
                            startDoubleQuotes = i;
                        else
                        {
                            endDoubleQuotes = i;
                            additionalStringForCode.Append(codeString.Substring(i - counterForAddStr, counterForAddStr - (endDoubleQuotes - startDoubleQuotes)));
                            checkDeletingLiterals = false;
                            counterForAddStr = -1;
                            startDoubleQuotes = endDoubleQuotes = 0;
                        }
                    }
                }
                counterForAddStr++;
            }
            if (checkDeletingLiterals == false)
            {
                additionalStringForCode.Append(codeString.Substring(i - counterForAddStr, counterForAddStr));
                codeString = additionalStringForCode.ToString();              
            }             
            return codeString;
        }
        public static String deleteComments(String codeString, String pattern) //delete all comments 
        {

            Regex regularExpression = new Regex(pattern);
            string replacement = emptyString;
            codeString = regularExpression.Replace(codeString, replacement);

            return codeString;
        }
        public static String deleteCompilerInstructions(String codeString, String pattern)
        {
            Regex regularExpression = new Regex(pattern);
            string replacement = emptyString;
            codeString = regularExpression.Replace(codeString, replacement);

            return codeString;
        }
        public static string readCodeFromFile()    // read code from file and write to string
        {
            string stringWithCode;
            string addressOfCode = "D:\\Studying\\3d term\\C#\\Metrika1\\c++ code\\DenchikCode\\code.cpp";

            StreamReader reader = new StreamReader(addressOfCode);     
            stringWithCode = reader.ReadToEnd();
            return stringWithCode;
        }

        public static bool isStandartType( string stringOfType)
        {
            bool isType = false;
            int counterForCycle = 0;

            while ((counterForCycle < numberOfStandartTypes) && (!(isType)))
            {
                if ((string.Compare(arrayOfTypes[counterForCycle], stringOfType, true)) == 0)
                {
                    isType = true;
                }
                counterForCycle++;
            }
            return isType;
        }

        public static structForGlobalArrayAndCount getCountAndArrayOfGlobalVars(String codeString, variableOfGlobalNames[] arrayOfGlobalVars, String pattern) // get number of global params
        {
            int countOfGlobalVars = 0;
            int additionalCounter = 0;
            int countOfCurvedBrackets = 0;
            int index;
            String signatureFunction;
            structForGlobalArrayAndCount structForReturning = new structForGlobalArrayAndCount();
            StringBuilder additionalCodeString = new StringBuilder(emptyString,codeString.Length);
            Regex regularExpression = new Regex(pattern);
            Match match = regularExpression.Match(codeString);
            
            while (match.Success)               // delete all functions in our code
            {              
                signatureFunction = match.Groups[0].Value;
                index = codeString.IndexOf(signatureFunction);
                additionalCodeString.Append(codeString.Substring(additionalCounter, index - additionalCounter));

                while (codeString[index] != openCurvedBracket)
                    index++;

                while (countOfCurvedBrackets >= 0)
                {
                    if (codeString[index] == openCurvedBracket)
                        countOfCurvedBrackets++;
                    else if (codeString[index] == closeCurvedBracket)
                        countOfCurvedBrackets--;
                    if (countOfCurvedBrackets == 0)
                        countOfCurvedBrackets = -1;
                    index++;
                }
                    additionalCounter = index;
                    countOfCurvedBrackets = 0;
                match = match.NextMatch();
            }

            additionalCodeString.Append(codeString.Substring(additionalCounter, codeString.Length - additionalCounter));
            codeString = additionalCodeString.ToString();

            // create new RegEx fo finding all variables in codeString(all code after deliting comments, strings, functions)
            
            pattern = identifierRegEx + pointerRegEx + identifierRegEx;
            regularExpression = new Regex(pattern);
            match = regularExpression.Match(codeString);

            index = 0;
            while (match.Success)
            {
                if (isStandartType(match.Groups[1].Value))
                {
                    arrayOfGlobalVars[index].typeOfVariable = match.Groups[1].Value;
                    arrayOfGlobalVars[index].nameOfVariable = match.Groups[2].Value;
                    index++;
                    countOfGlobalVars++;
                }
                match = match.NextMatch();
            }

            structForReturning.countOfGlobalVars = countOfGlobalVars;
            structForReturning.arrayOfGlobalVars = arrayOfGlobalVars; 

            return structForReturning;
        }

        public static int getCountOfFuncFieldView(String codeString, structForGlobalArrayAndCount globalArray )
        {
            StringBuilder typeAndNameVar = new StringBuilder("",30);
            string additionalString;
            StringBuilder additionalStringCode = new StringBuilder("", codeString.Length);
            int indexOfVar;
            int countOfFuncFieldView = 0;

            Regex regularExpression = new Regex(signatureFunctionRegEx);
            Match match  ;
            for (int i = 0; i < globalArray.countOfGlobalVars; i++)
            {
                typeAndNameVar.Append(globalArray.arrayOfGlobalVars[i].typeOfVariable);
                typeAndNameVar.Append(" ");
                typeAndNameVar.Append(globalArray.arrayOfGlobalVars[i].nameOfVariable);
                additionalString = typeAndNameVar.ToString();
                indexOfVar = codeString.IndexOf(additionalString);
                additionalStringCode.Append(codeString.Substring(indexOfVar, codeString.Length - indexOfVar));
                additionalString = additionalStringCode.ToString();
                match = regularExpression.Match(additionalString);
                while (match.Success)
                {
                    countOfFuncFieldView++;
                    match = match.NextMatch();
                }
                typeAndNameVar.Replace(typeAndNameVar.ToString(), "");
                additionalStringCode.Replace(additionalStringCode.ToString(), "");
            }
            return countOfFuncFieldView;
        }

        public static int getCountAccessFuncToGlobalVars(string codeString, structForGlobalArrayAndCount globalArray)
        {
            int additionalCounter = 0;
            int countOfCurvedBrackets = 0;
            int index;
            int indexOfGlobalVariable = 0;
            int CountAccessFuncToGlobalVars = 0;
            string additionalString;
            bool isGlobal = true;
            String signatureFunction;
            StringBuilder additionalCodeString = new StringBuilder(emptyString, codeString.Length);
            Regex regularExpressionSignature = new Regex(signatureFunctionRegEx);
            Match matchSignature = regularExpressionSignature.Match(codeString);

            while (matchSignature.Success)               // find function
            {
                signatureFunction = matchSignature.Groups[0].Value;
                index  = codeString.IndexOf(signatureFunction);

                while (codeString[index] != openCurvedBracket)
                    index++;
                additionalCounter = index;
                while (countOfCurvedBrackets >= 0)
                {
                    if (codeString[index] == openCurvedBracket)
                        countOfCurvedBrackets++;
                    else if (codeString[index] == closeCurvedBracket)
                        countOfCurvedBrackets--;
                    if (countOfCurvedBrackets == 0)
                        countOfCurvedBrackets = -1;
                    index++;
                }
                additionalCodeString.Append(codeString.Substring(additionalCounter, index - additionalCounter));
                additionalString = additionalCodeString.ToString();
                for (int i = 0; i < globalArray.countOfGlobalVars; i++ )
                {
                    if (additionalString.IndexOf(globalArray.arrayOfGlobalVars[i].nameOfVariable) != -1) 
                    {
                        int indexOfEndType = 0; 
                        indexOfGlobalVariable = additionalString.IndexOf(globalArray.arrayOfGlobalVars[i].nameOfVariable);
                        indexOfGlobalVariable--;
                        while ((additionalString[indexOfGlobalVariable] == '\n') ||
                               (additionalString[indexOfGlobalVariable] == '\t') ||
                               (additionalString[indexOfGlobalVariable] == '\r') ||
                               (additionalString[indexOfGlobalVariable] == ' ') && (indexOfGlobalVariable != 0)) 
                        {
                            indexOfGlobalVariable--;
                        }
                        indexOfEndType = indexOfGlobalVariable;
                        while ((additionalString[indexOfGlobalVariable] != '\n') &&
                               (additionalString[indexOfGlobalVariable] != '\t') &&
                               (additionalString[indexOfGlobalVariable] != '\r') && 
                               (additionalString[indexOfGlobalVariable] != ' ') && (indexOfGlobalVariable != 0 ))
                        {
                            indexOfGlobalVariable--;
                        }
                        if (isStandartType(additionalString.Substring(indexOfGlobalVariable + 1, indexOfEndType - indexOfGlobalVariable )) && (indexOfGlobalVariable != 0))
                            isGlobal = false;
                        if ((isGlobal) || (indexOfGlobalVariable ==0))
                            CountAccessFuncToGlobalVars++;
                        isGlobal = true;
                    
                    }
                    
                }
                    countOfCurvedBrackets = 0;
                    matchSignature = matchSignature.NextMatch();
                additionalCodeString.Replace(additionalCodeString.ToString(), "");
            }
            
            codeString = additionalCodeString.ToString();
            return CountAccessFuncToGlobalVars;
        }

        
    

    
    

    
    }
}

