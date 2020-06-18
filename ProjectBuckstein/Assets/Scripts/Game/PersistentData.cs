////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  PersistentData : static class | Written by Parker Staszkiewicz and Anthony Pascone                            //
//  Static class for containing persistent data across scenes for the client.                                     //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public static class PersistentData {

    private static string userName;
    private static bool isHost;

    private static bool playerHasWon;
    private static bool isEndScene = false;

    /// <summary>
    /// Set persistent username for this client.
    /// </summary>
    /// <param name="uName">Name to set.</param>
    public static void SetUserName(string uName)
    {
        userName = uName;
    }

    /// <summary>
    /// Returns the username for this client.
    /// </summary>
    public static string GetUserName()
    {
        return userName;
    }

    /// <summary>
    /// Set persistent client type for this client.
    /// </summary>
    /// <param name="hosting">Whether this client is also a host.</param>
    public static void SetHosting(bool hosting)
    {
        isHost = hosting;
    }

    /// <summary>
    /// Returns the client type for this client.
    /// </summary>
    public static bool GetHosting()
    {
        return isHost;
    }

    /// <summary>
    /// Set if the player has won or not
    /// </summary>
    /// <param name="didThey">Deciding if the player has won</param>
    public static void SetPlayerHasWon(bool didThey)
    {
        playerHasWon = didThey;
    }

    /// <summary>
    /// Get if the player has won
    /// </summary>
    /// <returns></returns>
    public static bool GetPlayerHasWon()
    {
        return playerHasWon;
    }

    /// <summary>
    /// Set if the current scene is the end scene or not
    /// </summary>
    /// <param name="isEnd">Whether it is the end scene or not</param>
    public static void SetIsEndScene(bool isEnd)
    {
        isEndScene = isEnd;
    }

    /// <summary>
    /// Get if the end scene is the end scene
    /// </summary>
    /// <returns></returns>
    public static bool GetIsEndScene()
    {
        return isEndScene;
    }

}
