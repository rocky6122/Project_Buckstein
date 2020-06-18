////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  PlayerUnit : class | Written by Anthony Pascone                                                               //
//  Adds specific functionality to Player Units                                                                   //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public class PlayerUnit : Unit {

    /// <summary>
    /// Overrides base. Specifically calls the PlayerUnitManager 
    /// to select this Unit.
    /// </summary>
    public override void SelectUnit()
    {
        if (TurnManager.instance.GetCurrentManager() == manager)
        {
            PlayerUnitManager.instance.SelectUnit(this);
        }
    }

    /// <summary>
    /// Allows for the Unit to be selected by a mouse click.
    /// </summary>
    private void OnMouseDown()
    {
        SelectUnit();
    }
}
