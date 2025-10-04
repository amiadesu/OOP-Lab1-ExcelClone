using System;
using ExcelClone.Utils;

namespace ExcelClone.Evaluators.Automatons;

public class CellNameAutomaton : IAutomaton
{
    private AutomatonState _state = AutomatonState.Initial;
    private int _inner_state = 0;

    public AutomatonState Insert(char input)
    {
        this.EvaluateInput(input);
        return this._state;
    }

    public AutomatonState GetCurrentState()
    {
        return this._state;
    }

    public bool TestChar(char input)
    {
        return CharChecker.IsUpperLatin(input);
    }

    public bool TestString(string input)
    {
        Reset();
        bool result = false;
        foreach (char c in input)
        {
            EvaluateInput(c);
            if (this._state == AutomatonState.Rejecting)
            {
                result = false;
                break;
            }
        }
        if (this._state == AutomatonState.Accepting)
        {
            result = true;
        }
        Reset();
        return result;
    }

    public void Reset()
    {
        this._state = AutomatonState.Initial;
        this._inner_state = 0;
    }

    private void EvaluateInput(char input)
    {
        if (this._inner_state == 3)
        {
            return;
        }

        switch (this._inner_state)
        {
            case 0:
                if (CharChecker.IsUpperLatin(input))
                {
                    this._inner_state = 1;
                }
                else
                {
                    this._inner_state = 3;
                }
                break;

            case 1:
                if (CharChecker.IsNotZeroDigit(input))
                {
                    this._inner_state = 2;
                }
                else if (!CharChecker.IsUpperLatin(input))
                {
                    this._inner_state = 3;
                }
                break;

            case 2:
                if (!Char.IsDigit(input))
                {
                    this._inner_state = 3;
                }
                break;
        }
        if (this._inner_state == 3)
        {
            this._state = AutomatonState.Rejecting;
        }
        else if (this._inner_state == 2)
        {
            this._state = AutomatonState.Accepting;
        }
        else
        {
            this._state = AutomatonState.Processing;
        }
    }
}