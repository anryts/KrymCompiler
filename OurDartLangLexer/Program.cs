Dictionary<string, string> tokenTable = new Dictionary<string, string>
{
    { "true", "boolval" },
    { "false", "boolval" },
    { "main", "keyword" },
    { "int", "keyword" },
    { "double", "keyword" },
    { "bool", "keyword" },
    { "if", "keyword" },
    { "else", "keyword" },
    { "while", "keyword" },
    { "for", "keyword" },
    { "read", "keyword" },
    { "print", "keyword" },
    { "=", "assign_op" },
    { "+", "add_op" },
    { "-", "add_op" },
    { "*", "mult_op" },
    { "/", "mult_op" },
    { "^", "exp_op" },
    { "==", "rel_op" },
    { "!=", "rel_op" },
    { ">", "rel_op" },
    { ">=", "rel_op" },
    { "<", "rel_op" },
    { "<=", "rel_op" },
    { "(", "brackets_op" },
    { ")", "brackets_op" },
    { "{", "braces_op" },
    { "}", "braces_op" },
    { ";", "punct" },
    { "\n", "eol" },
    { "\r\n", "eol" },
    { "\t", "ws" },
    { " ", "ws" }
};

// Решта токенів визначаються за заключним станом
Dictionary<int, string> tokStateTable = new Dictionary<int, string>
{
    { 12, "id" },
    { 22, "intnum" },
    { 24, "realnum" },
    { 62, "comment" }
};

// Стан переходів
Dictionary<(int, string), int> stf = new Dictionary<(int, string), int>
{
    { (0, "Letter"), 11 }, { (11, "Letter"), 11 }, { (11, "Digit"), 11 }, { (11, "other"), 12 },
    { (0, "Digit"), 21 }, { (21, "Digit"), 21 }, { (21, "other"), 22 },
    { (21, "dot"), 23 }, { (23, "Digit"), 23 }, { (23, "other"), 24 },
    { (0, "+"), 20 }, { (0, "-"), 20 }, { (20, "Digit"), 21 },
    { (20, "other"), 25 },
    { (0, "="), 30 }, { (30, "="), 32 },
    { (30, "other"), 31 },
    { (0, ";"), 41 },
    { (0, "eol"), 51 },
    { (0, "/"), 60 }, { (60, "/"), 61 }, { (61, "Letter"), 61 }, { (61, "Digit"), 61 }, { (61, "eol"), 62 },
    { (60, "other"), 63 },
    { (0, "("), 71 }, { (0, ")"), 71 }, { (0, "{"), 71 }, { (0, "}"), 71 },
    { (0, "*"), 81 }, { (0, "^"), 81 },
    { (0, ">"), 90 }, { (0, "<"), 90 }, { (90, "="), 92 },
    { (0, "!"), 91 }, { (91, "="), 92 },
    { (90, "other"), 93 },
    { (0, "ws"), 0 },
    { (0, "other"), 100 }
};

// Стартовий стан
int initState = 0;
// Множина заключних станів
HashSet<int> F = new HashSet<int> { 12, 22, 24, 25, 31, 32, 41, 51, 62, 63, 71, 81, 92, 93, 100 };
// Заключні стани, що потребують обробки
HashSet<int> Fstar = new HashSet<int> { 12, 22, 24, 25, 31, 63, 93 };
// Стани помилок
HashSet<int> Ferror = new HashSet<int> { 100 };

// Таблиця ідентифікаторів
Dictionary<string, int> tableOfId = new Dictionary<string, int>();

// Таблиця констант
Dictionary<string, Tuple<string, int>> tableOfConst = new Dictionary<string, Tuple<string, int>>();

// Таблиця розбору
List<string> tableOfSymb = new List<string>();

// Поточний стан
int state = initState;

Console.OutputEncoding = System.Text.Encoding.Unicode;

string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
string fileName = "test.our_dart_lang";
string filePath = Path.Combine(projectDirectory, fileName);

if (!File.Exists(filePath)) throw new Exception("File not found: " + filePath);

string[] sourceCodeArr = File.ReadAllLines(filePath);
string sourceCode = "";

foreach (string sourceCodeLine in sourceCodeArr)
{
    sourceCode += sourceCodeLine;
    sourceCode += "\n";
}

// Ознака успішності/неуспішності розбору
Tuple<string, bool> FSuccess = new Tuple<string, bool>("Lexer", false);

int lenCode = sourceCode.Length; // Кількість символів у файлі з кодом програми
int numLine = 1; // Лексичний аналіз починаємо з першого рядка
int numChar = -1; // Нумерація з 0
char charSymbol = '\0'; // Ще не брали жодного символу
string lexeme = ""; // Ще не починали розпізнавати лексеми

void Lex()
{
    try
    {
        while (numChar < lenCode)
        {
            if(numChar + 1 >= lenCode) break;

            charSymbol = NextChar(); // Прочитати наступний символ
            string classCh = ClassOfChar(charSymbol); // До якого класу належить
            state = NextState(state, classCh); // Обчислити наступний стан

            if (IsFinal(state)) // Якщо стан заключний
            {
                Processing(); // Виконати семантичні процедури
            }
            else if (state == initState)
            {
                lexeme = ""; // Якщо стан НЕ заключний, а стартовий - нова лексема
            }
            else
            {
                lexeme += charSymbol; // Якщо стан НЕ заключний і не стартовий - додати символ до лексеми
            }
        }

        Console.WriteLine("Lexer: Лексичний аналіз завершено успішно");
        FSuccess = new Tuple<string, bool>("Lexer", true);
    }
    catch (Exception e)
    {
        Console.WriteLine($"Lexer: Аварійне завершення програми з кодом {e.Message}");
    }
}

