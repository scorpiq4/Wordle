namespace Wordle;

public class Character(char @char, CharacterStatus status)
{
    public char Char { get; } = @char;
    public CharacterStatus Status { get; set; } = status;
    public override string ToString() => $"{Char} - {Status}";
}
