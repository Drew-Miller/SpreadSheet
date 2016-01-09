//Drew Miller 11382134

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Spreadsheet_Engine;
using System.Xml;

namespace Spreadsheet_DMiller
{
    public partial class Form1 : Form
    {
        SpreadSheet _spreadSheet;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //short cut handler
            KeyPreview = true;
            KeyDown += new KeyEventHandler(Form1_KeyDown);

            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToOrderColumns = false;
            dataGridView1.RowHeadersWidth = 50;
            int columns = 26;
            int rows = 50;

            addColumns(columns);
            addRows(rows);

            _spreadSheet = new SpreadSheet(columns, rows);

            _spreadSheet.CellPropertyChanged += _spreadSheet_CellPropertyChanged;
            _spreadSheet.StackChange += _spreadSheet_StackChange;

            undoToolStripMenuItem.Enabled = false;
            redoToolStripMenuItem.Enabled = false;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //performs undo fucntion
            if (e.Control && e.KeyCode == Keys.Z)
            {
                undoOperation();
            }

            //performs redo fucntion
            if (e.Control && e.KeyCode == Keys.A)
            {
                redoOperation();
            }
        }

        private void _spreadSheet_StackChange(object sender, PropertyChangedEventArgs e)
        {
            if(sender is SpreadSheet)
            {
                SpreadSheet mySpread = (SpreadSheet)sender;

                if(e.PropertyName == "Undos")
                {
                    if(mySpread.hasUndo())
                    {
                        undoToolStripMenuItem.Enabled = true;
                        undoToolStripMenuItem.Text = "Undo " + mySpread.peekUndo().name() + " (Ctrl-Z)";
                    }

                    else
                    {
                        undoToolStripMenuItem.Enabled = false;
                        undoToolStripMenuItem.Text = "Undo (Ctrl-Z)";
                    }
                }

                if(e.PropertyName == "Redos")
                {
                    if(mySpread.hasRedo())
                    {
                        redoToolStripMenuItem.Enabled = true;
                        redoToolStripMenuItem.Text = "Redo " + mySpread.peekRedo().name() + " Ctrl-A)";
                    }

                    else
                    {
                        redoToolStripMenuItem.Enabled = false;
                        redoToolStripMenuItem.Text = "Redo (Ctrl-A)";
                    }
                }
            }
        }

        private void _spreadSheet_CellPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Cell myCell = (Cell)sender;

            if (e.PropertyName == "Text")
            {
                dataGridView1[myCell.ColumnIndex, myCell.RowIndex].Value = myCell.Value;
            }

