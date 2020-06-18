////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  MathLibrary: class | Written by Parker Staszkiewicz                                                           //
//  An extensions class for math functions to be handled on variables.                                            //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public static class MathLibrary  {

    /// <summary>
    /// Maps the value of a float from one range to another.
    /// </summary>
    /// <param name="value">Value to be mapped.</param>
    /// <param name="fromMin">Minumum value of original range.</param>
    /// <param name="toMin">Minimum value of new range.</param>
    /// <param name="fromMax">Maximum value of original range.</param>
    /// <param name="toMax">Maximum value of new range.</param>
    public static float Map(this float value, float fromMin, float toMin, float fromMax, float toMax)
    {
        return (value - fromMin) / (toMin - fromMin) * (toMax - fromMax) + fromMax;
    }
}
