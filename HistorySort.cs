using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace versusSortApp
{
    class HistorySort
    {
        #region Properties
        public string Name { get; }
        public List<string> BeatList { get; set; }
        #endregion

        #region Constructions
        /// <summary>
        /// Default constructor.
        /// </summary>
        public HistorySort()
        {

        }

        /// <summary>
        /// Non-default constructor. Sets the element name and an empty Beat List.
        /// </summary>
        /// <param name="inName">The element name.</param>
        public HistorySort(string inName)
        {
            Name = inName;
            BeatList = new List<string>();
        }
        #endregion

        #region Methods
        public static bool CheckHistory(List<HistorySort> historyList, int indexOfPotWinner, int indexOfPotLoser)
        {
            return historyList[indexOfPotWinner].BeatList.Contains(historyList[indexOfPotLoser].Name);
        }

        public static List<HistorySort> WriteToHistory(List<HistorySort> historyList, int indexOfWinner, int indexOfLoser)
        {
            List<HistorySort> updatedHistory = new List<HistorySort>();

            //newEntries: List of losers to be added newly added to the winner beatlists.
            List<string> newEntries = new List<string>();
            //Add loser to newEntries.
            newEntries.Add(historyList[indexOfLoser].Name);
            //Add all items on loser's beatlist to newEntries.
            foreach (string item in historyList[indexOfLoser].BeatList)
            {
                newEntries.Add(item);
            }

            //receiveEntries: list of the indices of all winners which will be gaining newEntries
            List<int> receiveEntries = new List<int>();
            //Add winner index to receiveEntries.
            receiveEntries.Add(indexOfWinner);

            //Check the history of each item to see if the winner is present.
            foreach (HistorySort element in historyList)
            {
                //for each kvp, if the winner is on the win list of
                if (element.BeatList.Contains(historyList[indexOfWinner].Name))
                {
                    receiveEntries.Add(historyList.IndexOf(element));
                }
            }

            //Takes the list of items receiving new entries and adds the new entries to their list
            foreach (int winnerIndex in receiveEntries)
            {
                foreach (string value in newEntries)
                {
                    //if the values list for key does not contain the item, add it.
                    if (!historyList[winnerIndex].BeatList.Contains(value))
                    {
                        historyList[winnerIndex].BeatList.Add(value);
                    }
                }
            }
            updatedHistory = historyList;
            return updatedHistory;
        }
        #endregion

        public override string ToString()
        {
            string beatListString = "";
            foreach (string element in this.BeatList)
            {
                beatListString += element + ";";
            }
            return Name + " BL: " + beatListString;
        }
    }
}
