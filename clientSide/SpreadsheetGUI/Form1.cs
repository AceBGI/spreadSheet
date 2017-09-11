// alex eakle v2

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SS;
using SpreadsheetUtilities;
using NetworkController;

namespace SpreadsheetGUI
{
    public partial class Form1 : Form
    {
        public Spreadsheet model;
        HashSet<string> reDrawCellsValue;
        public Form1()
        {
            InitializeComponent();
            spreadsheetPanel1.SelectionChanged += displaySelection;
            spreadsheetPanel1.SelectionChanged += showCellContent;
            spreadsheetPanel1.SelectionChanged += showCellValue;
            model = new Spreadsheet(s => true, x => x.ToUpper(), "ps6");
        }

        /// <summary>
        /// shows the content of the cell in the CellContent TextBox
        /// </summary>
        /// <param name="ss"></param>
        private void showCellContent(SpreadsheetPanel ss)
        {
            int row, col;
            ss.GetSelection(out col, out row);
            CellContent.Text = (string)model.GetCellContents(colLetters[col] + (row + 1).ToString()).ToString();
        }

        /// <summary>
        /// Shows the value of the cell in the cellValue textbox
        /// </summary>
        /// <param name="ss"></param>
        private void showCellValue(SpreadsheetPanel ss)
        {
            int row, col;
            string value;
            ss.GetSelection(out col, out row);
            ss.GetValue(col, row, out value);
            cellValue.Text = value;
        }

        /// <summary>
        /// Event handler for When a new cell is selected
        /// </summary>
        /// <param name="ss"></param>
        private void displaySelection(SpreadsheetPanel ss)
        {
            int row, col;
            String value;
            ss.GetSelection(out col, out row);
            ss.GetValue(col, row, out value);

            // show the content for the cell in the cellContent text box
            CellContent.Text = model.GetCellContents(colLetters[col] + (row + 1).ToString() ).ToString();

            // updates all of the cells values when SelectionChanged
            updateCellsValues(reDrawCellsValue);
            

            // change the label to the curent cell
            cellLabel.Text = colLetters[col] + (row + 1).ToString();


            // if (value == "")
            // {
            //     ss.SetValue(col, row, col.ToString() + "-" + row.ToString());
            //     ss.GetValue(col, row, out value);
            //     //MessageBox.Show("Selection: column " + col + " row " + row + " value " + value);
            // }
        }

        /// <summary>
        /// 2 dictionaries that are used to refference the Cells in the GUI
        /// </summary>
        private Dictionary<int, string> colLetters = new Dictionary<int, string>()
        {
            {0, "A"},{1, "B"},{2, "C"},{3, "D"},{4, "E"},{5, "F"},{6, "G"},{7, "H"},{8, "I"},{9, "J"},{10, "K"},{11, "L"},{12, "M"},
            { 13, "N"},{14, "O"},{15, "P"},{16, "Q"},{17, "R"},{18, "S"},{19, "T"},{20, "U"},{21, "V"},{22, "W"},{23, "X"},{24, "Y"},{25, "Z"}
        };

        private Dictionary<string, int> LettersToCol = new Dictionary<string, int>()
        {
            {"A", 0},{"B", 1},{"C",2},{"D",3},{"E", 4},{"F", 5},{"G", 6},{"H", 7},{"I",8},{"J",9},{"K", 10},{"L", 11},{"M",12},
            {"N", 13},{"O", 14},{"P", 15},{"Q", 16},{"R", 17},{"S", 18},{"T", 19},{"U", 20},{"V", 21},{"W", 22},{"X", 23},{"Y", 24},{"Z", 25}
        };

        /// <summary>
        /// Updates the value of the cells that have just been recalculated.
        /// </summary>
        /// <param name="reDrawCellsValue"></param>
        private void updateCellsValues(HashSet<string> reDrawCellsValue)
        {
            if (reDrawCellsValue != null)
            {
                foreach (string item in reDrawCellsValue)
                {
                    int number;
                    int c = LettersToCol[item[0].ToString()];


                    Int32.TryParse(item.Remove(0, 1), out number);
                    int r = number;

                    spreadsheetPanel1.SetValue(c, r - 1, model.GetCellValue(item).ToString());

                }
            }
        }


        /// <summary>
        /// Opens up a new Spreadsheet in a new window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Tell the application context to run the form on the same
            // thread as the other forms.
            DemoApplicationContext.getAppContext().RunForm(new Form1());
        }

        /// <summary>
        /// closes all of the forms. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Button that sets the cellContent textbox as content of the cells.
        /// can be called when "enter" is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            int row, col;
            bool errorBox = false;

            // get the current selection
            spreadsheetPanel1.GetSelection(out col, out row);

