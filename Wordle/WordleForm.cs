using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace Wordle;

[SupportedOSPlatform("windows")]
public partial class WordleForm : Form
{
    private Dictionary<char, Button> _keyboard;
    public WordleGame Game { get; private set; }

    public WordleForm()
    {
        InitializeComponent();
        InitKeyboardDictionary();

        KeyPreview = true;

        NewGame();
    }

    protected override void OnShown(EventArgs e)
    {
        buttonEnter.Focus();
        base.OnShown(e);
    }

    [MemberNotNull(nameof(_keyboard))]
    private void InitKeyboardDictionary()
    {
        _keyboard = this.ControlsOfType<Button>()
            .Where(i => i.Text.Length == 1)
            .ToDictionary(i => i.Text[0]);
    }

    [MemberNotNull(nameof(Game))]
    private void NewGame()
    {
        Game = new WordleGame(new GameOptions { Tries = 6, WordLength = 5 });
        ClearGuesses();
        ClearKeyboard();
        TryAgain();
    }

    private void TryAgain()
    {
        Game.Tries++;
        for (int i = 0; i < Game.Options.WordLength; i++)
        {
            var button = new Button { Dock = DockStyle.Fill };
            button.Font = new Font(button.Font.FontFamily, button.Font.Size + 10);
            tableLayoutPanelGuesses.Controls.Add(button, i, Game.Tries - 1);
        }
    }

    private void Button_Click(object sender, EventArgs e)
    {
        if (sender is Button button)
            ProcessText(button.Text);
    }

    private void WordleForm_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (e.KeyChar == (char)Keys.Enter)
            ProcessText("Enter");
        else if (e.KeyChar == (char)Keys.Back)
            ProcessText("Back");
        else  if ((e.KeyChar >= 'A' && e.KeyChar <= 'Z') || (e.KeyChar >= 'a' && e.KeyChar <= 'z'))
            ProcessText(e.KeyChar.ToString().ToUpper());

        e.Handled = true;
    }

    private void ProcessText(string text)
    {
        var guessWord = GetGuessWord();

        if (text == "Enter")
        {
            if (guessWord.Length < Game.Options.WordLength)
                return;

            if (!Dictionary.WordsHash.Contains(guessWord))
            {
                MessageBox.Show("Word not found");
                return;
            }

            var youWon = Game.EvaluateGuessWord(guessWord);

            for (int characterPosition = 0; characterPosition < Game.Options.WordLength; characterPosition++)
                GetGuessButton(characterPosition).BackColor = GetColor(Game.GetCharacterStatus(characterPosition));

            for (var i = 'A'; i <= 'Z'; i++)
            {
                if (Game.CharactersPlayed.TryGetValue(i, out var character))
                    _keyboard[i].BackColor = GetColor(character.Status);
            }

            if (youWon)
            {
                MessageBox.Show("You won!");
                NewGame();
            }
            else if (Game.Tries == Game.Options.Tries)
            {
                MessageBox.Show($"You lost! {Game.Word}");
                NewGame();
            }
            else
            {
                TryAgain();
            }
        }
        else if (text == "Back")
        {
            if (guessWord.Length > 0)
                GetGuessButton(guessWord.Length - 1).Text = string.Empty;
        }
        else if (guessWord.Length < Game.Options.WordLength)
        {
            GetGuessButton(guessWord.Length).Text = text;
        }
    }

    private void ClearGuesses()
    {
        for (var row = 0; row < Game.Options.Tries; row++)
        {
            for (var characterPosition = 0; characterPosition < Game.Options.WordLength; characterPosition++)
            {
                if (tableLayoutPanelGuesses.GetControlFromPosition(characterPosition, row) is Button button)
                {
                    tableLayoutPanelGuesses.Controls.Remove(button);
                    button.Dispose();
                }
            }
        }
    }

    private void ClearKeyboard()
    {
        foreach (var button in _keyboard.Values)
            button.BackColor = SystemColors.Control;
    }

    private string GetGuessWord()
    {
        var guessWord = string.Empty;
        for (var characterPosition = 0; characterPosition < Game.Options.WordLength; characterPosition++)
        {
            var button = GetGuessButton(characterPosition);
            if (button.Text == string.Empty)
                break;
            guessWord += button.Text;
        }
        return guessWord;
    }

    private Button GetGuessButton(int characterPosition)
    {
        return (Button)tableLayoutPanelGuesses.GetControlFromPosition(characterPosition, Game.Tries - 1)!;
    }

    private static Color GetColor(CharacterStatus status)
    {
        return status switch
        {
            CharacterStatus.ExactMatch => Color.LimeGreen,
            CharacterStatus.Match => Color.Gold,
            CharacterStatus.Eliminated => Color.DarkGray,
            _ => Color.DarkGray
        };
    }
}
