using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client;

public partial class MainWindow : Window
{
    private char MyType = '\0';
    private readonly TcpClient client;
    private readonly BinaryWriter bw;
    private readonly BinaryReader br;
    public MainWindow()
    {
        client = new TcpClient("127.0.0.1", 45678);
        InitializeComponent();

        var stream = client.GetStream();
        bw = new BinaryWriter(stream);
        br = new BinaryReader(stream);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Button clickedButton = (Button)sender;

        if (Table.IsEnabled && MyType == 'X')
            clickedButton.Content = 'X';
        else if (Table.IsEnabled && MyType == 'O')
            clickedButton.Content = 'O';

        int row = Grid.GetRow(clickedButton);
        int col = Grid.GetColumn(clickedButton);

        bw.Write($"Turn {row} {col} {clickedButton.Content} {tBox_Room.Text} {clickedButton.Name}");
        clickedButton.IsEnabled = false;
        Table.IsEnabled = false;
        var turnn = MyType == 'X' ? "O" : "X";
        tBox_Turn.Text = "Turn: " + turnn;
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        foreach (UIElement element in Table.Children)
        {
            if (element is Button)
            {
                Button button = (Button)element;
                button.Content = "";
            }
        }

        tBox_MyType.Text = "My Type: ";
        tBox_Turn.Text = "Turn: ";

        Button clickedButton = (Button)sender;

        switch (clickedButton.Name)
        {
            case "btn_Join":
                if (tBox_Room.Text.Length > 0)
                {
                    bw.Write($"Room {tBox_Room.Text}");
                    tBox_Room.IsEnabled = false;
                    btn_Join.IsEnabled = false;
                    btn_Leave.Visibility = Visibility.Visible;
                    tBox_GameStatus.Text = "Game Status: Waiting Player ...";
                }
                break;
            case "btn_Leave":
                bw.Write($"Room {tBox_Room.Text}");
                btn_Join.IsEnabled = true;
                tBox_Room.IsEnabled = true;
                btn_Leave.Visibility = Visibility.Hidden;
                tBox_GameStatus.Text = "Game Status: ";
                ResetGame();
                break;
        }      
    }

    private void ResetGame()
    {
        Dispatcher.Invoke(() => {
            foreach (UIElement element in Table.Children)
            {
                if (element is Button)
                {
                    Button button = (Button)element;
                    button.IsEnabled = true;
                    button.Content = "";
                }
            }

            MyType = '\0';

            tBox_GameStatus.Text = "Game Status: ";
            tBox_MyType.Text = "My Type: ";
            tBox_Turn.Text = "Turn: ";

            Table.IsEnabled = false;

            btn_Join.IsEnabled = true;
            tBox_Room.IsEnabled = true;
            btn_Leave.Visibility = Visibility.Hidden;
        });
    }

    private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (!char.IsDigit(e.Text, e.Text.Length - 1))
            e.Handled = true;
    }

    private void BoardManagment(string Line)
    {
        Dispatcher.Invoke(() => {
            Button myButton = this.FindName(Line) as Button;
            char tempType = '\0';

            if (myButton != null)
            {
                if (MyType == 'X')
                    tempType = 'O';
                else
                    tempType = 'X';
                myButton.Content = tempType.ToString();
                myButton.IsEnabled = false;
                
                Table.IsEnabled = true;
                tBox_Turn.Text = "Turn: " + MyType;
            }
        });
    }

    private async void ListenForData()
    {
        while (true)
        {
            var message = br.ReadString();
            if (message == "Room is Full")
            {
                Dispatcher.Invoke(() =>
                {
                    btn_Join.IsEnabled = true;
                    tBox_Room.IsEnabled = true;
                    btn_Leave.Visibility = Visibility.Hidden;
                });
                MessageBox.Show(message);
            }
            else if (message == "X")
            {
                MyType = 'X';
                Dispatcher.Invoke(() => { Table.IsEnabled = true; tBox_GameStatus.Text = "Game Status: Started"; tBox_MyType.Text = "My Type: X"; tBox_Turn.Text = "Turn: " + MyType; });
            }
            else if (message == "O")
            {
                MyType = 'O';
                Dispatcher.Invoke(() => { tBox_GameStatus.Text = "Game Status: Started"; tBox_MyType.Text = "My Type: O"; tBox_Turn.Text = "Turn: X"; });
            }
            else if (message == "One" || message == "Two" || message == "Three" || message == "Four" || message == "Five" || message == "Six" || message == "Seven" || message == "Eight" || message == "Nine")
                BoardManagment(message);
            else if (message == "X is Winner" || message == "O is Winner")
            {
                Dispatcher.Invoke(() => { Table.IsEnabled = false; tBox_GameStatus.Text = "Game Status: Stoped"; });
                MessageBox.Show(message);
                ResetGame();
            }
            else if (message == "Reseting Game")
                ResetGame();
            else if (message == "Draw")
            {
                MessageBox.Show(message);
                ResetGame();
            }
                
            //MessageBox.Show(message);
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) => Task.Run(() => ListenForData());
}
