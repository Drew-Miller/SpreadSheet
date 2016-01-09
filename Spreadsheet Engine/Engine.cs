//Drew Miller 11382134

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using CptS322;
using System.Xml;
using System.IO;

//implement reference array of cells
//if a cell uses another cell, add it to the reference
//if it did then changes text, remove it
//check for circular
namespace Spreadsheet_Engine
{
    public abstract class Cell : INotifyPropertyChanged
    {
        private int _rowIndex;
        private int _columnIndex;
        protected string _text;
        protected string _value;
        //sets bgcolor to default rgb of white
        protected int[] _BGColor = {255,255,255};
        public event PropertyChangedEventHandler PropertyChanged;

        public Cell(int _c, int _r)
        {
            _rowIndex = _r;
            _columnIndex = _c;
            _text = "";
        }

        public int RowIndex
        {
            get { return _rowIndex; }
        }
        public int ColumnIndex
        {
            get { return _columnIndex; }
        }

        public string Text
        {
            get { return _text; }
            protected set
            {
                if (_text != value)
                {
                    _text = value;
                    OnChanged("Text");
                }
            }
        }
        public string Value
        {
            get { return _value; }
            protected set { _value = value; }
        }
        public int[] BGColor
        {
            get { return _BGColor; }
            protected set
            {
                if(_BGColor != value)
                {
                    _BGColor = value;
                    OnChanged("BGColor");
                }
            }
        }

        public void OnChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    //can expose and construct cells for the spreadsheet to use
    class SpreadSheetCell : Cell
    {
        public SpreadSheetCell(int _c, int _r) : base(_c, _r)
        {
            
        }

        public void setText(string text)
        {
            Text = text;
        }

        public void setValue(string value)
        {
            Value = value;
        }

        public void setBGColor(int[] bgcolor)
        {
            //if we have a valid amount of items in the array
            if (bgcolor.Length == 3)
            {
                BGColor = bgcolor;
            }
        }
    }

    public class SpreadSheet
    {
        //use the references member variable to document which
        //cells in the hashset of cells which reference the
        //cell that the hashset is mapped to in the dictionary
        private Cell[,] _cells;
        private Dictionary<Cell, HashSet<Cell>> _references;
        public event PropertyChangedEventHandler CellPropertyChanged;
        public event PropertyChangedEventHandler StackChange;
        private Stack<UndoRedoCollection> _undos;
        private Stack<UndoRedoCollection> _redos;

        public SpreadSheet(int _c, int _r)
        {
            _cells = new Cell[_c, _r];
            _references = new Dictionary<Cell, HashSet<Cell>>();
            _undos = new Stack<UndoRedoCollection>();
            _redos = new Stack<UndoRedoCollection>();

            for (int i = 0; i < _cells.GetLength(0); i++)
            {
                for (int j = 0; j < _cells.GetLength(1); j++)
                {
                     var myNewCell = new SpreadSheetCell(i, j);
                    _cells[i, j] = myNewCell;
                    _references.Add(_cells[i,j], new HashSet<Cell>());
                    _cells[i, j].PropertyChanged += SpreadSheet_PropertyChanged;
                }
            }
        }

        //resetes cells to default
        public void clear()
        {
            for (int i = 0; i < _cells.GetLength(0); i++)
            {
                for (int j = 0; j < _cells.GetLength(1); j++)
                {
                    setCellText(_cells[i, j], "");
                    setBGColor(_cells[i,j], new int[] {255, 255, 255});
                }
            }
        }

        //resets our spreadsheet completely
        public void reset(int _c, int _r)
        {
            _cells = new Cell[_c, _r];
            _references = new Dictionary<Cell, HashSet<Cell>>();
            _undos = new Stack<UndoRedoCollection>();
            _redos = new Stack<UndoRedoCollection>();

            for (int i = 0; i < _cells.GetLength(0); i++)
            {
                for (int j = 0; j < _cells.GetLength(1); j++)
                {
                    _cells[i, j] = new SpreadSheetCell(i, j);
                    _references.Add(_cells[i, j], new HashSet<Cell>());
                    _cells[i, j].PropertyChanged += SpreadSheet_PropertyChanged;
                }
            }
        }
        
