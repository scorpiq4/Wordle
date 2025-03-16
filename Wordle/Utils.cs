using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Windows.Forms;
using static System.Windows.Forms.Control;

namespace Wordle;

public static class Utils
{
    public static IEnumerable<T> ControlsOfType<T>(this Control control)
    {
        foreach (var child in control.Controls.OfType<Control>())
        {
            if (child is T ofType)
                yield return ofType;
            foreach (var descendent in ControlsOfType<T>(child))
                yield return descendent;
        }
    }
}
