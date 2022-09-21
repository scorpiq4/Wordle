namespace Wordle
{
    public class Character
    {
        public Character(char @char, CharacterStatus status)
        {
            Char = @char;
            Status = status;
        }
        public char Char { get; }
        public CharacterStatus Status { get; set; }
        public override string ToString() => $"{Char} - {Status}";
    }
}