            try
            {
                reDrawCellsValue = new HashSet<string>(model.SetContentsOfCell(colLetters[col] + (row + 1).ToString(), CellContent.Text));
            }
            catch (CircularException)
            {
                DialogResult result = MessageBox.Show("Circular Exception: You have a cell that is referencing itself. Either in its content or in a cell that it depends on.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                CellContent.Text = model.GetCellContents(colLetters[col] + (row + 1).ToString()).ToString();
                errorBox = true;
            }
            catch (Exception f)
            {
                DialogResult result = MessageBox.Show(f.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                CellContent.Text = model.GetCellContents(colLetters[col] + (row + 1).ToString()).ToString();
                errorBox = true;
            }

            if(errorBox == false)
            {
                // if you return a formulaError from getcellvalue display error reason in cellcontent
                if (model.GetCellValue(colLetters[col] + (row + 1).ToString()).GetType() == typeof(FormulaError))
                {
                    FormulaError fError = (FormulaError)model.GetCellValue(colLetters[col] + (row + 1).ToString());
                    CellContent.Text = fError.Reason;
                }
                string value = CellContent.Text;

                // pass value to SetContentsOfCell
                updateCellsValues(reDrawCellsValue);

                // update the cellvalue text box
                spreadsheetPanel1.GetValue(col, row, out value);
                cellValue.Text = value;
            }
        }

        // keeps track of the file that you saved to or opened from
        private bool ExistingFile { get; set; }
        private string FilesName { get; set; }

        /// <summary>
        /// if the file does not already exist then call saveAsToolStripMenuItem_Click
        /// if the file does exist then save the file to that file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ExistingFile == false)
                saveAsToolStripMenuItem_Click(sender, e);
            else
                model.Save(FilesName);
        }

        /// <summary>
        /// save the file to the path that is provided from the dialog box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialogBox = new SaveFileDialog();
            saveFileDialogBox.Filter = "Spreadsheet Files|*.sprd|All files (*.*)|*.*";
            saveFileDialogBox.AddExtension = true;
            saveFileDialogBox.Title = "Save an Spreadsheet Files";
            saveFileDialogBox.ShowDialog();