        //returns the cell that had it's property changed.
        //Evaluate text of the cell in this step if text was changed
        private void SpreadSheet_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = CellPropertyChanged;

            if (handler != null)
            {
                if (e.PropertyName == "Text")
                {
                    Cell myCell = (Cell)sender;
                    evaluate(myCell);
                }

                handler(sender, e);
            }
        }

        //event for handling undo redo menu items
        private void SpreadSheet_StackPropertyChange(string name)
        {
            PropertyChangedEventHandler handler = StackChange;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public void setCellText(Cell c, string text)
        {
            SpreadSheetCell myCell = (SpreadSheetCell)c;
            myCell.setText(text);
        }

        public void setBGColor(Cell c, int[] rgb)
        {
            SpreadSheetCell myCell = (SpreadSheetCell)c;
            myCell.setBGColor(rgb);
        }

        //returns a cell from the _cell array at the corresponding value
        public Cell getCell(int _c, int _r)
        {
            if (_c <= (columnCount() - 1) && _c >= 0 && _r >= 0 && _r <= (rowCount() - 1))
            {
                return _cells[_c, _r];
            }
            else
            {
                return null;
            }
        }

        //returns amount of columns in the _cell array
        public int columnCount()
        { return _cells.GetLength(0); }

        //returns amount of rows in _cell array
        public int rowCount()
        { return _cells.GetLength(1); }

        //work on evaluation maybe
        //USE OF EXP TREE HERE
        private void evaluate(Cell c)
        {
            SpreadSheetCell myCell = (SpreadSheetCell)c;
            myCell.setValue("");
            string text = myCell.Text;

            //stores all cells that this cell is referencing
            List<Cell> cellsReferenced = new List<Cell>();

            //iterates through every key in references and if we find our cell in the list of cells
            //that is referencing one of the cells, we add that cell to list of items to remove
            //and remove our cell from them
            foreach (Cell key in _references.Keys)
            {
                if(_references[key].Contains((Cell)myCell))
                {
                    _references[key].Remove((Cell) myCell);
                }
            }

            //evaluates for if the cell is set equal to another cell
            //also collects the cells that are being referenced in our expression
            if (text != "")
            {
                //THIS REGION
                //evaluates the expression inside the cell
                //and creates a list of cells that have been referenced by our cell
                #region
                //if the text needs to be evaluated further, it will start with
                // and '=' sign. Else _value = _text of the cell (at least for
                //this assignment so far)
                if (text[0] == '=')
                {
                    //create a dictionary that stores all variables by cell name and value in the cell.
                    //ex. A1 is 12, adds <A1:12> to dictionary.
                    //ex. A1 is "Hello World". Adds <A1:0> to dictionary.
                    //zero by default.
                    ExpTree evaluator = new ExpTree(myCell.Text.Substring(1), createDict());

                    //if we only have a cell name, set the cell to the value
                    //of the cell name. (should only be in case where
                    //the referenced cell is a text string).
                    if (isCellName(myCell.Text.Substring(1)))
                    {
                        int myRow = 0;
                        int myColumn = 0;

                        if (myCell.Text[1] >= 'A' && myCell.Text[1] <= 'Z')
                        {
                            myColumn = (int)(myCell.Text[1] - 'A');
                        }

                        else if (myCell.Text[1] >= 'a' && myCell.Text[1] <= 'z')
                        {
                            myColumn = (int)(myCell.Text[1] - 'a');
                        }

                        myRow = (int)toDouble(myCell.Text.Substring(2)) - 1;

                        text = _cells[myColumn, myRow].Value;

                        //set reference to the specific cell
                        if (!cellsReferenced.Contains(_cells[myColumn, myRow]))
                        {
                            cellsReferenced.Add(_cells[myColumn, myRow]);
                        }
                    }

                    //if we are operating on the contents, we must
                    //evaluate the text
                    else if (evaluator.isValidExpression())
                    {
                        //get list of cells that we are referencing in the expression
                        //and add to references
                        text = evaluator.Eval().ToString();

                        //get each split in the expression
                        List<string> mySplit = evaluator.splitExpression();
                        
                        foreach(string s in mySplit)
                        {
                            //if we hit a cellname, store it into our referenced cells list
                            if(isCellName(s))
                            {
                                int myRow = 0;
                                int myColumn = 0;

                                if (s[0] >= 'A' && s[0] <= 'Z')
                                {
                                    myColumn = (int)(s[0] - 'A');
                                }

                                else if (s[0] >= 'a' && s[0] <= 'z')
                                {
                                    myColumn = (int)(s[0] - 'a');
                                }

                                myRow = (int)toDouble(s.Substring(1)) - 1;

                                if (!cellsReferenced.Contains(_cells[myColumn, myRow]))
                                {
                                    cellsReferenced.Add(_cells[myColumn, myRow]);
                                }
                            } 
                        }
                    }

                    else
                    {
                        myCell.setValue("!(bad reference)");
                    }
                }
                #endregion
            }

            //check to make sure we dont override if a we prviously wrote the statement of "bad reference"
            if (myCell.Value != "!(bad reference)")
            {
                //check for self reference
                if (!selfReferenceCheck(cellsReferenced, myCell))
                {
                    //sets the value to the evaluated
                    //string of the input text string
                    myCell.setValue(text);

                    //links our cell to every cell that it has referenced.
                    foreach (Cell cell in cellsReferenced)
                    {
                        _references[cell].Add(myCell);
                    }

                    
                    //update all cells that are referencing our cell
                    List<Cell> cellsReferencing = new List<Cell>();

                    //adds all the cells in the hashset of our cell
                    foreach (Cell referencing in _references[myCell])
                    {
                        cellsReferencing.Add(referencing);
                    }
                    
                    if(crefCheck(_references[myCell],myCell))
                    {
                        myCell.setValue("!(circular reference)");

                        //if we do have a circular reference, unlink this cell from referencing all cells
                        foreach (Cell key in _references.Keys)
                        {
                            if (_references[key].Contains((Cell)myCell))
                            {
                                _references[key].Remove((Cell)myCell);
                            }
                        }
                    }

                    else
                    {
                        //evey cell we referenced, fire their event so we will update our cell in the UI
                        foreach (Cell referencing in cellsReferencing)
                        {
                            referencing.OnChanged("Text");
                        }
                    }    
                }

                //if we do have a self reference
                else
                {
                    myCell.setValue("!(self reference)");
                }
            }
        }

        //pushes new undo onto the stack
        public void addUndo(UndoRedoCollection undo)
        {
            _undos.Push(undo);
            SpreadSheet_StackPropertyChange("Undos");
        }

        public UndoRedoCollection getUndo()
        {
            UndoRedoCollection pop = _undos.Pop();
            SpreadSheet_StackPropertyChange("Undos");
            return pop;
        }

        //true if stack is not empty
        public bool hasUndo()
        {
            if (_undos.Count > 0)
            { return true; }

            return false;
        }

        //returns the top value of the stack without removing
        public UndoRedoCollection peekUndo()
        {
            return _undos.Peek();
        }

        //pushes new undo onto the stack
        public void addRedo(UndoRedoCollection redo)
        {
            _redos.Push(redo);
            SpreadSheet_StackPropertyChange("Redos");
        }

        public UndoRedoCollection getRedo()
        {
            UndoRedoCollection pop = _redos.Pop();
            SpreadSheet_StackPropertyChange("Redos");
            return pop;
        }

        //clears the redo stack
        public void clearRedo()
        {
            _redos = new Stack<UndoRedoCollection>();
            SpreadSheet_StackPropertyChange("Redos");
        }

        //true if stack is not empty
        public bool hasRedo()
        {
            if (_redos.Count > 0)
            { return true; }

            return false;
        }

        //returns the top value of the stack without removing
        public UndoRedoCollection peekRedo()
        {
            return _redos.Peek();
        }

        //checks if we do have a circular reference
        //Our cells that have been accessed are stored in the hashset of our cells
        //WORK ON ALGORITHM HERE
        bool crefCheck(HashSet<Cell> children, Cell myCell)
        {
            bool isCR = false;

            //if our children contains the cell, we have a circular reference
            if(children.Contains(myCell))
            {
                isCR = true;
            }

            //if we haven't found a circular reference
            else
            {
                //traverse through the each childrens cell's referencing cells
                foreach (Cell c in children)
                {
                    //we pass that cells child recrursively and match it to our cell
                    if(crefCheck(_references[c], myCell))
                    {
                        //if c's children contain our cell, then we have a circular reference
                        isCR = true;
                    }
                }
            }

            return isCR;
        }

        //checks if the cell references itself
        bool selfReferenceCheck(List<Cell> referenced, Cell myCell)
        {
            foreach(Cell c in referenced)
            {
                if(c==myCell)
                {
                    return true;
                }
            }

            return false;
        }

        //Creates a new dictionary of current values in all spreadsheetcells
        //used when evaluating a change in spreadsheet cell value
        //ESSENTIALLY: Adds every cell as a variable with it's value as the value.
        Dictionary<string, double> createDict()
        {
            Dictionary<string, double> myDict = new Dictionary<string, double>();
            
            //iterates through every item in _cells
            for(int i = 0; i < _cells.GetLength(0); i++)
            {
                for(int j = 0; j < _cells.GetLength(1); j++)
                {
                    //sets the name to something like A1 or Z50
                    string name = ((char)(i + 'A') + (j+1).ToString()).ToString();
                    double value = 0;

                    //if we do have a value in the cell that is a number,
                    //pair the cells name to the value. Else that cell is evaluated as 0
                    if (_cells[i,j].Value != null)
                    {
                        if (isDouble(_cells[i, j].Value))
                        { value = toDouble(_cells[i, j].Value); }
                    }

                    myDict.Add(name, value);
                }
            }

            return myDict;
        }

        //returns true if the string is a valid double
        bool isDouble(string value)
        {
            bool valid = true;

            foreach (char c in value)
            {
                if (!isNumber(c))
                {
                    valid = false;
                }
            }

            return valid;
        }

        //converts a valid double string to an actual double value and returns it
        double toDouble(string value)
        {
            double evaluation = 0;

            //does a secondary check to make sure the value
            //passed is indeed a double
            if (isDouble(value))
            {
                foreach (char c in value)
                {
                    evaluation = (evaluation * 10) + (c - 48);
                }
            }

            return evaluation;
        }

        //returns true if the character is numerical
        bool isNumber(char c)
        {
            return (c >= '0' && c <= '9');
        }

        //returns true if the cellname is within our spread sheet
        bool isCellName(string name)
        {
            bool isName = false;

            if((name[0] >= 'A' && name[0] <= 'Z')||((name[0] >= 'a' && name[0] <= 'z')))
            {
                if(isDouble(name.Substring(1)))
                {
                    double myRow = toDouble(name.Substring(1));

                    if (myRow <= 50 && myRow >= 1)
                    {
                        isName = true;
                    }
                }
            }

            return isName;
        }

        //saving and loading implementations
        //saving to XML

        //WRITING INCORRECTLY.
        //NOT CLOSING AT END OF COLUMN
        public void WriteXml(string path)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            //creates a file at the path instructed with the settings instructed
            FileStream fs = File.Create(@path);
            XmlWriter writer = XmlWriter.Create(fs, settings);

            //starts doc
            writer.WriteStartDocument();

            writer.WriteStartElement("Spreadsheet");
            writer.WriteAttributeString("C", columnCount().ToString());
            writer.WriteAttributeString("R", rowCount().ToString());

            for (int c = 0; c < columnCount(); c++)
            {   
                for (int r = 0; r < rowCount(); r++)
                {
                    Cell myCell = getCell(c, r);

                    if (!isDefaultCell(myCell))
                    {
                        //creates a Cell element
                        writer.WriteStartElement("Cell");

                        //writes column row information
                        //uses user friendly notation (e.g. A1 vs. 00)
                        writer.WriteAttributeString("Col", ((char)(c + 65)).ToString());
                        writer.WriteAttributeString("Row", (r+1).ToString());

                        //writes text
                        if (!isDefaultText(myCell))
                        {
                            writer.WriteElementString("Text", myCell.Text);
                        }

                        //writes value
                        if (!isDefaultValue(myCell))
                        {
                            writer.WriteElementString("Value", myCell.Value);
                        }

                        if (!isDefaultBG(myCell))
                        {
                            writer.WriteStartElement("Color");
                            writer.WriteAttributeString("R", myCell.BGColor[0].ToString());
                            writer.WriteAttributeString("G", myCell.BGColor[1].ToString());
                            writer.WriteAttributeString("B", myCell.BGColor[2].ToString());
                            writer.WriteEndElement();
                        }

                        //closes cell
                        writer.WriteEndElement();
                    }
                }
            }

            //ends spreadsheet
            writer.WriteEndElement();

            //ends our doc
            writer.WriteEndDocument();

            //closing duties
            writer.Flush();
            writer.Close();
            fs.Close();
            fs.Dispose();
        }

