using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Antlr4.Runtime;
using System.Linq;
using Antlr4.Runtime.Misc;

class StringColor
{
    public int startPos;
    public int endPos;

    public string color;

    public void Modify(ref string str)
    {
        str = str.Insert(endPos + 1, "</color>");
        str = str.Insert(startPos, "<color=" + color + ">");
    }
}

class LuaHighlighter : LuaBaseVisitor<bool>
{
    string code;

    private List<StringColor> colors = new List<StringColor>();
    private LuaParser.ChunkContext AST;

    Dictionary<string, string> HighLightColors = new Dictionary<string, string>
        {
            { "KEYWORDS", "#c678dd" },
            { "OPERATORS", "#56b6c2" },
            { "COMMENT", "#7f848e" },
            { "LINE_COMMENT", "#7f848e" },
            { "INT", "#d19a66" },
            { "FLOAT", "#d19a66" },
            { "HEX", "#d19a66" },
            { "HEX_FLOAT", "#d19a66" },
        };

    private static readonly string[] _KeyWords =
    {
            "'break'", "'goto'", "'do'", "'end'", "'while'", "'repeat'",
            "'until'", "'if'", "'then'", "'elseif'", "'else'", "'for'",
            "'in'", "'function'", "'local'", "'return'", "'nil'", "'false'", "'true'"
        };

    private static readonly string[] _Operators =
    {
            "'='", "'...'", "'or'", "'and'", "'<='", "'>='", "'~='", "'=='", "'..'", "'+'","'<'", "'>'",
            "'-'", "'*'", "'/'", "'%'", "'&'", "'|'", "'~'", "'<<'", "'>>'", "'not'", "'#'", "'^'"
        };

    public LuaHighlighter(string str)
    {
        code = str;

        var stream = new AntlrInputStream(code);
        var lexer = new LuaLexer(stream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new LuaParser(tokenStream);

        tokenStream.Fill();
        var tokens = tokenStream.GetTokens();

        foreach (var token in tokens)
        {
            var style = lexer.Vocabulary.GetDisplayName(token.Type);

            if (_KeyWords.Contains(style))
                style = "KEYWORDS";
            else if (_Operators.Contains(style))
                style = "OPERATORS";

            if (HighLightColors.ContainsKey(style))
            {
                colors.Add(new StringColor
                {
                    color = HighLightColors[style],
                    startPos = token.StartIndex,
                    endPos = token.StopIndex
                });
            }
        }

        AST = parser.chunk();
    }

    public override bool VisitParlist([NotNull] LuaParser.ParlistContext context)
    {
        var childs = context.namelist().NAME();

        foreach (var par in childs)
        {
            colors.Add(new StringColor
            {
                color = "#e06c75",
                startPos = par.Symbol.StartIndex,
                endPos = par.Symbol.StopIndex
            });
        }

        return base.VisitParlist(context);
    }

    public override bool VisitString([NotNull] LuaParser.StringContext context)
    {
        colors.Add(new StringColor
        {
            color = "#98c379",
            startPos = context.Start.StartIndex,
            endPos = context.Start.StopIndex
        });

        return base.VisitString(context);
    }

    public override bool VisitVar([NotNull] LuaParser.VarContext context)
    {
        colors.Add(new StringColor
        {
            color = "#61afef",
            startPos = context.Start.StartIndex,
            endPos = context.Start.StopIndex
        });

        return base.VisitVar(context);
    }

    public override bool VisitFuncname([NotNull] LuaParser.FuncnameContext context)
    {
        colors.Add(new StringColor
        {
            color = "#56b6c2",
            startPos = context.Start.StartIndex,
            endPos = context.Start.StopIndex
        });

        return base.VisitFuncname(context);
    }

    public string GetHighlightedCode()
    {
        Visit(AST);

        colors.Sort((a, b) =>
        {
            return b.startPos - a.startPos;
        });

        string result = code;

        foreach (var color in colors)
            color.Modify(ref result);

        return result;
    }
}

public class ScriptEditor : MonoBehaviour
{
    public Text displayText;
    public RectTransform viewport;
    public Canvas editorCanvas;
    public InputField input;

    private RectTransform rect;

    private bool codeChanged = false;

    private string highlightedCode = "";
    private int cursorPos = 0;
    private float lineHeight = 0.0f;

    public string Code { get { return input.text; } set { input.text = value; } }

    // Start is called before the first frame update
    void Start()
    {
        rect = transform as RectTransform;

        lineHeight = displayText.fontSize + displayText.fontSize / 5;
    }

    // Update is called once per frame
    void Update()
    {
        if(codeChanged)
        {
            codeChanged = false;

            var highlighter = new LuaHighlighter(input.text);
            highlightedCode = highlighter.GetHighlightedCode();

            displayText.text = highlightedCode;
        }

        var size = rect.sizeDelta;

        size.y = displayText.preferredHeight + 15 - viewport.rect.height;

        if (size.y <= 0)
            size.y = 0;

        rect.sizeDelta = size;

        if (input.caretPosition != cursorPos)
            OnCursorMoved();

        cursorPos = input.caretPosition;
    }

    private void OnCursorMoved()
    {
        int currentLine = 0;

        unsafe
        {
            fixed (char* text = input.text)
            {
                char* search = text + input.caretPosition;

                while ((--search) >= text)
                    if (*search == '\n')
                        currentLine++;
            }
        }

        var minLine = (int)Math.Ceiling(rect.offsetMax.y / lineHeight);
        var maxLine = minLine + (int)Math.Ceiling(viewport.rect.height / lineHeight);

        if (currentLine < minLine || currentLine > maxLine)
        {
            var line = currentLine < minLine ? minLine : maxLine;

            var delta = lineHeight * (currentLine - line);
            var pos = rect.anchoredPosition;
            pos.y += delta;

            rect.anchoredPosition = pos;
        }
    }

    public void OnScriptEditorOpen()
    {
        if(string.IsNullOrEmpty(Code))
            Code = "-- BanGround Chart Script\r\nfunction OnUpdate(audioTime)\r\n\r\nend\r\n";

        editorCanvas.gameObject.SetActive(true);
    }

    public void OnScriptEditorClose()
    {
        editorCanvas.gameObject.SetActive(false);
    }

    public void OnTextChanged() => codeChanged = true;
}