            if (saveFileDialogBox.FileName != "")
            {
                model.Save(saveFileDialogBox.FileName);
                ExistingFile = true;
                FilesName = saveFileDialogBox.FileName;
            }
        }

        /// <summary>
        /// Opens up a file from a file path given by user.
        /// updates forms GUI with newly loaded cells
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialogBox = new OpenFileDialog();
            openDialogBox.Filter = "Spreadsheet Files|*.sprd|All files (*.*)|*.*";
            openDialogBox.Title = "Open an Spreadsheet Files";
            openDialogBox.ShowDialog();

            if (openDialogBox.FileName != "")
            {
                try
                {
                    model = new Spreadsheet(openDialogBox.FileName, s => true, x => x.ToUpper(), "ps6");
                }
                catch (SpreadsheetReadWriteException)
                {
                    DialogResult result = MessageBox.Show("There was a error with loading your file.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
                ExistingFile = true;
                FilesName = openDialogBox.FileName;
            }

            // takes all of the new cells that have been loaded into the spreadsheet
            // and put them into the GUI's cells
            HashSet<string> loadedCells = new HashSet<string>();
            foreach (var key in model.namesContents.Keys)
            {
                loadedCells.Add(key);
            }
            updateCellsValues(loadedCells);

        }

        /// <summary>
        /// helps you get a specific cell
        /// its puts the current selected cell into your clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getCellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int row, col;
            spreadsheetPanel1.GetSelection(out col, out row);
            Clipboard.SetText(colLetters[col] + (row + 1).ToString());
        }

        /// <summary>
        /// copies the text from the cellContent textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int row, col;
            spreadsheetPanel1.GetSelection(out col, out row);

            Clipboard.SetText(CellContent.Text);
        }

        /// <summary>
        /// past text in clipboard into cellcontent textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CellContent.Text = Clipboard.GetText();
        }

        /// <summary>
        /// Checks to see if the spreadsheet has not been saved ask if you could like to save it before closing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (model.Changed == true)
            {
                // Display a message box asking users if they
                // want to exit the application.
                DialogResult result = MessageBox.Show("Want to save your changes before exiting?", "Warning",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    saveToolStripMenuItem_Click(sender, e);
                }
            }
        }

        /// <summary>
        /// help menu for users / TA
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("___________________________________________________________________________\r\n" +
                "-- Basic Instructions -- \r\n" +
                "To enter information into the spreadsheet cells first click on the editable tetbox at the top. \r\n" +
                "\r\n" +
                " To enter text into cells you must use the button or hit/press enter. \r\n" +
                "___________________________________________________________________________\r\n" +
                "\r\n" +
                "-- Extra Features -- \r\n" +
                "-Keyshortcut: Many of the items in the menu have key shortcuts that you can use to make your life easier. like CTRL G to use 'Get cell' or press enter to add text from the content textbox into a cell, instead of pressing the button.\r\n" +
                "\r\n" +
                "-Edit: \r\n" +
                "     Copy: copies the content of your cell into your clipboard.\r\n" +
                "     Paste: pastes the text from your clipboard into the current cells content.\r\n" +
                "     Get cell: Gets the cells name from the current selected cell and save it into your clipboard. You can use this in formulas to easily get the name of the current cell then you can paste this into another cells content.\r\n" +
                "\r\n" +
                "-Arrow keys: While selected on cell/cells content box you are able to use the arrow keys to move around on the grid of cells.\r\n" +
                "If you have namy question you can text us at 801-696-9024. \r\n",
                "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// move between cells using the arrow keys. updates the cellContent and cell label
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CellContent_KeyDown(object sender, KeyEventArgs e)
        {
            int col, row;
            spreadsheetPanel1.GetSelection(out col, out row);
            spreadsheetPanel1.SetColor(col, row, Color.White);

            if (e.KeyCode == Keys.Left)
            {
                spreadsheetPanel1.SetSelection((col - 1), row);
                updateLabelContent(col, row);
                e.Handled = true;
            }
                
            if (e.KeyCode == Keys.Right)
            {
                spreadsheetPanel1.SetSelection((col + 1), row);
                updateLabelContent(col, row);
                e.Handled = true;
            }
                
            if (e.KeyCode == Keys.Up)
            {
                spreadsheetPanel1.SetSelection(col, (row - 1));
                updateLabelContent(col, row);
                e.Handled = true;
            }
                
            if (e.KeyCode == Keys.Down)
            {
                spreadsheetPanel1.SetSelection(col, (row + 1));
                updateLabelContent(col, row);
                e.Handled = true;
            }

            spreadsheetPanel1.GetSelection(out col, out row);
            spreadsheetPanel1.SetColor(col, row, Color.Red);
            // base.OnKeyDown(e);
        }

        /// <summary>
        /// helper function, helps get the content for the newly selected cell.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="r"></param>
        private void updateLabelContent(int c, int r)
        {
            int col = c;
            int row = r;
            string value;

            // update cellLabel
            spreadsheetPanel1.GetSelection(out col, out row);
            cellLabel.Text = colLetters[col] + (row + 1).ToString();

            //update cellContent
            CellContent.Text = (string)model.GetCellContents(colLetters[col] + (row + 1).ToString()).ToString();

            // update cellValue
            spreadsheetPanel1.GetValue(col, row, out value);
            cellValue.Text = value;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CellContent.Focus();
        }

        private void spreadsheetPanel1_Load(object sender, EventArgs e)
        {
            CellContent.Focus();
        }

        private void spreadsheetPanel1_Load_1(object sender, EventArgs e)
        {

        }


        // conects client to server
        private void connectToServerMenuItem_Click(object sender, EventArgs e)
        {
            FormConnect testDialog = new FormConnect();

            // Show testDialog as a modal dialog and determine if DialogResult = OK.
            if (testDialog.ShowDialog(this) == DialogResult.OK)
            {
                // Read the contents of testDialog's TextBox.
                this.Text = testDialog.Text;
            }
            else
            {
                this.Text = "Cancelled";
            }
            testDialog.Dispose();

        }

        public partial class FormConnect : Form
        {
            public FormConnect()
            {
                InitializeComponent();
            }

            public string getText()
            {
                return textBox1.Text;
            }
        }




#if false
        // create the socket state. ***********************************
        private SocketState theServer;

        // This object represents the world.
        // In this simple demo, the world consists of one dot
        private SnakeWorld world;

        // Fake data that will simulate a message from the server
        private int worldX = 150, worldY = 150;

        //ID of the player's snake.
        public int playerID { get; private set; }








        /// <summary>
        /// Takes the name from the text box and sets up the socket to recieve startup data
        /// Then it will send that data off. 
        /// </summary>
        /// <param name="ss"></param>
        public void FirstContact(SocketState ss)
        {
            string Name = messageToSendBox.Text;
            ss.EventCallback = ReceiveStartUp;
            Networking.Send(ss, Name);
        }

        /// <summary>
        /// Processes the Startup data and sets the socket up to recieve world updates. 
        /// starts loop to get more data. 
        /// </summary>
        /// <param name="ss"></param>
        public void ReceiveStartUp(SocketState ss)
        {
            ProcessStartUpMessage(ss);
            ss.EventCallback = ReceiveWorld;
            Networking.GetData(ss);
        }


        /// <summary>
        /// Processes the world data and continues the loop to look for more data.
        /// </summary>
        /// <param name="ss"></param>
        public void ReceiveWorld(SocketState ss)
        {
            ProcessWorldMessage(ss);
            Networking.GetData(ss);
        }


        /// <summary>
        /// This method takes in the initial data and sets the player name and panel size.
        /// </summary>
        /// <param name="ss"></param>
        private void ProcessStartUpMessage(SocketState ss)
        {
            //Break the message in the substring into parts to be processed. 
            string totalData = ss.sb.ToString();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            int numHolder;

            //Handle ID
            if (Int32.TryParse(parts[0], out numHolder))
            {
                playerID = numHolder;
                world.playerID = numHolder;
            }

            // do something *********************************************************

            // Then remove it from the SocketState's growable buffer            
            ss.sb.Clear();
            ss.messageBuffer = new byte[1024];
        }

        /// <summary>
        /// Takes in the world data from Json and builds the world at that frame. 
        /// </summary>
        /// <param name="ss"></param>
        private void ProcessWorldMessage(SocketState ss)
        {
            string totalData = ss.sb.ToString();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");


            // Loop until we have processed all messages.
            // We may have received more than one.

            foreach (string p in parts)
            {
                //Ignore strings that are too small to contain useful information.
                if (p.Length <= 2)
                    continue;
                // Ignore empty strings added by the regex splitter or strings that don't start with ID.
                if (p.Length == 0 || p[2] != 'I')
                    continue;
                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (p[p.Length - 1] != '\n')
                    break;

                if (p.Length <= 4)
                {
                    MessageBox.Show("not long enough data.");
                }


                // parse the string p and check to see what type of object it is. *********************************************
                // then convert string by deserializing it into the specified object. 
                JObject obj = JObject.Parse(p);

                JToken snakeToken = obj["vertices"];

                // Deserialize the object into a snake 
                //if the object is found to contain Vertices.
                if (snakeToken != null)
                {
                    Snake newSnake = new Snake();
                    newSnake = JsonConvert.DeserializeObject<Snake>(p);
                    newSnake.headX = newSnake.vertices[newSnake.vertices.Count - 1].X;
                    newSnake.headY = newSnake.vertices[newSnake.vertices.Count - 1].Y;
                    newSnake.Score = world.getSnakeSize(newSnake.vertices);

                    if (newSnake.vertices[0].X == -1 && newSnake.vertices[0].Y == -1 &&
                        newSnake.vertices[1].X == -1 && newSnake.vertices[1].Y == -1)
                    {
                        world.RemoveSnake(newSnake);
                    }
                    else
                    {
                        world.SetSnake(newSnake);
                    }
                }
                else //Deserialize the object into a food item. 
                {
                    Food newFood = new Food();
                    newFood = JsonConvert.DeserializeObject<Food>(p);

                    // check if food needs to be added/updated or removed
                    if (newFood.loc.X == -1 && newFood.loc.Y == -1)
                        world.removeFood(newFood);
                    else
                        world.SetFood(newFood);
                }

                //Console.WriteLine(p);

            }
            // Then remove it from the SocketState's growable buffer
            ss.sb.Clear();
            ss.messageBuffer = new byte[1024];
        }
    }
}