        public void LoadXml(string path)
        {
            //clears spreadsheet stacks
            while(hasUndo())
            {
                getUndo();
            }

            while (hasRedo())
            {
                getRedo();
            }

            //settings of reader
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;

            //reader from stream
            XmlReader reader = XmlReader.Create(@path, settings);
            List<string> read = new List<string>();

            //uses to store data about the cell we are currently visiting

            //while we are not at the end of the file and while we are interactive
            while (reader.Read())
            {
                read.Add(reader.Name);

                //if we are at the start of the spreadsheet, read attributes of spreadsheet and apply
                if(reader.NodeType == XmlNodeType.Element)
                {
                    //takes care of the spreasheet attributes of the loaded document
                    if(reader.Name == "Spreadsheet")
                    {
                        double c = 1, r = 1;

                        if (reader.HasAttributes)
                        {
                            while (reader.MoveToNextAttribute())
                            {
                                if (reader.Name == "C")
                                {
                                    c = toDouble(reader.GetAttribute("C"));
                                }

                                if (reader.Name == "R")
                                {
                                    r = toDouble(reader.GetAttribute("R"));
                                }
                            }
                        }

                        reset((int)c, (int)r);
                    }

                    //cell data collection operations
                    else if(reader.Name == "Cell")
                    {
                        //get cell location
                        int c = 0, r = 0;

                        if (reader.HasAttributes)
                        {
                            while (reader.MoveToNextAttribute())
                            {
                                if (reader.Name == "Col")
                                {
                                    //col is in letter formation, convert to int
                                    c = (int)reader.GetAttribute("Col")[0] - 65;
                                }

                                if (reader.Name == "Row")
                                {
                                    //row is at user friendly location. ex A1 vs 00. Subtract one to find the proper index
                                    r = (int)toDouble(reader.GetAttribute("Row")) - 1;
                                }
                            }
                        }

                        //keeps track of whether or not we are in a cell
                        bool inCell = false;
                        
                        //loops thorugh contents of cell
                        while(!reader.EOF && !inCell)
                        {
                            reader.Read();

                            
                            //catches end tag of the cell
                            if(reader.Name == "Cell")
                            {
                                inCell = true;
                            }


                            else if ((reader.Name == "Text")&& (reader.NodeType != XmlNodeType.EndElement))
                            {
                                //gets the text from the document
                                setCellText(getCell(c, r),reader.ReadElementContentAsString());
                            }

                            else if ((reader.Name == "Value")&&(reader.NodeType != XmlNodeType.EndElement))
                            {
                                //sets the value of the cell to whatever it was in the xml document
                                SpreadSheetCell mySSCell = (SpreadSheetCell)getCell(c, r);
                                mySSCell.setValue(reader.ReadElementContentAsString());

                                _cells[c, r] = mySSCell;
                            }

                            else if (reader.Name == "Color")
                            {
                                double red = 255, gr = 255, bl = 255;
                                //get attributes
                                if (reader.HasAttributes)
                                {
                                    while (reader.MoveToNextAttribute())
                                    {
                                        if (reader.Name == "R")
                                        {
                                            red = toDouble(reader.GetAttribute("R"));
                                        }

                                        if (reader.Name == "G")
                                        {
                                            gr = toDouble(reader.GetAttribute("G"));
                                        }

                                        if (reader.Name == "B")
                                        {
                                            bl = toDouble(reader.GetAttribute("B"));
                                        }
                                    }
                                }

                                //set the doubles into an int array
                                int[] rgb = new int[]{ (int)red, (int)gr, (int)bl };

                                //set the cell at c,r to have a bg or int[]{red,bl,gr}
                                SpreadSheetCell mySSCell = (SpreadSheetCell)getCell(c, r);
                                mySSCell.setBGColor(rgb);
                                _cells[c, r] = mySSCell;
                            }
                        }
                    }
                }
            }

            //read out reader
            //System.IO.File.WriteAllLines(@"C:\Users\dmles\Desktop\WriteLines.txt", read);

            //closing duties
            reader.Close();
            reader.Dispose();
        }