void Processing()
{
    if (state == 12 || state == 22 || state == 24) // keyword, id, bool, float, int
    {
        string token = GetToken(state, lexeme);
        if (token == "keyword") // Keyword
        {
            Console.WriteLine($"{numLine,-3} {lexeme,-10} {token,-10}");
            tableOfSymb.Add($"{numLine,-3} {lexeme,-10} {token,-10}");
        }
        else if(token == "boolval") // Boolean
        {

        }
        else // Number
        {
            int index = IndexIdConst(state, lexeme);
            Console.WriteLine($"{numLine,-3} {lexeme,-10} {token,-10} {index,-5}");
            tableOfSymb.Add($"{numLine,-3} {lexeme,-10} {token,-10} {index,-5}");
        }

        lexeme = "";
        numChar = PutCharBack(numChar); // Зірочка
        state = initState;
    }

    if ( state == 25 || state == 31 || state == 63 || state == 93) // +,- or = or / or <,>
    {
        string token = GetToken(state, lexeme);
        Console.WriteLine($"{numLine,-3} {lexeme,-10} {token,-10}");
        tableOfSymb.Add($"{numLine,-3} {lexeme,-10} {token,-10}");
        lexeme = "";
        numChar = PutCharBack(numChar); // Зірочка
        state = initState;
    }

    if (state == 32 || state == 41 || state == 71 || state == 81 || state == 92) // == or ; or (){} or *^ or !=, <=, >=
    {
        lexeme += charSymbol;
        string token = GetToken(state, lexeme);
        Console.WriteLine($"{numLine,-3} {lexeme,-10} {token,-10}");
        tableOfSymb.Add($"{numLine,-3} {lexeme,-10} {token,-10}");
        lexeme = "";
        state = initState;
    }

    if (state == 51) // End of Line
    {
        numLine++;
        state = initState;
    }

    if (state == 62) // Comment
    {
        state = initState;
    }

    if (Ferror.Contains(state)) // ERROR
    {
        Fail();
    }
}

void Fail()
{
    Console.WriteLine(numLine);
    if (state == 100)
    {
        Console.WriteLine($"Lexer: у рядку {numLine} неочікуваний символ {charSymbol}");
        Environment.Exit(100);
    }
    /*else if (state == 102)
    {
        Console.WriteLine($"Lexer: у рядку {numLine} очікувався символ =, а не {charSymbol}");
        Environment.Exit(102);
    }*/
}

bool IsFinal(int state)
{
    return F.Contains(state);
}

int NextState(int state, string classCh)
{
    try
    {
        return stf[(state, classCh)];
    }
    catch (KeyNotFoundException)
    {
        return stf[(state, "other")];
    }
}

char NextChar()
{
    numChar++;
    return sourceCode[numChar];
}

int PutCharBack(int numChar)
{
    return numChar - 1;
}

string ClassOfChar(char ch)
{
    if (ch == '.') return "dot";
    if (char.IsLower(ch)) return "Letter";
    if (char.IsDigit(ch)) return "Digit";
    if (ch == ' ' || ch == '\t') return "ws";
    if (ch == '\n' || ch == '\r') return "eol";
    if ("=+-*/^!(){};".Contains(ch)) return ch.ToString();
    return "символ не належить алфавіту";
}

string GetToken(int state, string lexeme)
{
    return tokenTable.ContainsKey(lexeme) ? tokenTable[lexeme] : tokStateTable[state];
}

int IndexIdConst(int state, string lexeme)
{
    int indx = 0;
    if (state == 12)
    {
        if (!tableOfId.ContainsKey(lexeme))
        {
            indx = tableOfId.Count + 1;
            tableOfId[lexeme] = indx;
        }
        else
        {
            indx = tableOfId[lexeme];
        }
    }
    else if (state == 22 || state == 24)
    {
        if (!tableOfConst.ContainsKey(lexeme))
        {
            indx = tableOfConst.Count + 1;
            tableOfConst[lexeme] = new Tuple<string, int>(tokStateTable[state], indx);
        }
        else
        {
            indx = tableOfConst[lexeme].Item2;
        }
    }
    else if (state == 62)
    {

    }

    return indx;
}

// Запуск лексичного аналізатора
Lex();

// Виведення таблиць
Console.WriteLine(new string('-', 30));
Console.WriteLine("tableOfSymb:");
foreach (var item in tableOfSymb)
{
    Console.WriteLine(item);
}
Console.WriteLine("tableOfId:");
foreach (var item in tableOfId)
{
    Console.WriteLine(item.Key + ": " + item.Value);
}
Console.WriteLine("tableOfConst:");
foreach (var item in tableOfConst)
{
    Console.WriteLine(item.Key + ": " + item.Value);
}
