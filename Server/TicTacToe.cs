namespace Server;

public class TicTacToe
{
    private Dictionary<string, string> Game = new();
    private Dictionary<int, char[,]> RoomCurrentGame = new();

    public void AddGamer(string User, string Type)
    {
        if (!Game.ContainsKey(User))
            Game.Add(User, Type);
        else
            Game[User] = Type;
    }

    public void StartRoomGame(int RoomId) => RoomCurrentGame.Add(RoomId, new char[3, 3]);

    public string GetUserType(string User) => Game[User];

    public void ResetRoomGame(int roomId) => RoomCurrentGame.Remove(roomId);

    public bool UpdateBoard(int roomId, int row, int col, string type)
    {
        char[,] currentGame = RoomCurrentGame[roomId];

        if (currentGame[row, col] == '\0')
        {
            currentGame[row, col] = type == "X" ? 'X' : 'O';

            if (currentGame[row, 0] == currentGame[row, 1] && currentGame[row, 1] == currentGame[row, 2] ||
                currentGame[0, col] == currentGame[1, col] && currentGame[1, col] == currentGame[2, col] ||
                currentGame[0, 0] == currentGame[1, 1] && currentGame[1, 1] == currentGame[2, 2] && currentGame[1, 1] != '\0' ||
                currentGame[0, 2] == currentGame[1, 1] && currentGame[1, 1] == currentGame[2, 0] && currentGame[1, 1] != '\0')
            {
                return true;
            }
        }
        return false;
    }
}