        internal bool isDefaultCell(Cell myCell)
        {
            bool isDefault = true;

            if (!(isDefaultText(myCell) && (isDefaultBG(myCell)) && (isDefaultValue(myCell))))
            {
                isDefault = false;
            }

            return isDefault;
        }

        internal bool isDefaultText(Cell myCell)
        {
            if (myCell.Text != "")
            {
                return false;
            }

            return true;
        }

        internal bool isDefaultBG(Cell myCell)
        {
            foreach (int i in myCell.BGColor)
            {
                if (i != 255)
                {
                    return false;
                }
            }

            return true;
        }

        internal bool isDefaultValue(Cell myCell)
        {
            if ((myCell.Value == null)||(myCell.Value == ""))
            {
                return true;
            }

            return false;
        }
    }         

    public class UndoRedoCollection
    {
        //a predefined object that keeps track of information that is created during an event changed property
        List<UndoRedo> _changedProperty = new List<UndoRedo>();

        //creates an undo redo collection of a list of properties changed at one instance
        public UndoRedoCollection(List<UndoRedo> property)
        {
            _changedProperty = property;
        }

        public List<UndoRedo> changedProperty
        {
            get { return _changedProperty; }
        }

        public string name()
        {
            //returns name of the last object in the list
            return _changedProperty[_changedProperty.Count - 1].name();
        }
    }

