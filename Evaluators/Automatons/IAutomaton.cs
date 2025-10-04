namespace ExcelClone.Evaluators.Automatons;

public interface IAutomaton
{
    AutomatonState Insert(char input);
    AutomatonState GetCurrentState();
    /// <summary>
    /// Test if automaton will accept this character as a starting one.
    /// </summary>
    bool TestChar(char input);
    /// <summary>
    /// Test if automaton will accept this string.<br/>
    /// Calling this method will reset automaton!
    /// </summary>
    bool TestString(string input);
    void Reset();
}