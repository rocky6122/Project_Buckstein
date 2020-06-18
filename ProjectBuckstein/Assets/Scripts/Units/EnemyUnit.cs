////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  PlayerUnit : class | Written by Anthony Pascone                                                               //
//  Adds specific functionality to Enemy Units.                                                                   //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public class EnemyUnit : Unit {

    Utility utilityAI;

    private void Start()
    {
        utilityAI = new Utility();
        utilityAI.SetUnit(this);
    }

    public override void SelectUnit()
    {
        EnemyUnitManager.instance.SelectUnit(this);
    }

    public Utility GetUtilityAI()
    {
        return utilityAI;
    }

    protected override void OnMouseOver()
    {
        base.OnMouseOver();

        if (canBeShot)
        {
            GameManager.instance.ShowDamageText();
        }
    }

    private void OnMouseDown()
    {
        PlayerUnitManager.instance.Shoot(this);
    }
}