            if (e.PropertyName == "BGColor")
            {
                dataGridView1[myCell.ColumnIndex, myCell.RowIndex].Style.BackColor = System.Drawing.Color.FromArgb(myCell.BGColor[0], myCell.BGColor[1], myCell.BGColor[2]);
            }
        }

        private void addColumns(int c)
        {
            for (int i = 0; i < c; i++)
            {
                dataGridView1.Columns.Add(((char)(i + 65)).ToString(), ((char)(i + 65)).ToString());
            }
        }

        private void addRows(int r)
        {
            for (int i = 0; i < r; i++)
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();
                dataGridView1.Rows[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            //use tag to store values in the spraedsheet and to access them later. ex. dgv.Tag
            if (sender is DataGridView)
            {
                //casts values and gets current cell values
                var dgv = (DataGridView)sender;
                Cell myCell = _spreadSheet.getCell(dgv.CurrentCell.ColumnIndex, dgv.CurrentCell.RowIndex);

                dgv.CurrentCell.Value = myCell.Text;
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (sender is DataGridView)
            {
                var dgv = (DataGridView)sender;
                Cell myCell = _spreadSheet.getCell(dgv.CurrentCell.ColumnIndex, dgv.CurrentCell.RowIndex);

                //if the cells value ends as null, set the value to "".
                if (dgv.CurrentCell.Value == null)
                {
                    dgv.CurrentCell.Value = "";
                }

                //HANDLES THE UNDOING OF THIS FUNCTION
                if (dgv.CurrentCell.Value.ToString() != myCell.Text)
                {
                    //creates an undo redo collection of a text chage object that contains this specific cell
                    //and containsn the text of the cell before it has been altered
                    List<UndoRedo> changedProperties = new List<UndoRedo>();
                    changedProperties.Add(new textChange(myCell, myCell.Text));
                    UndoRedoCollection text = new UndoRedoCollection(changedProperties);

                    //adds it to the stack
                    _spreadSheet.addUndo(text);

                    //clears redo stack
                    _spreadSheet.clearRedo();
                }

                //at the end of edit, set myCell to the value ""
                _spreadSheet.setCellText(myCell, dgv.CurrentCell.Value.ToString());

                //Set the value of the cell to the evaluated myCell value.
                dgv.CurrentCell.Value = myCell.Value;
            }
        }

        private void changeCellBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.ShowDialog();
            System.Drawing.Color color = cd.Color;

            //testing the three int array format
            int[] rgb = {color.R, color.G, color.B };

            //list of the bgcolors that the cells changed from
            List<UndoRedo> changedProperties = new List<UndoRedo>();

            foreach(DataGridViewCell c in dataGridView1.SelectedCells)
            {
                Cell myCell = _spreadSheet.getCell(c.ColumnIndex, c.RowIndex);

                //if one of the cells has a change of background color, create an undo function
                //TEST MORE
                if(myCell.BGColor != rgb)
                {
                    //create a new bgcolorchange that tells us the specific color of the cell we are gonna change colors
                    changedProperties.Add(new bgColorChange(myCell, myCell.BGColor));
                }

                //set the bgColor in the cell
                _spreadSheet.setBGColor(myCell, rgb);
            }

            //HANDLES UNDO FUNCTION
            if(changedProperties.Count > 0)
            {
                _spreadSheet.addUndo(new UndoRedoCollection(changedProperties));

                //clears redo stack
                _spreadSheet.clearRedo();
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            undoOperation();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            redoOperation();
        }

        private void undoOperation()
        {
            //if the stack is not empty
            if (_spreadSheet.hasUndo())
            {
                UndoRedoCollection myUndo = _spreadSheet.getUndo();
                List<UndoRedo> myRedoList = new List<UndoRedo>();

                //for every object in the changedProperty list
                foreach (UndoRedo property in myUndo.changedProperty)
                {
                    //undo their function
                    myRedoList.Add(property.Reverse(_spreadSheet));
                    property.Execute(_spreadSheet);
                }

                UndoRedoCollection myRedo = new UndoRedoCollection(myRedoList);
                _spreadSheet.addRedo(myRedo);
            }
        }

        private void redoOperation()
        {
            //if the stack is not empty
            if (_spreadSheet.hasRedo())
            {
                UndoRedoCollection myRedo = _spreadSheet.getRedo();
                List<UndoRedo> myUndoList = new List<UndoRedo>();

                //for every object in the changedProperty list
                foreach (UndoRedo property in myRedo.changedProperty)
                {
                    //undo their function
                    myUndoList.Add(property.Reverse(_spreadSheet));
                    property.Execute(_spreadSheet);
                }

                UndoRedoCollection myUndo = new UndoRedoCollection(myUndoList);
                _spreadSheet.addUndo(myUndo);
            }
        }

        private void saveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "XML Files (*.xml)|*.xml";

            //if our input is valid
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //pass in our input as the file pathname
                _spreadSheet.WriteXml(sfd.FileName.ToString());
            }
        }

        private void loadFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "XML Files (*.xml)|*.xml";

            //if our input is valid
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //pass in our input as the file pathname
                _spreadSheet.LoadXml(ofd.FileName.ToString());

                //resets our UI
                dataGridView1.RowCount = 0;
                dataGridView1.ColumnCount = 0;

                //adds columns and rows back to our ui
                addColumns(_spreadSheet.columnCount());
                addRows(_spreadSheet.rowCount());

                //update all dgv cell values adn bgcolor
                for(int i = 0; i < dataGridView1.ColumnCount; i++)
                {
                    for (int j = 0; j < dataGridView1.RowCount; j++)
                    {
                        //if we have a cell that does not have a null value
                        if (_spreadSheet.getCell(i, j).Value != null)
                        {
                            //update the UI
                            dataGridView1.Rows[j].Cells[i].Value = _spreadSheet.getCell(i, j).Value.ToString();
                        }

                        //if we have a cell that does not have a default bg color
                        if (_spreadSheet.getCell(i, j).BGColor != new int[] { })
                        {
                            //update the UI
                            int[] myRGB = _spreadSheet.getCell(i, j).BGColor;
                            dataGridView1[i, j].Style.BackColor = System.Drawing.Color.FromArgb(myRGB[0], myRGB[1], myRGB[2]);
                        }
                    }
                }
            }
        }
    }
}