    //interface for all undo or redo functions
    public interface UndoRedo
    {
        void Execute(SpreadSheet s);
        UndoRedo Reverse(SpreadSheet s);
        string name();
    }

    public class textChange: UndoRedo
    {
        string _prevText;
        Cell myCell;

        public textChange(Cell cell, string text)
        {
            _prevText = text;
            myCell = cell;
        }

        public string Text
        {
            get { return _prevText; }
        }
        public Cell Cell
        {
            get { return myCell; }
        }

        public void Execute(SpreadSheet s)
        {
            s.setCellText(myCell, Text);
        }

        //implement swapping out this data for current data
        public UndoRedo Reverse(SpreadSheet s)
        {
            textChange myReverse = new textChange(Cell, Cell.Text);

            return myReverse;
        }

        public string name()
        {
            return "text change";
        }
    }

    public class bgColorChange : UndoRedo
    {
        int[] color;
        Cell myCell;

        public bgColorChange(Cell cell, int[] rgb)
        {
            color = rgb;
            myCell = cell;
        }

        public int[] Color
        {
            get { return color; }
        }
        public Cell Cell
        {
            get { return myCell; }
        }

        public void Execute(SpreadSheet s)
        {
            s.setBGColor(myCell, color);
        }

        public UndoRedo Reverse(SpreadSheet s)
        {
            bgColorChange myReverse = new bgColorChange(Cell, Cell.BGColor);

            return myReverse;
        }

        public string name()
        {
            return "background color change";
        }
    }
}