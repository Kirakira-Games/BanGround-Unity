using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Antlr4.Runtime;
using System.Linq;

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

    private const string _NormalColor = "#abb2bf";

    public string Code { get { return input.text; } set { input.text = value; } }

    Dictionary<string, string> HighLightColors = new Dictionary<string, string>
    {
        { "KEYWORDS", "#c678dd" },
        { "OPERATORS", "#56b6c2" },
        { "NORMALSTRING", "#98c379" },
        { "CHARSTRING", "#98c379" },
        { "LONGSTRING", "#98c379" },
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
            
            var stream = new AntlrInputStream(input.text);
            var lexer = new LuaLexer(stream);
            var tokenStream = new CommonTokenStream(lexer);
            tokenStream.Fill();
            var tokens = tokenStream.GetTokens();

            highlightedCode = input.text;

            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                var token = tokens[i];

                var style = lexer.Vocabulary.GetDisplayName(token.Type);

                if (_KeyWords.Contains(style))
                    style = "KEYWORDS";
                else if (_Operators.Contains(style))
                    style = "OPERATORS";

                var color = HighLightColors.ContainsKey(style) ? HighLightColors[style] : _NormalColor;

                highlightedCode = highlightedCode.Insert(token.StopIndex + 1, "</color>");
                highlightedCode = highlightedCode.Insert(token.StartIndex, "<color=" + color + ">");
            }

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