#endif


#if false

// Demo code

public Form1()
{
    InitializeComponent();

    // This an example of registering a method so that it is notified when
    // an event happens.  The SelectionChanged event is declared with a
    // delegate that specifies that all methods that register with it must
    // take a SpreadsheetPanel as its parameter and return nothing.  So we
    // register the displaySelection method below.

    // This could also be done graphically in the designer, as has been
    // demonstrated in class.
    spreadsheetPanel1.SelectionChanged += displaySelection;
    spreadsheetPanel1.SetSelection(2, 3);
}

// Every time the selection changes, this method is called with the
// Spreadsheet as its parameter.  We display the current time in the cell.

private void displaySelection(SpreadsheetPanel ss)
{
    int row, col;
    String value;
    ss.GetSelection(out col, out row);
    ss.GetValue(col, row, out value);
    if (value == "")
    {
        ss.SetValue(col, row, DateTime.Now.ToLocalTime().ToString("T"));
        ss.GetValue(col, row, out value);
        MessageBox.Show("Selection: column " + col + " row " + row + " value " + value);
    }
}

// Deals with the New menu
private void newToolStripMenuItem_Click(object sender, EventArgs e)
{
    // Tell the application context to run the form on the same
    // thread as the other forms.
    DemoApplicationContext.getAppContext().RunForm(new Form1());
}

// Deals with the Close menu
private void closeToolStripMenuItem_Click(object sender, EventArgs e)
{
    Close();
}

}

#endif