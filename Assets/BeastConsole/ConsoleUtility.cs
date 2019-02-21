using System.Text;
using BeastConsole;
using UnityEngine;

public static class ConsoleUtility
{
    private static StringBuilder sb = new StringBuilder();
    private const string COLUMN_SEPARATOR = " | ";
    private const char SPACE = ' ';


    public static string DeNewLine(string message)
    {
        return message.Replace("\n", " | ");
    }

    public static void Print(string line)
    {
        Console.WriteLine(line);
    }

    public static void Print(object obj)
    {
        Console.WriteLine(obj.ToString());
    }

    public static void RegisterCommand(string name, string description, object owner, System.Action<string[]> callback)
    {
        Console.AddCommand(name, description, owner, callback);
    }

    public static void PrintTableRow(string A, string B, int columnSize = 20)
    {
        sb.Clear();
        appendColumn(A, columnSize);
        appendColumn(B, columnSize);
        sb.Append(COLUMN_SEPARATOR);

        Console.WriteLine(sb.ToString());
    }

    public static void PrintTableRowIndividualSize(string A, string B, int columnSizeA, int columnSizeB)
    {
        sb.Clear();
        appendColumn(A, columnSizeA);
        appendColumn(B, columnSizeB);
        sb.Append(COLUMN_SEPARATOR);

        Console.WriteLine(sb.ToString());
    }

    public static void PrintTableRow(string A, string B, string C, int columnSize = 20)
    {
        sb.Clear();
        appendColumn(A, columnSize);
        appendColumn(B, columnSize);
        appendColumn(C, columnSize);
        sb.Append(COLUMN_SEPARATOR);

        Console.WriteLine(sb.ToString());
    }

    public static void PrintTableRowIndividualSize(string A, string B, string C, int columnSizeA, int columnSizeB, int columnSizeC)
    {
        sb.Clear();
        appendColumn(A, columnSizeA);
        appendColumn(B, columnSizeB);
        appendColumn(C, columnSizeC);
        sb.Append(COLUMN_SEPARATOR);

        Console.WriteLine(sb.ToString());
    }

    public static void PrintTableTitles(string ATitle, string BTitle, int columnSize = 20)
    {
        sb.Clear();
        sb.Append(SPACE, COLUMN_SEPARATOR.Length);
        appendColumnTitle(ATitle, columnSize);
        appendColumnTitle(BTitle, columnSize);

        Console.WriteLine(sb.ToString());
    }

    public static void PrintTableTitlesIndividualSize(string ATitle, string BTitle, int columnSizeA, int columnSizeB)
    {
        sb.Clear();
        sb.Append(SPACE, COLUMN_SEPARATOR.Length);
        appendColumnTitle(ATitle, columnSizeA);
        appendColumnTitle(BTitle, columnSizeB);

        Console.WriteLine(sb.ToString());
    }

    public static void PrintTableTitles(string ATitle, string BTitle, string CTitle, int columnSize = 20)
    {
        sb.Clear();
        sb.Append(SPACE, COLUMN_SEPARATOR.Length);
        appendColumnTitle(ATitle, columnSize);
        appendColumnTitle(BTitle, columnSize);
        appendColumnTitle(CTitle, columnSize);

        Console.WriteLine(sb.ToString());
    }

    public static void PrintTableTitlesIndividualSize(string ATitle, string BTitle, string CTitle, int columnSizeA, int columnSizeB, int columnSizeC)
    {
        sb.Clear();
        sb.Append(SPACE, COLUMN_SEPARATOR.Length);
        appendColumnTitle(ATitle, columnSizeA);
        appendColumnTitle(BTitle, columnSizeB);
        appendColumnTitle(CTitle, columnSizeC);

        Console.WriteLine(sb.ToString());
    }

    private static void appendColumn(string str, int columnSize)
    {
        int al = str.Length;
        int length = al < columnSize ? al : columnSize;

        sb.Append(COLUMN_SEPARATOR);
        sb.Append(str, 0, length);
        sb.Append(SPACE, columnSize - length);
    }

    private static void appendColumnTitle(string str, int columnSize)
    {
        int al = str.Length;
        int length = al < columnSize ? al : columnSize;
        int side = (columnSize - length) / 2;

        sb.Append(SPACE, side);
        sb.Append(str, 0, length);
        sb.Append(SPACE, side);
        sb.Append(SPACE, COLUMN_SEPARATOR.Length);

    }


    public static string ToHex(Color color)
    {
        sb.Clear();
        Color32 col = color;

        sb.Append("<color=");
        sb.Append('#');
        sb.Append(col.r.ToString("X2"));
        sb.Append(col.g.ToString("X2"));
        sb.Append(col.b.ToString("X2"));
        sb.Append(">");

        return sb.ToString();
    }

    public static int WrapInColor(string color, string value, out string result)
    {
        sb.Clear();
        sb.Append(color);
        sb.Append(value);
        sb.Append("</color>");
        result = sb.ToString();
        return sb.Length;
    }

    public static string WrapInColor(string color, string value)
    {
        sb.Clear();
        sb.Append(color);
        sb.Append(value);
        sb.Append("</color>");
        return sb.ToString();
    }
}
