using Server;
using System.Net;
using System.Net.Sockets;

var listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 45678);
listener.Start();
Console.WriteLine("Listening...");

var clients = new HashSet<TcpClient>();

Dictionary<int, List<string>> Room = new Dictionary<int, List<string>>();
TicTacToe game = new TicTacToe();

while (true)
{
    var client = await listener.AcceptTcpClientAsync();
    clients.Add(client);
    var User = client.Client.RemoteEndPoint.ToString();

    new Task(() =>
    {
        var stream = client.GetStream();
        var bw = new BinaryWriter(stream);
        var br = new BinaryReader(stream);

        void SendData(int _roomId, string text)
        {
            foreach (var clientAddress in Room[_roomId])
            {
                var destination = clients.FirstOrDefault(c => c.Client.RemoteEndPoint?.ToString() == clientAddress);
                bw = new BinaryWriter(destination!.GetStream());
                bw.Write(text);
            }
        }

        while (true)
        {
            var message = br.ReadString();
            var splitedMessage = message.Split(' ');

            if (splitedMessage[0] == "Room")
            {
                int roomId = int.Parse(message.Split(' ')[1]);

                if (Room.ContainsKey(roomId))
                {
                    try
                    {
                        if (!Room[roomId].Contains(User))
                        {
                            if (Room[roomId].Count < 2)
                            {
                                Room[roomId].Add(User);
                                game.AddGamer(User, "O");
                                // bw.Write($"{User} - {roomId} Joined Room");
                                if (Room[roomId].Count == 2)
                                {
                                    foreach (var clientAddress in Room[roomId])
                                    {
                                        var destination = clients.FirstOrDefault(c => c.Client.RemoteEndPoint?.ToString() == clientAddress);

                                        bw = new BinaryWriter(destination!.GetStream());
                                        bw.Write(game.GetUserType(clientAddress));
                                    }

                                    game.StartRoomGame(roomId);
                                }
                            }
                            else
                                bw.Write("Room is Full");
                        }
                        else
                        {
                            Room[roomId].Remove(User);
                            SendData(roomId, "Reseting Game");
                            game.ResetRoomGame(roomId);
                            Room.Remove(roomId);
                        }
                    }
                    catch (IOException)
                    {
                        Room[roomId].Remove(User);
                        SendData(roomId, "Reseting Game");
                        game.ResetRoomGame(roomId);
                        Room.Remove(roomId);
                    }
                }
                else
                {
                    Room.Add(roomId, new List<string> { User });
                    game.AddGamer(User, "X");
                }
            }
            else if (splitedMessage[0] == "Turn")
            {
                
                int row = int.Parse(splitedMessage[1]);
                int col = int.Parse(splitedMessage[2]);
                int roomId = int.Parse(splitedMessage[4]);
                string type = splitedMessage[3];
                Console.WriteLine(game.GetCurrentMove(roomId));
                if (game.GetCurrentMove(roomId) == 8)
                {
                    SendData(roomId, "Draw");
                    game.ResetRoomGame(roomId);
                    Room.Remove(roomId);
                }
                else
                {
                    if (!game.UpdateBoard(roomId, row, col, type))
                    {
                        foreach (var clientAddress in Room[roomId])
                        {
                            if (clientAddress != User)
                            {
                                var destination = clients.FirstOrDefault(c => c.Client.RemoteEndPoint?.ToString() == clientAddress);

                                bw = new BinaryWriter(destination!.GetStream());
                                bw.Write(splitedMessage[5]);
                            }
                        }
                    }
                    else
                        SendData(roomId, $"{type} is Winner");
                } 
            }
        }
    }).Start();
}