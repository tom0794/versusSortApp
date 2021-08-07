using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace versusSortApp
{
    public partial class VersusGUI : Form
    {
        #region Global Variables
        private const int EVENT_LINES = 30;
        private const int SUBSET_SIZE = 5;
        private int indexPosOList = 0;
        // File variables
        private string filename = "";
        private string saveDirectory = "";
        private string saveFilename = "";
        private StreamReader reader = null;
        private StreamWriter writer = null;
        // List variables
        private List<string> originalList = new List<string>();
        private List<string> leftZipper = new List<string>();
        private List<string> rightZipper = new List<string>();
        private List<string> completeZipper = new List<string>();
        private List<HistorySort> historyList = new List<HistorySort>();
        // Loop control variables
        private int indexChoice1 = 0;
        private int indexChoice2 = 1;
        // State variables
        private bool historySortActive = false;
        private bool zipperSortActive = false;
        #endregion


        public VersusGUI()
        {
            InitializeComponent();
        }

        #region Text Output
        /// <summary>
        /// Consumes a string and displays it in txtOutput. If txtOutput contains the max number of lines, the oldest
        /// line is removed.
        /// </summary>
        /// <param name="newLine">The new string to be displayed.</param>
        private void UpdateEventLog(string newLine)
        {
            if (txtOutput.Lines.Length > EVENT_LINES)
            {
                List<string> eventLogLines = new List<string>();
                for (int i = 1; i < txtOutput.Lines.Length; i++)
                {
                    if (txtOutput.Lines[i] != "")
                    {
                        eventLogLines.Add(txtOutput.Lines[i]);
                    }
                }
                eventLogLines.Add(newLine);
                txtOutput.Clear();
                for (int i = eventLogLines.Count - 1; i > -1; i--)
                {
                    txtOutput.Text += eventLogLines[i] + "\n";
                }
            }
            else
            {
                txtOutput.Text = newLine + "\n" + txtOutput.Text;
            }
        }
        #endregion

        #region File Handling
        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog getFileName = new OpenFileDialog
            {
                DefaultExt = "txt",
                Filter = "text file (*.txt)|*.txt|All files (*.*)|*.*",
                AddExtension = true,
                Title = "Select a list to sort (.txt file)"
                //InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (getFileName.ShowDialog().ToString() == "OK")
            {
                filename = @getFileName.FileName;
                saveDirectory = @filename.Substring(0, filename.LastIndexOf('\\') + 1);
                saveFilename = @filename.Substring(filename.LastIndexOf('\\') + 1, (filename.Length - 5) - filename.LastIndexOf('\\')) + "-Sorted.txt";
                UpdateEventLog("File loaded from path " + filename);
                UpdateEventLog("Results will be saved at " + saveDirectory + saveFilename);
                btnStart.Enabled = true;
            }
            else
            {
                UpdateEventLog("Please open a file to begin.");
            }
            getFileName.Dispose();
        }

        #endregion

        #region Buttons
        private void btnTest_Click(object sender, EventArgs e)
        {
            UpdateEventLog($"olist: {indexPosOList}");

        }

        /// <summary>
        /// Populates the original list using the loaded file. Call director method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            // Use the file to create the original list.
            using (reader = new StreamReader(filename))
            {
                while (!reader.EndOfStream)
                {
                    originalList.Add(reader.ReadLine());
                }
            }
            Director();
        }

        private void SetUpUserInput()
        {
            btnChoice1.Enabled = true;
            btnChoice2.Enabled = true;
            if (historySortActive)
            {
                btnChoice1.Text = historyList[indexChoice1].Name;
                btnChoice2.Text = historyList[indexChoice2].Name;
            }
            else if (zipperSortActive)
            {
                btnChoice1.Text = leftZipper[indexChoice1];
                btnChoice2.Text = rightZipper[indexChoice2];
            }
        }

        private void btnChoice1_Click(object sender, EventArgs e)
        {
            btnChoice1.Enabled = false;
            btnChoice2.Enabled = false;
            if (historySortActive)
            {
                HistoryResolution(indexChoice1, indexChoice2);
            }
            else if (zipperSortActive)
            {
                ZipperResolution(leftZipperWinner: true);
            }
        }

        private void btnChoice2_Click(object sender, EventArgs e)
        {
            btnChoice1.Enabled = false;
            btnChoice2.Enabled = false;
            if (historySortActive)
            {
                HistoryResolution(indexChoice2, indexChoice1);
            }
            else if (zipperSortActive)
            {
                ZipperResolution(leftZipperWinner: false);
            }
        }
        #endregion


        /// <summary>
        /// Return a subset of the original list.
        /// </summary>
        /// <returns>A list of strings that is a subset of the original list.</returns>
        private List<string> GetSubset()
        {
            List<string> subset = new List<string>();
            while (subset.Count < SUBSET_SIZE && (indexPosOList < originalList.Count()))
            {
                subset.Add(originalList[indexPosOList]);
                indexPosOList++;
            }
            return subset;
        }

        #region Director
        private void Director()
        {
            // If the right zipper is empty, fill it
            if (rightZipper.Count == 0 && indexPosOList != originalList.Count)
            {
                List<string> listSubset = GetSubset();
                if (listSubset.Count == 1)
                {
                    rightZipper.Add(listSubset[0]);
                    Director();
                }
                else
                {
                    historySortActive = true;
                    indexChoice1 = 0;
                    indexChoice2 = 1;
                    historyList.Clear();
                    foreach (string str in listSubset)
                    {
                        historyList.Add(new HistorySort(str));
                    }
                    DoHistorySort();
                }
            }
            // if the left zipper is empty and the right is not, move the contents of the right to the left
            else if (leftZipper.Count == 0 && rightZipper.Count != 0)
            {
                foreach (string str in rightZipper)
                {
                    leftZipper.Add(str);
                }
                rightZipper.Clear();
                Director();
            }
            // if both are full, combine and place results in the left zipper
            else if (leftZipper.Count != 0 && rightZipper.Count != 0)
            {
                indexChoice1 = 0;
                indexChoice2 = 0;
                completeZipper.Clear();
                zipperSortActive = true;
                DoZipperSort();
            }
            // when there is nothing left to sort
            else if (indexPosOList == originalList.Count)
            {
                using (writer = new StreamWriter(saveDirectory + saveFilename, append: false))
                {
                    for (int i = 0; i <= leftZipper.Count - 1; i++)
                    {
                        if (i + 1 < 10)
                        {
                            writer.WriteLine($" {i + 1}. {leftZipper[i]}");
                        }
                        else
                        {
                            writer.WriteLine($"{i + 1}. {leftZipper[i]}");
                        }
                    }
                }
                UpdateEventLog($"Process complete. Results saved to {saveFilename}.");
                indexPosOList = 0;
                originalList.Clear();
                rightZipper.Clear();
                leftZipper.Clear();
                btnStart.Enabled = false;
            }
        }

        #endregion



        #region History Sort
        // startup (director will set it up)

        // repeatedly call
        private void DoHistorySort()
        {
            if (HistorySort.CheckHistory(historyList, indexChoice1, indexChoice2))
            {
                HistoryResolution(indexChoice1, indexChoice2);
            }
            else if (HistorySort.CheckHistory(historyList, indexChoice2, indexChoice1))
            {
                HistoryResolution(indexChoice2, indexChoice1);
            }
            else
            {
                SetUpUserInput();
            }
        }

        // resolution that increments variables
        private void HistoryResolution(int winner, int loser)
        {
            historyList = HistorySort.WriteToHistory(historyList, winner, loser);
            //UpdateEventLog($"{historyList[winner].Name} beat {historyList[loser].Name}!");

            IncrementIndices();
            if (historySortActive)
            {
                DoHistorySort();
            }
            else
            {
                int lcv = historyList.Count - 1;
                while (lcv >= 0)
                {
                    foreach (HistorySort element in historyList)
                    {
                        if (element.BeatList.Count == lcv)
                        {
                            rightZipper.Add(element.Name);
                        }
                    }
                    lcv--;
                }
                Director();
            }
        }

        private void IncrementIndices()
        {
            if (indexChoice2 == (historyList.Count - 1))
            {
                if (indexChoice1 == (historyList.Count - 2))
                {
                    historySortActive = false;
                }
                else
                {
                    indexChoice1++;
                    indexChoice2 = indexChoice1 + 1;
                }
            }
            else
            {
                indexChoice2++;
            }
        }
        #endregion

        #region Zipper Sort
        private void DoZipperSort()
        {
            SetUpUserInput();
        }

        private void ZipperResolution(bool leftZipperWinner)
        {
            // if the left zipper choice won
            if (leftZipperWinner)
            {
                completeZipper.Add(leftZipper[indexChoice1]);
                // if there is nothing left on the left list add the remainder of the right list
                if (indexChoice1 == leftZipper.Count - 1)
                {
                    while (indexChoice2 <= rightZipper.Count - 1)
                    {
                        completeZipper.Add(rightZipper[indexChoice2]);
                        indexChoice2++;
                    }
                    FinishZipperSort();
                }
                else
                {
                    indexChoice1++;
                    DoZipperSort();
                }
            }
            // if the right zipper choice won
            else 
            {
                completeZipper.Add(rightZipper[indexChoice2]);
                // if there is nothing left on the right list add the remainder of the left list
                if (indexChoice2 == rightZipper.Count - 1)
                {
                    while (indexChoice1 <= leftZipper.Count - 1)
                    {
                        completeZipper.Add(leftZipper[indexChoice1]);
                        indexChoice1++;
                    }
                    FinishZipperSort();
                }
                else
                {
                    indexChoice2++;
                    DoZipperSort();
                }
            }
        }

        private void FinishZipperSort()
        {
            leftZipper.Clear();
            rightZipper.Clear();
            foreach (string str in completeZipper)
            {
                leftZipper.Add(str);
            }
            zipperSortActive = false;
            Director();
        }
        #endregion

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
